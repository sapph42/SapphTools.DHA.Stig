using ExcelDataReader;
using System.Data;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;
using System.Text.RegularExpressions;
using DataTable = System.Data.DataTable;
using StigRule = SapphTools.DHA.Stig.Common.Classes.Rule;

namespace SapphTools.DHA.Stig.Remediator.Forms;
public partial class Main : Form {
    private bool _supressUiCascade = false;
    private static readonly string localPath = Path.GetDirectoryName(Environment.ProcessPath!)!;
    private readonly BindingSource catalogSource = [];
    private readonly BindingSource findingSource = [];
    private readonly BindingSource resultsSource = [];
    private readonly SettingContext AppContext;
    protected DataTable CatalogTable = new();
    protected DataTable FindingsTable = new();
    protected DataTable ResultsTable = new();
    protected DataView FilteredFindings;
    protected Catalog? StigCatalog;
    public Main() {
        InitializeComponent();
        AppContext = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ?
            SettingContext.Administrator :
            SettingContext.None;
        AppContext = WindowsIdentity.GetCurrent().IsSystem ?
            SettingContext.System :
            AppContext;
        CurrentContext.Text = AppContext.ToString();

        InitializeCatalogTable();
        InitializeFindingsTable();
        InitializeCatalog();
        InitializeFindings();
        FilteredFindings = new(FindingsTable) {
            RowFilter = "RuleAvailable = true"
        };
        if (!string.IsNullOrEmpty(Settings.Default.CatalogPath)
                && File.Exists(Settings.Default.CatalogPath)) {
            try {
                StigCatalog = JsonSerializer.Deserialize<Catalog>(
                    File.ReadAllText(Settings.Default.CatalogPath),
                    GlobalConstants.JsonSerializerOptions
                );
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }
        Findings.CellFormatting += (s, e) => {
            if (e.RowIndex < 0) {
                return;
            }
            if (Findings.Columns[e.ColumnIndex].Name == "Remediate") {
                if (Findings.Rows[e.RowIndex].Cells["RuleAvailable"].Value is bool and false
                        || Findings.Rows[e.RowIndex].Cells["ProperContext"].Value is bool and false) {
                    Findings.Rows[e.RowIndex].Cells["Remediate"].ReadOnly = true;
                }
                if (AppContext >= SettingContext.System) {
                    string rowHost = (string)Findings.Rows[e.RowIndex].Cells["Hostname"].Value;
                    if (!rowHost.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)) {
                        Findings.Rows[e.RowIndex].Cells["Remediate"].ReadOnly = true;
                    }
                }
            }
        };
        Findings.CellPainting += (s, e) => {
            if (e.RowIndex < 0 ||
                    Findings.Columns[e.ColumnIndex].Name != "Remediate" ||
                    Findings.Rows[e.RowIndex].Cells["ProperContext"].Value is not bool ||
                    Findings.Rows[e.RowIndex].Cells["ProperContext"].Value is bool proper and true) {
                return;
            }
            e.PaintBackground(e.ClipBounds, true);
            e.Paint(
                e.ClipBounds,
                DataGridViewPaintParts.Border |
                    DataGridViewPaintParts.Focus |
                    DataGridViewPaintParts.SelectionBackground
            );
            Size size = CheckBoxRenderer.GetGlyphSize(e.Graphics!, System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled);
            Point loc = new(
                e.CellBounds.Left + (e.CellBounds.Width - size.Width) / 2,
                e.CellBounds.Top + (e.CellBounds.Height - size.Height) / 2
            );
            CheckBoxRenderer.DrawCheckBox(e.Graphics!, loc, System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled);
            e.Handled = true;
        };
    }

    private void Main_Load(object sender, EventArgs e) {
        CatalogPath.Text = Settings.Default.CatalogPath;
        if (StigCatalog is not null) {
            foreach (StigRule rule in StigCatalog.Rules) {
                CatalogTable.Rows.Add(
                    rule.RuleId, 
                    rule.Description, 
                    rule.Settings.Any(s => s.Dangerous), 
                    rule.Settings.Max(s => s.RequiredContext),
                    rule.Clone()
                );
            }
            RuleCount.Text = $"Catalog loaded. {CatalogTable.Rows.Count} plugins available for remediation.";
        } else {
            RuleCount.Text = $"Catalog not loaded. Remediation unavailable.";
        }
        Catalog.DataSource = catalogSource;
        Findings.DataSource = findingSource;
    }
    private void Main_FormClosing(object sender, FormClosingEventArgs e) {
        if (!string.IsNullOrWhiteSpace(CatalogPath.Text) && File.Exists(CatalogPath.Text)) {
            Settings.Default.CatalogPath = CatalogPath.Text;
            Settings.Default.Save();
        }
    }

    private void EditCatalogMenu_Click(object sender, EventArgs e) {
        CatalogEditor catalogEditor;
        if (StigCatalog is null) {
            catalogEditor = new();
        } else {
            catalogEditor = new(StigCatalog);
        }
        if (catalogEditor.ShowDialog() == DialogResult.OK && catalogEditor.Catalog is not null) {
            StigCatalog = catalogEditor.Catalog.Clone();
            CatalogTable.Rows.Clear();
            foreach (StigRule rule in StigCatalog.Rules) {
                CatalogTable.Rows.Add(rule.RuleId, rule.Description, rule.Settings.Any(s => s.Dangerous), rule.Clone());
            }
            catalogSource.ResetBindings(false);
            CheckForAvailability();
            try {
                string json = JsonSerializer.Serialize(StigCatalog, GlobalConstants.JsonSerializerOptions);
                File.WriteAllText(CatalogPath.Text, json);
#if DEBUG
                File.WriteAllText(@"C:\Users\n\OD\repos\SapphTools\DHA\Stig\Remediator\catalog.json", json);
#endif
            } catch {
                MessageBox.Show("Your changes to the catalog could not be saved, but will remain active for this session.");
            }
        }
    }
    private void Filter_CheckedChanged(object sender, EventArgs e) {
        if (_supressUiCascade) { return; }
        _supressUiCascade = true;
        FilterFindingsMenu.Checked = Filter.Checked;
        if (Filter.Checked) {
            findingSource.DataSource = FilteredFindings;
        } else {
            findingSource.DataSource = FindingsTable;
        }
        findingSource.ResetBindings(false);
        _supressUiCascade = false;
    }
    private void FilterFindingsMenu_Click(object sender, EventArgs e) {
        if (_supressUiCascade) { return; }
        _supressUiCascade = true;
        Filter.Checked = FilterFindingsMenu.Checked;
        _supressUiCascade = false;
    }
    private void LoadFindings_Click(object sender, EventArgs e) {
        int? count = null;
        List<string> sheetNames = [
            "In Boundary Servers",
            "Out of Boundary Servers",
            "STIG Vulns InBound",
            "STIG Vulns OoB"
        ];
        OpenFileDialog ofd = new() {
            AddExtension = true,
            AddToRecent = false,
            AutoUpgradeEnabled = true,
            CheckFileExists = true,
            DefaultExt = "xlsx",
            DereferenceLinks = true,
            Filter = "Excel files|*.xlsx;*.xlsm;*.xlsb",
            InitialDirectory = @"\\eamcfs01\dept$\_EAMC_DATA\workgroup\IMD\System Administration\_AdminApps\Stig Remediator",
            Multiselect = false,
            OkRequiresInteraction = true,
            Title = "Select ACAS Vulnerability Report"
        };
        if (ofd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName) && File.Exists(ofd.FileName)) {
            try {
                Cursor = Cursors.WaitCursor;
                InitializeFindingsTable();
                InitializeFindings();
                using FileStream stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                do {
                    if (!sheetNames.Any(n => n.Equals(reader.Name, StringComparison.OrdinalIgnoreCase))) {
                        continue;
                    }
                    reader.Read();
                    Dictionary<string, int> columns = new(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++) {
                        string? name = reader.GetValue(i)?.ToString();
                        if (!string.IsNullOrWhiteSpace(name)) {
                            columns[name] = i;
                        }
                    }
                    while (reader.Read()) {
                        string? ruleid = reader.GetValue(columns["Plugin"])?.ToString();
                        string? desc = reader.GetValue(columns["Plugin Name"])?.ToString();
                        string? host = reader.GetValue(columns["Nickname"])?.ToString();
                        if (ruleid is not null && desc is not null && host is not null) {
                            Match match = StigNamePattern().Match(desc);
                            if (match.Success) {
                                desc = match.Groups[1].Value;
                            }
                            bool ruleAvail = false;
                            bool drakeMallard = false;
                            bool context = false;
                            if (CatalogTable.Rows.Find(ruleid) is DataRow ruleRow) {
                                ruleAvail = true;
                                drakeMallard = (bool)ruleRow["Dangerous"];
                                context = (SettingContext)(int)ruleRow["Context"] <= AppContext;
                                if (AppContext >= SettingContext.System) {
                                    if (!host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)) {
                                        context = false;
                                    }
                                }
                                count ??= 0;
                            }
                            bool autoRem = ruleAvail && !drakeMallard && context;
                            _ = FindingsTable.Rows.Add(autoRem, ruleid, desc, host, drakeMallard, ruleAvail, context);
                            if (ruleAvail && context) {
                                count++;
                            }
                        }
                    }
                } while (reader.NextResult());
                if (Filter.Checked || FilterFindingsMenu.Checked) {
                    Filter_CheckedChanged(new(), EventArgs.Empty);
                }
            } catch (Exception ex) {
                MessageBox.Show(
                    $"Exception thrown trying to open or parse vulnerability report.\r\n{ex.Message}",
                    "Exception!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
            } finally {
                findingSource.ResetBindings(true);
                if (count is null) {
                    FindingsCount.Text = "Findings not loaded. Remediation unavailable.";
                } else {
                    FindingsCount.Text = $"Findings loaded. {count} findings available for remediation.";
                }
                Cursor = Cursors.Default;
            }
        }
    }
    private void LogsMenu_Click(object sender, EventArgs e) {
        Logs logs = new() {
            CatalogTable = CatalogTable.Copy()
        };
        logs.Show();
    }
    private void Remediate_Click(object sender, EventArgs e) {
        if (!string.IsNullOrWhiteSpace(CatalogPath.Text) && File.Exists(CatalogPath.Text)) {
            Settings.Default.CatalogPath = CatalogPath.Text;
            Settings.Default.Save();
        }
        Guid batch = Guid.NewGuid();
        InitializeResultsTable();
        foreach (DataGridViewRow row in Findings.Rows.Cast<DataGridViewRow>()) {
            if (row.Cells["Remediate"].Value is bool and true &&
                    row.Cells["Plugin"].Value is string plugin &&
                    row.Cells["Hostname"].Value is string hostname &&
                    CatalogTable.Rows.Find(plugin) is DataRow drRow &&
                    drRow["Rule"] is StigRule rule) {
                Debug.WriteLine($"Calling Executefor rule {rule.RuleId} on {hostname} from row {row.Index}");
                List<RemediationActionResult> res = 
                    Classes.Remediators.Remediator.Execute(rule, batch, hostname, WhatIf.Checked);
                ActionResult agg = ActionResult.NoActionTaken;
                string message = string.Empty;
                if (res.Any(r => !r.IsSuccess)) {
                    agg = ActionResult.ActionFailure;
                    message = res.First(r => !r.IsSuccess).Message;
                } else if (!res.All(r => r.Value is null)) {
                    agg = res.Max(r => r.Value?.Result ?? ActionResult.NoActionTaken);
                    message = res.FirstOrDefault(r => r.Value?.FailureMessage is not null)?.Value?.FailureMessage ?? string.Empty;
                }
                ResultsTable.Rows.Add(plugin, hostname, agg, message);
            }
        }
        InitializeFindingsForResults();
    }
    private void SetCatalogPathMenu_Click(object sender, EventArgs e) {
        OpenFileDialog ofd = new() {
            AddExtension = true,
            AddToRecent = false,
            AutoUpgradeEnabled = true,
            CheckFileExists = false,
            DefaultExt = "json",
            DereferenceLinks = true,
            Filter = "JSON files |*.json",
            InitialDirectory = @"\\eamcfs01\dept$\_EAMC_DATA\workgroup\IMD\System Administration\_AdminApps\Stig Remediator",
            Multiselect = false,
            OkRequiresInteraction = true,
            Title = "Select STIG Remediator Catalog"
        };
        if (ofd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName)) {
            if (!File.Exists(ofd.FileName)) {
                DialogResult res =
                    MessageBox.Show("No catalog exists at that path. Create a new catalog?", "New Catalog", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes) {
                    CatalogPath.Text = ofd.FileName;
                    CatalogTable.Rows.Clear();
                }
                return;
            }
            try {
                string json = File.ReadAllText(ofd.FileName);
                Catalog? catalog = JsonSerializer.Deserialize<Catalog>(
                    json,
                    GlobalConstants.JsonSerializerOptions
                );
                if (catalog is not null && catalog.Rules.Count > 0) {
                    CatalogPath.Text = ofd.FileName;
                    StigCatalog = catalog;
                    CatalogTable.Rows.Clear();
                    foreach (StigRule rule in StigCatalog.Rules) {
                        CatalogTable.Rows.Add(rule.RuleId, rule.Description, rule.Settings.Any(r => r.Dangerous), rule.Clone());
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(
                    $"Exception thrown trying to open or parse catalog file.\r\n{ex.Message}",
                    "Exception!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
            }
        }
        if (Findings.Rows.Count > 1) {
            CheckForAvailability();
        }
    }
    private void SimulationModeMenu_Click(object sender, EventArgs e) {
        if (_supressUiCascade) { return; }
        _supressUiCascade = true;
        WhatIf.Checked = SimulationModeMenu.Checked;
        _supressUiCascade = false;
    }
    private void WhatIf_CheckedChanged(object sender, EventArgs e) {
        if (_supressUiCascade) { return; }
        _supressUiCascade = true;
        SimulationModeMenu.Checked = WhatIf.Checked;
        _supressUiCascade = false;
    }

    private void CheckForAvailability() {
        if (CatalogTable.Rows.Count == 0) {
            RuleCount.Text = $"Catalog not loaded. Remediation unavailable.";
        }
        if (Findings.Rows.Count == 0) {
            FindingsCount.Text = "Findings not loaded. Remediation unavailable.";
        }
        if (CatalogTable.Rows.Count == 0 || Findings.Rows.Count == 0) {
            return;
        }
        int count = 0;
        foreach (DataRow findingsRow in FindingsTable.Rows.Cast<DataRow>()) {
            bool ruleAvail = false;
            bool drakeMallard = false;
            if (CatalogTable.Rows.Find(findingsRow["Plugin"]) is DataRow ruleRow) {
                ruleAvail = true;
                drakeMallard = (bool)ruleRow["Dangerous"];
                count++;
            }
            findingsRow["Remediate"] = ruleAvail & (bool)findingsRow["Remediate"] & !drakeMallard;
            findingsRow["RuleAvailable"] = ruleAvail;
        }
        RuleCount.Text = $"Catalog loaded. {CatalogTable.Rows.Count} plugins available for remediation.";
        FindingsCount.Text = $"Findings loaded. {count} findings available for remediation.";
    }
    private void InitializeCatalog() {
        Catalog.AutoGenerateColumns = false;
        Catalog.Rows.Clear();
        Catalog.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "RuleId",
                HeaderText = "Plugin",
                Name = "RuleId",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.Fill,
                DataPropertyName = "Description",
                HeaderText = "Description",
                Name = "Description",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell() {
                    ValueType = typeof(StigRule)
                },
                DataPropertyName = "Rule",
                Name = "Rule",
                ReadOnly = true,
                ValueType = typeof(StigRule),
                Visible = false
            }
        ]);
        Catalog.DataSource = catalogSource;
    }
    private void InitializeCatalogTable() {
        if (CatalogTable.Columns.Count > 0) {
            catalogSource.DataSource = CatalogTable;
            return;
        }
        CatalogTable.Columns.AddRange([
            new("RuleId", typeof(string)),
            new("Description", typeof(string)),
            new("Dangerous", typeof(bool)),
            new("Context", typeof(SettingContext)),
            new("Rule", typeof(StigRule))
        ]);
        CatalogTable.PrimaryKey = [CatalogTable.Columns["RuleId"]!];
        catalogSource.DataSource = CatalogTable;
    }
    private void InitializeFindings() {
        Findings.AutoGenerateColumns = false;
        Findings.DataSource = null;
        Findings.Columns.Clear();
        Findings.Columns.AddRange([
            new DataGridViewCheckBoxColumn() {
                CellTemplate = new DataGridViewCheckBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Remediate",
                HeaderText = "Remediate",
                Name = "Remediate",
                ReadOnly = false,
                ThreeState = false,
                ValueType = typeof(bool),
                Visible = true
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Plugin",
                HeaderText = "Plugin",
                Name = "Plugin",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.Fill,
                DataPropertyName = "Description",
                HeaderText = "Description",
                Name = "Description",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Hostname",
                HeaderText = "Hostname",
                Name = "Hostname",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewCheckBoxColumn() {
                CellTemplate = new DataGridViewCheckBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Dangerous",
                HeaderText = "Dangerous",
                Name = "Dangerous",
                ReadOnly = true,
                ThreeState = false,
                ValueType = typeof(bool),
                Visible = true
            },
            new DataGridViewCheckBoxColumn() {
                CellTemplate = new DataGridViewCheckBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "RuleAvailable",
                HeaderText = "Remediation Available",
                Name = "RuleAvailable",
                ReadOnly = true,
                ThreeState = false,
                ValueType = typeof(bool),
                Visible = true
            },
            new DataGridViewCheckBoxColumn() {
                CellTemplate = new DataGridViewCheckBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "ProperContext",
                HeaderText = "ProperContext",
                Name = "ProperContext",
                ReadOnly = true,
                ThreeState = false,
                ValueType = typeof(bool),
                Visible = false
            },
        ]);
        Findings.DataSource = findingSource;
    }
    private void InitializeFindingsTable() {
        FindingsTable.Rows.Clear();
        if (FindingsTable.Columns.Count > 0) {
            findingSource.DataSource = FindingsTable;
            Findings.DataSource = findingSource;
            return;
        }
        FindingsTable.Columns.AddRange([
            new("Remediate", typeof(bool)),
            new("Plugin", typeof(string)),
            new("Description", typeof(string)),
            new("Hostname", typeof(string)),
            new("Dangerous", typeof(bool)),
            new("RuleAvailable", typeof(bool)),
            new("ProperContext", typeof(bool))
        ]);
        FindingsTable.PrimaryKey = [FindingsTable.Columns["Plugin"]!, FindingsTable.Columns["Hostname"]!];
        findingSource.DataSource = FindingsTable;
    }
    private void InitializeResultsTable() {
        if (ResultsTable.Columns.Count == 0) {
            ResultsTable.Columns.AddRange([
                new("Plugin", typeof(string)),
                new("Hostname", typeof(string)),
                new("Result", typeof(ActionResult)),
                new("Message", typeof(string))
            ]);
            resultsSource.DataSource = ResultsTable;
        }
    }
    private void InitializeFindingsForResults() {
        Findings.AutoGenerateColumns = false;
        Findings.DataSource = null;
        Findings.Columns.Clear();
        Findings.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Plugin",
                HeaderText = "Plugin",
                Name = "Plugin",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Hostname",
                HeaderText = "Hostname",
                Name = "Hostname",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Result",
                HeaderText = "Result",
                Name = "Result",
                ReadOnly = true,
                ValueType = typeof(ActionResult),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Message",
                HeaderText = "Message",
                Name = "Message",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
        ]);
        Findings.DataSource = resultsSource;
        resultsSource.ResetBindings(false);
    }

    [GeneratedRegex(@"((.(?!:SV-))*\.?)(?::SV-[^:]+)(?::)(?:[^:]+)(?::)(?:.*)")]
    private static partial Regex StigNamePattern();
}
