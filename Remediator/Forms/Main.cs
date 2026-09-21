using ExcelDataReader;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using DataTable = System.Data.DataTable;
using StigRule = SapphTools.DHA.Stig.Common.Classes.Rule;

namespace SapphTools.DHA.Stig.Remediator.Forms;
public partial class Main : Form {
    private bool _supressUiCascade = false;
    private static readonly string localPath = Path.GetDirectoryName(Environment.ProcessPath!)!;
    private readonly BindingSource catalogSource;
    private readonly BindingSource findingSource;
    protected DataTable RuleTable;
    protected DataTable FindingsTable;
    protected DataView FilteredFindings;
    protected Catalog? StigCatalog;
    public Main() {
        InitializeComponent();
        Catalog.AutoGenerateColumns = false;
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
        ]);
        RuleTable = new();
        RuleTable.Columns.AddRange([
            new("RuleId", typeof(string)),
            new("Description", typeof(string)),
            new("Dangerous", typeof(bool)),
            new("Rule", typeof(StigRule))
        ]);
        RuleTable.PrimaryKey = [RuleTable.Columns["RuleId"]!];
        FindingsTable = new();
        FindingsTable.Columns.AddRange([
            new("Remediate", typeof(bool)),
            new("Plugin", typeof(string)),
            new("Description", typeof(string)),
            new("Hostname", typeof(string)),
            new("Dangerous", typeof(bool)),
            new("RuleAvailable", typeof(bool))
        ]);
        FindingsTable.PrimaryKey = [FindingsTable.Columns["Plugin"]!, FindingsTable.Columns["Hostname"]!];
        FilteredFindings = new(FindingsTable) {
            RowFilter = "RuleAvailable = true"
        };
        catalogSource = new() {
            DataSource = RuleTable
        };
        findingSource = new() {
            DataSource = FindingsTable
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
            if (Findings.Columns[e.ColumnIndex].Name == "RuleAvailable") {
                if (e.Value is bool avail) {
                    Findings.Rows[e.RowIndex].Cells["Remediate"].ReadOnly = !avail;
                }
            }
        };
    }

    private void Main_Load(object sender, EventArgs e) {
        CatalogPath.Text = Settings.Default.CatalogPath;
        if (StigCatalog is not null) {
            foreach (StigRule rule in StigCatalog.Rules) {
                RuleTable.Rows.Add(rule.RuleId, rule.Description, rule.Settings.Any(s => s.Dangerous), rule.Clone());
            }
            RuleCount.Text = $"Catalog loaded. {RuleTable.Rows.Count} plugins available for remediation.";
        } else {
            RuleCount.Text = $"Catalog not loaded. Remediation unavailable.";
        }
        Catalog.DataSource = catalogSource;
        Findings.DataSource = findingSource;
    }

    private void FilterFindingsMenu_Click(object sender, EventArgs e) {
        if (_supressUiCascade) { return; }
        _supressUiCascade = true;
        Filter.Checked = FilterFindingsMenu.Checked;
        _supressUiCascade = false;
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
                    RuleTable.Rows.Clear();
                }
                return;
            }
            try {
                string json = File.ReadAllText(ofd.FileName);
                Catalog? catalog = JsonSerializer.Deserialize<Catalog>(
                    File.ReadAllText(Settings.Default.CatalogPath),
                    GlobalConstants.JsonSerializerOptions
                );
                if (catalog is not null && catalog.Rules.Count > 0) {
                    CatalogPath.Text = ofd.FileName;
                    StigCatalog = catalog;
                    RuleTable.Rows.Clear();
                    foreach (StigRule rule in StigCatalog.Rules) {
                        RuleTable.Rows.Add(rule.RuleId, rule.Description, rule.Clone());
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
                            if (RuleTable.Rows.Find(ruleid) is DataRow ruleRow) {
                                ruleAvail = true;
                                drakeMallard = (bool)ruleRow["Dangerous"];
                            }
                            bool autoRem = ruleAvail && !drakeMallard;
                            _ = FindingsTable.Rows.Add(autoRem, ruleid, desc, host, drakeMallard, ruleAvail);
                            if (ruleAvail) {
                                count ??= 0;
                                count++;
                            }
                        }
                    }
                } while (reader.NextResult());
            } catch (Exception ex) {
                MessageBox.Show(
                    $"Exception thrown trying to open or parse vulnerability report.\r\n{ex.Message}",
                    "Exception!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
            } finally {
                findingSource.ResetBindings(false);
                if (count is null) {
                    FindingsCount.Text = "Findings not loaded. Remediation unavailable.";
                } else {
                    FindingsCount.Text = $"Findings loaded. {count} findings available for remediation.";
                }
                Cursor = Cursors.Default;
            }
        }
    }
    private void Remediate_Click(object sender, EventArgs e) {
        if (!string.IsNullOrWhiteSpace(CatalogPath.Text) && File.Exists(CatalogPath.Text)) {
            Settings.Default.CatalogPath = CatalogPath.Text;
            Settings.Default.Save();
        }
        Guid batch = Guid.NewGuid();
        foreach (DataGridViewRow row in Findings.Rows.Cast<DataGridViewRow>()) {
            if (row.Cells["Remediate"].Value is bool and true &&
                    row.Cells["Plugin"].Value is string plugin &&
                    row.Cells["Hostname"].Value is string hostname &&
                    RuleTable.Rows.Find(plugin) is DataRow drRow &&
                    drRow["Rule"] is StigRule rule) {
                Debug.WriteLine($"Calling Executefor rule {rule.RuleId} on {hostname} from row {row.Index}");
                Classes.Remediators.Remediator.Execute(rule, batch, hostname, WhatIf.Checked);
            }
        }
    }
    private void CheckForAvailability() {
        if (RuleTable.Rows.Count == 0) {
            RuleCount.Text = $"Catalog not loaded. Remediation unavailable.";
        }
        if (Findings.Rows.Count == 0) {
            FindingsCount.Text = "Findings not loaded. Remediation unavailable.";
        }
        if (RuleTable.Rows.Count == 0 || Findings.Rows.Count == 0) {
            return;
        }
        int count = 0;
        foreach (DataRow findingsRow in FindingsTable.Rows.Cast<DataRow>()) {
            bool ruleAvail = false;
            bool drakeMallard = false;
            if (RuleTable.Rows.Find(findingsRow["Plugin"]) is DataRow ruleRow) {
                ruleAvail = true;
                drakeMallard = (bool)ruleRow["Dangerous"];
                count++;
            }
            findingsRow["Remediate"] = ruleAvail & (bool)findingsRow["Remediate"] & !drakeMallard;
            findingsRow["RuleAvailable"] = ruleAvail;
        }
        RuleCount.Text = $"Catalog loaded. {RuleTable.Rows.Count} plugins available for remediation.";
        FindingsCount.Text = $"Findings loaded. {count} findings available for remediation.";
    }

    [GeneratedRegex(@"((.(?!:SV-))*\.?)(?::SV-[^:]+)(?::)(?:[^:]+)(?::)(?:.*)")]
    private static partial Regex StigNamePattern();

    private void EditCatalogMenu_Click(object sender, EventArgs e) {
        CatalogEditor catalogEditor;
        if (StigCatalog is null) {
            catalogEditor = new();
        } else {
            catalogEditor = new(StigCatalog);
        }
        if (catalogEditor.ShowDialog() == DialogResult.OK && catalogEditor.Catalog is not null) {
            StigCatalog = catalogEditor.Catalog.Clone();
            RuleTable.Rows.Clear();
            foreach (StigRule rule in StigCatalog.Rules) {
                RuleTable.Rows.Add(rule.RuleId, rule.Description, rule.Settings.Any(s => s.Dangerous), rule.Clone());
            }
            catalogSource.ResetBindings(false);
            CheckForAvailability();
            try {
                string json = JsonSerializer.Serialize(StigCatalog, GlobalConstants.JsonSerializerOptions);
                File.WriteAllText(CatalogPath.Text, json);
            } catch {
                MessageBox.Show("Your changes to the catalog could not be saved, but will remain active for this session.");
            }
        }
    }

    private void Main_FormClosing(object sender, FormClosingEventArgs e) {
        if (!string.IsNullOrWhiteSpace(CatalogPath.Text) && File.Exists(CatalogPath.Text)) {
            Settings.Default.CatalogPath = CatalogPath.Text;
            Settings.Default.Save();
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
    private void WhatIf_CheckedChanged(object sender, EventArgs e) {
        if (_supressUiCascade) { return; }
        _supressUiCascade = true;
        SimulationModeMenu.Checked = WhatIf.Checked;
        _supressUiCascade = false;
    }
    private void LogsMenu_Click(object sender, EventArgs e) {
        Logs logs = new();
        logs.Show();
    }
}
