using SapphTools.DHA.Stig.Remediator.Classes.Rollback;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Forms;

namespace SapphTools.DHA.Stig.Remediator.Forms;
public partial class Logs : Form {
    private readonly BindingSource logsSource = [];
    private readonly BindingSource resultsSource = [];
    private readonly DataTable LogsTable = new();
    protected DataTable ResultsTable = new();
    private bool _preventCascade = false;
    public required DataTable CatalogTable;

    public Logs() {
        InitializeComponent();
        InitializeLogsTable();
        InitializeLogs();
    }
    private void Logs_Load(object sender, EventArgs e) {
        Debug.WriteLine("Logs_Load fired!");
        OpenFileDialog ofd = new() {
            AutoUpgradeEnabled = true,
            CheckFileExists = true,
            Filter = "Log Files (*.log;*.jsonl)|*.log;*.jsonl",
            InitialDirectory = @"\\eamcfs01\dept$\_EAMC_DATA\workgroup\IMD\System Administration\_AdminApps\Stig Remediator\RemediationLog\",
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
        foreach (LogAction action in batches.Flatten()) {
            LogsTable.Rows.Add(
                action.RemediationBatch,
                action.RuleBatch,
                action.SettingBatch,
                action.ActionNumber,
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
        LogsView.DataSource = logsSource;
        logsSource.ResetBindings(false);
    }
    private void Actions_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
        if (!ReferenceEquals(LogsView.DataSource, logsSource)) {
            return;
        }
        DataGridViewColumn column = LogsView.Columns[e.ColumnIndex];
        if (column.SortMode != DataGridViewColumnSortMode.Programmatic) {
            return;
        }
        string columnName = column.DataPropertyName;
        string dir = column.HeaderCell.SortGlyphDirection == SortOrder.Ascending ?
            "DESC" :
            "ASC";
        if (columnName != "RuleId") {
            logsSource.Sort = $"{columnName} {dir}, RuleId ASC, SettingIndex ASC";
        } else {
            logsSource.Sort = $"{columnName} {dir}, SettingIndex ASC";
        }
        foreach (DataGridViewColumn col in LogsView.Columns) {
            if (col.SortMode == DataGridViewColumnSortMode.Programmatic) {
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
        }
        column.HeaderCell.SortGlyphDirection = dir == "ASC" ?
            SortOrder.Ascending :
            SortOrder.Descending;
    }
    private void Actions_SelectionChanged(object sender, EventArgs e) {
        if (_preventCascade || !ReferenceEquals(LogsView.DataSource, logsSource)) {
            return;
        }
        _preventCascade = true;
        foreach (DataGridViewRow selected in LogsView.SelectedRows.Cast<DataGridViewRow>()) {
            if ((RollbackCapability)selected.Cells["RollbackCapability"].Value != RollbackCapability.Automatic) {
                selected.Selected = false;
                continue;
            }
            foreach (DataGridViewRow sibling in LogsView.Rows.Cast<DataGridViewRow>().Where(
                        r => r.Cells["RemediationBatch"].Value.Equals(selected.Cells["RemediationBatch"].Value) &&
                        r.Cells["RuleBatch"].Value.Equals(selected.Cells["RuleBatch"].Value) &&
                        r.Cells["SettingBatch"].Value.Equals(selected.Cells["SettingBatch"].Value)
                    )) {
                sibling.Selected = true;
            }
        }
        _preventCascade = false;
    }
    private void Rollback_Click(object sender, EventArgs e) {
        DialogResult resp = MessageBox.Show("This will rollback successful remediation, and restore settings that led to an ACAS finding. Are you sure?",
            "Be Sure.",
            MessageBoxButtons.YesNoCancel);
        if (resp != DialogResult.Yes) {
            return;
        }
        Guid batch = Guid.NewGuid();
        InitializeResultsTable();
        DataTable RollbackTargets = new();
        RollbackTargets.Columns.AddRange([
            ..CreateLogKeys(),
            new() {
                Caption = "Plugin",
                ColumnName = "RuleId",
                DataType = typeof(string),
            },
            new() {
                Caption = "Action",
                ColumnName = "Action",
                DataType = typeof(RemediationAction),
            },
        ]);
        foreach (DataGridViewRow row in LogsView.SelectedRows.Cast<DataGridViewRow>()) {
            if ((RollbackCapability)row.Cells["RollbackCapability"].Value == RollbackCapability.Automatic) {
                if (row.Cells["RuleId"].Value is string plugin &&
                        row.Cells["RemediationBatch"].Value is Guid remBatch &&
                        row.Cells["RuleBatch"].Value is Guid ruleBatch &&
                        row.Cells["SettingBatch"].Value is Guid settingBatch &&
                        row.Cells["ActionNo"].Value is int actionNo &&
                        LogsTable.Rows.Find([remBatch, ruleBatch, settingBatch, actionNo]) is DataRow drRow &&
                        drRow["Action"] is LogAction action) {
                    RollbackTargets.Rows.Add(remBatch, ruleBatch, settingBatch, actionNo, plugin, action.ToRemediationAction());
                }
            }
        }
        DataView rollbackOrder = new(
            RollbackTargets,
            RowFilter: null,
            Sort: "RemediationBatch ASC, RuleBatch ASC, SettingBatch ASC",
            RowState: DataViewRowState.CurrentRows
        );
        foreach (DataRowView row in rollbackOrder) {
            RemediationAction action = (RemediationAction)row["Action"];
            RemediationActionResult res = Classes.Remediators.Rollbacker.Execute(batch, action);
            ResultsTable.Rows.Add(row["RuleId"],
                action.ComputerName,
                res.Value?.Result ?? ActionResult.ActionFailure,
                res.Value?.FailureMessage ?? res.Message
            );
        }
        InitializeLogsForResults();
    }
    private static DataColumn[] CreateLogKeys() {
        DataColumn remBatch = new() {
            Caption = "Remediation Batch",
            ColumnName = "RemediationBatch",
            DataType = typeof(Guid),
        };
        DataColumn ruleBatch = new() {
            Caption = "Rule Batch",
            ColumnName = "RuleBatch",
            DataType = typeof(Guid),
        };
        DataColumn settingBatch = new() {
            Caption = "Setting Batch",
            ColumnName = "SettingBatch",
            DataType = typeof(Guid),
        };
        DataColumn actionNo = new() {
            Caption = "Action Number",
            ColumnName = "ActionNo",
            DataType = typeof(int),
        };
        return [remBatch, ruleBatch, settingBatch, actionNo];
    }
    private void InitializeLogsTable() {
        LogsTable.Rows.Clear();
        if (LogsTable.Columns.Count > 0) {
            logsSource.DataSource = LogsTable;
            LogsView.DataSource = logsSource;
            return;
        }
        DataColumn[] logKeys = CreateLogKeys();
        LogsTable.Columns.AddRange([
            ..logKeys,
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
        LogsTable.PrimaryKey = logKeys;
        logsSource.DataSource = LogsTable;
    }
    private void InitializeLogs() {
        LogsView.AutoGenerateColumns = false;
        LogsView.DataSource = null;
        LogsView.Columns.Clear();
        LogsView.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "RemediationBatch",
               Name = "RemediationBatch",
               Visible = false
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "RuleBatch",
               Name = "RuleBatch",
               Visible = false
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "SettingBatch",
               Name = "SettingBatch",
               Visible = false
            },
            new DataGridViewTextBoxColumn() {
               DataPropertyName = "ActionNo",
               Name = "ActionNo",
               Visible = false
            },
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
               SortMode = DataGridViewColumnSortMode.Programmatic,
               ValueType = typeof(RollbackCapability)
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
        LogsView.DataSource = logsSource;
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
    private void InitializeLogsForResults() {
        LogsView.AutoGenerateColumns = false;
        LogsView.DataSource = null;
        LogsView.Columns.Clear();
        LogsView.Columns.AddRange([
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
        LogsView.DataSource = resultsSource;
        resultsSource.ResetBindings(false);
    }
}
