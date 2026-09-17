using Microsoft.Office.Core;
using SapphTools.DHA.Stig.Common.Classes;
using System.Data;
using Rule = SapphTools.DHA.Stig.Common.Classes.Rule;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common;
public partial class CatalogEditor : Form {
    private readonly Catalog? OriginalCatalog;
    private readonly DataTable RulesTable;
    private readonly BindingSource source;
    private readonly Catalog ActiveCatalog;
    public Catalog? Catalog;
    public CatalogEditor() {
        InitializeComponent();
        Rules.AutoGenerateColumns = false;
        Rules.Columns.AddRange([
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
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.Fill,
                DataPropertyName = "SettingCount",
                HeaderText = "# of Settings",
                Name = "SettingCount",
                ReadOnly = true,
                ValueType = typeof(int),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell() {
                    ValueType = typeof(Rule)
                },
                DataPropertyName = "Rule",
                Name = "Rule",
                ReadOnly = true,
                ValueType = typeof(Rule),
                Visible = false
            }
        ]);
        Blacklist.AutoGenerateColumns = false;
        Blacklist.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "RuleId",
                HeaderText = "Plugin",
                Name = "RuleId",
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Reason",
                HeaderText = "Reason",
                Name = "Reason",
                ValueType = typeof(string),
                Visible = true,
            }
        ]);
        RulesTable = new();
        RulesTable.Columns.AddRange([
            new("RuleId", typeof(string)),
            new("Description", typeof(string)),
            new("SettingCount", typeof(int)),
            new("Rule", typeof(Rule))
        ]);
        RulesTable.PrimaryKey = [RulesTable.Columns["RuleId"]!];
        source = new() {
            DataSource = RulesTable
        };
        SchemaVersion.Text = Resources.SchemaVersion;
        ActiveCatalog = new() {
            Name = "",
            SchemaVersion = int.Parse(Resources.SchemaVersion),
            Rules = []
        };
    }
    public CatalogEditor(Catalog catalog) : this() {
        OriginalCatalog = catalog;
        ActiveCatalog = catalog.Clone();
        foreach (Rule rule in OriginalCatalog.Rules) {
            RulesTable.Rows.Add(rule.RuleId, rule.Description, rule.Settings.Count, rule.Clone());
        }
        foreach (BlacklistedPlugin plugin in OriginalCatalog.Blacklist) {
            int idx = Blacklist.Rows.Add();
            DataGridViewRow row = Blacklist.Rows[idx];
            row.Cells["RuleId"].Value = plugin.RuleId;
            row.Cells["Reason"].Value = plugin.Reason;
        }
        CatalogName.Text = catalog.Name;
        SchemaVersion.Text = catalog.SchemaVersion.ToString();
    }
    private void CatalogEditor_Load(object sender, EventArgs e) {
        Rules.DataSource = source;
        if (!int.TryParse(Resources.SchemaVersion, out int canonicalVer)) {
            MessageBox.Show("The canonical schema version is not integral. This is an unrecoverable bug. See developer.");
            Close();
            return;
        }
        if (OriginalCatalog is not null && OriginalCatalog.SchemaVersion > canonicalVer) {
            MessageBox.Show("The provided schema is of a higher version than this binary permits.");
            Close();
            return;
        }
    }
    private void AddRule_Click(object sender, EventArgs e) {
        RuleEditor ruleEditor = new();
        if (ruleEditor.ShowDialog() == DialogResult.OK && ruleEditor.ActiveRule is Rule rule) {
            Dictionary<string, string> check = Blacklist.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells[0].Value is not null)
                .Select(r => new KeyValuePair<string, string>(
                    r.Cells["RuleId"].Value.ToString()!,
                    r.Cells["Reason"].Value.ToString()!
                ))
                .ToDictionary();
            if (check.TryGetValue(rule.RuleId, out string? reason)) {
                MessageBox.Show(
                    "You tried to add a rule that has been blacklisted.\r\n" +
                    "Reason: \r\n\r\n" +
                    reason,
                    "Blocked Rule Creation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );
                return;
            }
            if (ActiveCatalog.Rules.Add(rule)) {
                RulesTable.Rows.Add(rule.RuleId, rule.Description, rule.Settings.Count, rule);
            }
        }
        CheckValidity();
    }
    private void EditRule_Click(object sender, EventArgs e) {
        if (GetSelectedRow() is not DataGridViewRow row || RulesTable.Rows.Find(row.Cells["RuleId"].Value) is not DataRow dRow) {
            return;
        }
        Rule rule = ((Rule)row.Cells["Rule"].Value).Clone();
        RuleEditor ruleEditor = new(rule);
        if (ruleEditor.ShowDialog() == DialogResult.OK && ruleEditor.ActiveRule is not null) {
            rule = ruleEditor.ActiveRule.Clone();
            dRow["RuleId"] = rule.RuleId;
            dRow["Description"] = rule.Description;
            dRow["SettingCount"] = rule.Settings.Count;
            dRow["Rule"] = rule;
        }
        CheckValidity();
    }
    private void RemoveRule_Click(object sender, EventArgs e) {
        if (GetSelectedRow() is not DataGridViewRow row) {
            return;
        }
        Rule toRemove = (Rule)row.Cells["Rule"].Value;
        if (RulesTable.Rows.Find(toRemove.RuleId) is DataRow dataRow) {
            RulesTable.Rows.Remove(dataRow);
        }
        CheckValidity();
    }
    private void Ok_Click(object sender, EventArgs e) {
        ActiveCatalog.Name = CatalogName.Text;
        ActiveCatalog.Blacklist.Clear();
        ActiveCatalog.Blacklist.AddRange(
            Blacklist.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells[0].Value is not null)
                .Select(r =>
                    new BlacklistedPlugin() {
                        RuleId = r.Cells["RuleId"].Value.ToString()!,
                        Reason = r.Cells["Reason"].Value.ToString()!
                    })
        );
        ActiveCatalog.Rules.Clear();
        foreach (DataRow row in RulesTable.Rows.Cast<DataRow>()) {
            ActiveCatalog.Rules.Add((Rule)row["Rule"]);
        }
        Catalog = ActiveCatalog.Clone();
        Close();
    }
    private void Cancel_Click(object sender, EventArgs e) {
        Catalog = null;
        Close();
    }
    private void Rules_SelectionChanged(object sender, EventArgs e) {
        if (GetSelectedRow() is not null) {
            EditRule.Enabled = true;
            RemoveRule.Enabled = true;
        } else {
            EditRule.Enabled = false;
            RemoveRule.Enabled = false;
        }
    }
    private void Blacklist_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
        if (Blacklist.Rows[e.RowIndex].IsNewRow) {
            return;
        }
        if (string.IsNullOrWhiteSpace(e.FormattedValue?.ToString())) {
            e.Cancel = true;
        }
    }
    private void Blacklist_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {
        if (e.RowIndex == 0) {
            return;
        }
        for (int i = e.RowIndex; i < Blacklist.Rows.Count; i++) {
            if (Blacklist.Rows[i].Cells["RuleId"]?.Value?.ToString() is string plugin) {
                if (RulesTable.Rows.Find(plugin) is DataRow row) {
                    RulesTable.Rows.Remove(row);
                }
            }
        }
        source?.ResetBindings(false);
    }
    private void CheckValidity() {
        source.ResetBindings(false);
        Ok.Enabled = !string.IsNullOrWhiteSpace(CatalogName.Text) && Rules.Rows.Count > 0;
    }
    private DataGridViewRow? GetSelectedRow() {
        if (Rules.SelectedRows.Count != 1) {
            return null;
        }
        return Rules.SelectedRows.OfType<DataGridViewRow>().FirstOrDefault();
    }
}
