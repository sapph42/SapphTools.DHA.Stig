using System.Data;
using Rule = SapphTools.DHA.Stig.Common.Classes.Rule;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common;
public partial class RuleEditor : Form {
    private readonly Rule? OriginalRule;
    private readonly DataTable SettingsTable;
    private readonly BindingSource source;
    private bool _hasValidMeta = false;
    private bool _hasSettings = false;
    public Rule? ActiveRule { get; private set; }
    public RuleEditor() {
        InitializeComponent();
        DataGridViewColumn settingDgv = new() {
            CellTemplate = new DataGridViewTextBoxCell() {
                ValueType = typeof(Setting)
            },
            DataPropertyName = "Setting",
            Name = "Setting",
            ValueType = typeof(Setting),
            Visible = false,
        };
        Settings.Columns.Add(settingDgv);
        Settings.AutoGenerateColumns = false;
        Settings.Columns["OrderDgv"].DataPropertyName = "Order";
        Settings.Columns["TargetTypeDgv"].DataPropertyName = "Type";
        Settings.Columns["TargetDgv"].DataPropertyName = "Target";
        DataColumn order = new("Order", typeof(int));
        DataColumn type = new("Type", typeof(TargetType));
        DataColumn target = new("Target", typeof(IValue));
        DataColumn setting = new("Setting", typeof(Setting));
        SettingsTable = new();
        SettingsTable.Columns.AddRange([order, type, target, setting]);
        SettingsTable.PrimaryKey = [order];
        source = new() {
            DataSource = SettingsTable,
        };
        OriginalRule = null;
        Settings.CellFormatting += (s, e) => {
            if (Settings.Columns[e.ColumnIndex].Name == "TargetDgv") {
                if (e.Value is IValue val) {
                    e.Value = val.TargetString;
                    e.FormattingApplied = true;
                }
            }
        };
    }
    public RuleEditor(Rule rule) : this() {
        OriginalRule = rule;
        ActiveRule = rule;
        RuleId.Text = rule.RuleId;
        Description.Text = rule.Description;
        foreach (Setting setting in rule.Settings) {
            SettingsTable.Rows.Add(setting.Order, setting.Data.Action, setting.Data.Clone(), setting.Clone());
        }
        Ok.Enabled = true;
    }
    #region Event Handlers
    #region Form
    private void RuleEditor_Load(object sender, EventArgs e) {
        Settings.DataSource = source;
    }
    #endregion Form
    #region Settings 
    private void Settings_SelectionChanged(object sender, EventArgs e) {
        DataGridViewRow? row = GetSelectedRow();
        bool selected = row is not null;
        EditSetting.Enabled =
            RemoveSetting.Enabled =
            MoveUp.Enabled =
            MoveDown.Enabled = selected;
        if (selected) {
            MoveUp.Enabled = CanMoveUp(row!);
            MoveDown.Enabled = CanMoveDown(row!);
        }
    }
    private void AddSetting_Click(object sender, EventArgs e) {
        SettingEditor editor = new() {
            RuleId = RuleId.Text,
        };
        if (editor.ShowDialog() == DialogResult.OK && editor.Setting is not null) {
            Setting setting = editor.Setting.Clone();
            if (SettingsTable.Rows.Count == 0) {
                setting.Order = 0;
            } else {
                setting.Order = (int)SettingsTable.Compute("MAX(Order)", "") + 1;
            }
            SettingsTable.Rows.Add(setting.Order, setting.Data.Action, setting.Data.Clone(), setting.Clone());
        }
        CheckValidity();
    }
    private void EditSetting_Click(object sender, EventArgs e) {
        if (GetSelectedRow() is not DataGridViewRow row || SettingsTable.Rows.Find(row.Cells["OrderDgv"].Value) is not DataRow dRow) {
            return;
        }
        Setting setting = ((Setting)row.Cells["Setting"].Value).Clone();
        SettingEditor editor = new(setting, RuleId.Text);
        if (editor.ShowDialog() == DialogResult.OK && editor.Setting is not null) {
            setting = editor.Setting.Clone();
            dRow["Type"] = setting.Data.Action;
            dRow["Target"] = setting.Data.Clone();
            dRow["Setting"] = setting;
        }
        CheckValidity();
    }
    private void RemoveSetting_Click(object sender, EventArgs e) {
        if (GetSelectedRow() is not DataGridViewRow row) {
            return;
        }
        Setting toRemove = (Setting)row.Cells["Setting"].Value;
        if (SettingsTable.Rows.Find(toRemove.Order) is DataRow dataRow) {
            SettingsTable.Rows.Remove(dataRow);
        }
        CheckValidity();
    }
    private void MoveUp_Click(object sender, EventArgs e) => ShiftRow(-1);
    private void MoveDown_Click(object sender, EventArgs e) => ShiftRow(1);
    #endregion Settings
    #region Other Form Controls
    private void Description_TextChanged(object sender, EventArgs e) {
        CheckValidity();
    }
    private void Ok_Click(object sender, EventArgs e) {
        CheckValidity();
        if (!_hasSettings || !_hasValidMeta) {
            return;
        }
        ActiveRule = new() {
            RuleId = RuleId.Text,
            Description = Description.Text,
            Settings = [.. SettingsTable.Rows.OfType<DataRow>().Select(r => r["Setting"]).Cast<Setting>().OrderBy(s => s.Order)]
        };
        Close();
    }
    private void RuleId_TextChanged(object sender, EventArgs e) {
        CheckValidity();
    }
    private void Cancel_Click(object sender, EventArgs e) {
        ActiveRule = OriginalRule;
        Close();
    }
    #endregion Other Form Controls
    #endregion Event Handlers
    #region Helper Methods
    private bool CanMoveDown(DataGridViewRow row) =>
        row.Cells["OrderDgv"].Value is int i && i < SettingsTable.Rows.Count;
    private static bool CanMoveUp(DataGridViewRow row) =>
        row.Cells["OrderDgv"].Value is int and not 0;
    private void CheckValidity() {
        source.ResetBindings(false);
        _hasValidMeta = !string.IsNullOrWhiteSpace(RuleId.Text) &&
            !string.IsNullOrWhiteSpace(Description.Text) &&
            long.TryParse(RuleId.Text, out _);
        _hasSettings = Settings.Rows.Count > 0;
        Ok.Enabled = _hasSettings && _hasValidMeta;
    } 
    private DataGridViewRow? GetSelectedRow() {
        if (Settings.SelectedRows.Count != 1) {
            return null;
        }
        return Settings.SelectedRows.OfType<DataGridViewRow>().FirstOrDefault();
    }
    private void ShiftRow(int direction) {
        // -1 is up, +1 is down
        bool canMove;
        DataGridViewRow? thisRow = GetSelectedRow();
        if (thisRow is null || direction == 0) {
            return;
        }
        if (direction < 0) {
            direction = -1;
            canMove = CanMoveUp(thisRow);
        } else {
            direction = 1;
            canMove = CanMoveDown(thisRow);
        }
        if (!canMove) {
            return;
        }
        int thisOrder = (int)thisRow.Cells["OrderDgv"].Value;
        int targetOrder = thisOrder + direction;
        DataRow thisDataRow = SettingsTable.Rows.Find(thisOrder)!;
        DataRow targetDataRow = SettingsTable.Rows.Find(targetOrder)!;
        thisDataRow["Order"] = -1;
        targetDataRow["Order"] = thisOrder;
        thisDataRow["Order"] = targetOrder;
        ((Setting)thisDataRow["Setting"]).Order = targetOrder;
        ((Setting)targetDataRow["Setting"]).Order = thisOrder;
        Settings.Refresh();
    }
    #endregion Helper Methods
}
