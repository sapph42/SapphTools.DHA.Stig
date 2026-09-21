using SapphTools.DHA.Stig.Remediator.Classes.Rollback;
using System.Data;
using System.Text.Json;

namespace SapphTools.DHA.Stig.Remediator.Forms;
public partial class Logs : Form {
    private readonly BindingSource source;
    private readonly DataTable logActions = new();
    private bool _preventCascade = false;
    public Logs() {
        InitializeComponent();
        source = new() {
            DataSource = logActions
        };
    }
    private void Logs_Load(object sender, EventArgs e) {
        OpenFileDialog ofd = new() {
            AutoUpgradeEnabled = true,
            CheckFileExists = true,
            Filter = "Log Files (*.log;*.jsonl)|*.log;*.jsonl",
            InitialDirectory = @"\\eamfs01\dept$\_EAMC_DATA\workgroup\IMD\System Administration\_AdminApps\Stig Remediator\RemediationLog\",
            Multiselect = false,
            OkRequiresInteraction = true,
            ReadOnlyChecked = true,
            SelectReadOnly = true,
            Title = "Select Remediator Log File"
        };
        if (ofd.ShowDialog() != DialogResult.OK ||
                string.IsNullOrWhiteSpace(ofd.FileName) ||
                !File.Exists(ofd.FileName)) {
            Close();
            return;
        }
        LogBatchCollection batches = [];
        foreach (string entry in File.ReadAllLines(ofd.FileName)) {
            try {
                RemediationAction action = JsonSerializer.Deserialize<RemediationAction>(entry, GlobalConstants.JsonSerializerOptions) ?? throw new Exception();
                batches.Add(action);
            } catch { }
        }
        logActions.Columns.AddRange([
            new() {
                Caption = "Batch",
                ColumnName = "RemediationBatch",
                DataType = typeof(Guid),
            },
            new() {
                Caption = "ComputerName",
                ColumnName = "ComputerName",
                DataType = typeof(string),
            },
            new() {
                Caption = "Plugin",
                ColumnName = "RuleId",
                DataType = typeof(string),
            },
            new() {
                Caption = "Description",
                ColumnName = "Description",
                DataType = typeof(string),
            },
            new() {
                Caption = "Setting #",
                ColumnName = "SettingIndex",
                DataType = typeof(int),
            },
            new() {
                Caption = "Source",
                ColumnName = "Source",
                DataType = typeof(ActionSource),
            },
            new() {
                Caption = "Type",
                ColumnName = "TargetType",
                DataType = typeof(TargetType),
            },
            new() {
                Caption = "Target",
                ColumnName = "Target",
                DataType = typeof(string),
            },
            new() {
                Caption = "Rollback Capability",
                ColumnName = "RollbackCapability",
                DataType = typeof(RollbackCapability),
            },
            new() {
                Caption = "Result",
                ColumnName = "Result",
                DataType = typeof(ActionResult),
            },
            new() {
                AllowDBNull = true,
                Caption = "Message",
                ColumnName = "FailureMessage",
                DataType = typeof(string),
            },
            new() {
                Caption = "Timestamp",
                ColumnName = "RemediationTimestamp",
                DataType = typeof(DateTimeOffset),
            },
            new() {
                Caption = "Action",
                ColumnName = "Action",
                DataType = typeof(LogAction),
            },
        ]);
        Actions.AutoGenerateColumns = false;
        Actions.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "ComputerName",
               HeaderText = "Computer Name",
               Name = "ComputerName",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
               SortMode = DataGridViewColumnSortMode.Programmatic
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "RuleId",
               HeaderText = "Plugin",
               Name = "RuleId",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
               SortMode = DataGridViewColumnSortMode.Programmatic
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "Description",
               HeaderText = "Description",
               Name = "Description",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
               SortMode = DataGridViewColumnSortMode.NotSortable,
               Width = 250
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "SettingIndex",
               HeaderText = "Setting #",
               Name = "SettingIndex",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
               SortMode = DataGridViewColumnSortMode.NotSortable
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "Source",
               HeaderText = "Source",
               Name = "Source",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
               SortMode = DataGridViewColumnSortMode.Programmatic
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "TargetType",
               HeaderText = "Type",
               Name = "TargetType",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
               SortMode = DataGridViewColumnSortMode.Programmatic
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "Target",
               HeaderText = "Target",
               Name = "Target",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
               SortMode = DataGridViewColumnSortMode.NotSortable,
               Width = 200
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "RollbackCapability",
               HeaderText = "Rollback Capability",
               Name = "RollbackCapability",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
               SortMode = DataGridViewColumnSortMode.Programmatic
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "Result",
               HeaderText = "Result",
               Name = "Result",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
               SortMode = DataGridViewColumnSortMode.Programmatic
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "FailureMessage",
               HeaderText = "Message",
               Name = "FailureMessage",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
               SortMode = DataGridViewColumnSortMode.NotSortable,
               MinimumWidth = 300
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "RemediationTimestamp",
               HeaderText = "Timestamp",
               Name = "RemediationTimestamp",
               AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
               SortMode = DataGridViewColumnSortMode.Programmatic
            },
        ]);
        foreach (LogAction action in batches.Flatten()) {
            logActions.Rows.Add(
                action.RemediationBatch,
                action.ComputerName,
                action.RuleId,
                action.Description,
                action.SettingIndex,
                action.Source,
                action.TargetType,
                action.Target,
                action.RollbackCapability,
                action.Result,
                action.FailureMessage,
                action.RemediationTimestamp,
                action
            );
        }
        Actions.DataSource = source;
        source.ResetBindings(false);
    }

    private void Actions_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
        DataGridViewColumn column = Actions.Columns[e.ColumnIndex];
        if (column.SortMode != DataGridViewColumnSortMode.Programmatic) {
            return;
        }
        string columnName = column.DataPropertyName;
        string dir = column.HeaderCell.SortGlyphDirection == SortOrder.Ascending ?
            "DESC" :
            "ASC";
        if (columnName != "RuleId") {
            source.Sort = $"{columnName} {dir}, RuleId ASC, SettingIndex ASC";
        } else {
            source.Sort = $"{columnName} {dir}, SettingIndex ASC";
        }
        foreach (DataGridViewColumn col in Actions.Columns) {
            if (col.SortMode == DataGridViewColumnSortMode.Programmatic) {
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
        }
        column.HeaderCell.SortGlyphDirection = dir == "ASC" ?
            SortOrder.Ascending :
            SortOrder.Descending;
    }

    private void Actions_SelectionChanged(object sender, EventArgs e) {
        if (_preventCascade) {
            return;
        }
        _preventCascade = true;
        foreach (DataGridViewRow selected in Actions.SelectedRows.Cast<DataGridViewRow>()) {
            foreach (DataGridViewRow sibling in Actions.Rows.Cast<DataGridViewRow>().Where(
                        r => r.Cells["ComputerName"].Value.Equals(selected.Cells["ComputerName"].Value) &&
                        r.Cells["RuleId"].Value.Equals(selected.Cells["RuleId"].Value)
                    )) {
                sibling.Selected = true;
            }
        }
        _preventCascade = false;
    }
}
