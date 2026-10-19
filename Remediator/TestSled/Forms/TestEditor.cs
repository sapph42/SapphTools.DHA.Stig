using SapphTools.DHA.Stig.Common.Classes;
using System.Data;

namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms;
public partial class TestEditor : Form {
    private readonly Test? OriginalTest;
    private readonly DataTable TestSuiteTable;
    private readonly DataTable RecoverySuiteTable;
    private readonly BindingSource testSource;
    private readonly BindingSource recoverySource;
    private readonly Test ActiveTest;
    private string SimulatedRuleId => $"T{TestId.Text}P{Phase.Text}O{Order.Text}";
    public Test? Test;
    public TestEditor() {
        InitializeComponent();
        TestSuite.AutoGenerateColumns = false;
        TestSuite.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Order",
                HeaderText = "Order",
                Name = "Order",
                ReadOnly = true,
                ValueType = typeof(int),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Setting",
                HeaderText = "Setting",
                Name = "Setting",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "ExpectedAfter",
                HeaderText = "Expected After",
                Name = "ExpectedAfter",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "ExpectedResult",
                HeaderText = "Expected Result",
                Name = "ExpectedResult",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell() {
                    ValueType = typeof(TestCase)
                },
                DataPropertyName = "TestCase",
                Name = "TestCase",
                ReadOnly = true,
                ValueType = typeof(TestCase),
                Visible = false
            }
        ]);
        TestSuite.CellFormatting += (s, e) => {
            switch (TestSuite.Columns[e.ColumnIndex].Name) {
                case "Setting":
                    if (e.Value is Setting setting) {
                        e.Value = setting.Data.TargetString;
                        e.FormattingApplied = true;
                    }
                    break;
                case "ExpectedAfter":
                    if (e.Value is IValue data) {
                        e.Value = data.TargetString;
                        e.FormattingApplied = true;
                    }
                    break;
                case "ExpectedResult":
                    if (e.Value is ActionResult res) {
                        e.Value = res.ToString();
                        e.FormattingApplied = true;
                    }
                    break;
            }
        };
        RecoverySuite.AutoGenerateColumns = false;
        RecoverySuite.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Order",
                HeaderText = "Order",
                Name = "Order",
                ReadOnly = true,
                ValueType = typeof(int),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Setting",
                HeaderText = "Setting",
                Name = "Setting",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "ExpectedAfter",
                HeaderText = "Expected After",
                Name = "ExpectedAfter",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "ExpectedResult",
                HeaderText = "Expected Result",
                Name = "ExpectedResult",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell() {
                    ValueType = typeof(TestCase)
                },
                DataPropertyName = "TestCase",
                Name = "TestCase",
                ReadOnly = true,
                ValueType = typeof(TestCase),
                Visible = false
            }
        ]);
        RecoverySuite.CellFormatting += (s, e) => {
            switch (RecoverySuite.Columns[e.ColumnIndex].Name) {
                case "Setting":
                    if (e.Value is Setting setting) {
                        e.Value = setting.Data.TargetString;
                        e.FormattingApplied = true;
                    }
                    break;
                case "ExpectedAfter":
                    if (e.Value is IValue data) {
                        e.Value = data.TargetString;
                        e.FormattingApplied = true;
                    }
                    break;
                case "ExpectedResult":
                    if (e.Value is ActionResult res) {
                        e.Value = res.ToString();
                        e.FormattingApplied = true;
                    }
                    break;
            }
        };
        TestSuiteTable = new();
        DataColumn testOrder = new("Order", typeof(int));
        TestSuiteTable.Columns.AddRange([
            testOrder,
            new("Setting", typeof(Setting)),
            new("ExpectedAfter", typeof(IValue)),
            new("ExpectedResult", typeof(ActionResult)),
            new("TestCase", typeof(TestCase))
        ]);
        RecoverySuiteTable = new();
        DataColumn recoveryOrder = new("Order", typeof(int));
        RecoverySuiteTable.Columns.AddRange([
            recoveryOrder,
            new("Setting", typeof(Setting)),
            new("ExpectedAfter", typeof(IValue)),
            new("ExpectedResult", typeof(ActionResult)),
            new("TestCase", typeof(TestCase))
        ]);
        TestSuiteTable.PrimaryKey = [testOrder];
        RecoverySuiteTable.PrimaryKey = [recoveryOrder];
        testSource = new() {
            DataSource = TestSuiteTable
        };
        recoverySource = new() {
            DataSource = RecoverySuiteTable
        };
        ActiveTest = new() {
            Config = new()
        };
        ConfigTarget.DropDownStyle = ComboBoxStyle.DropDownList;
        ConfigTarget.DataSource = Enum.GetValues<TestTarget>()
            .Where(target => target != TestTarget.Invalid).ToArray();
        ConfigTarget.SelectedItem = TestTarget.Local;

        TestSuite.AllowUserToAddRows = false;
        TestSuite.AllowUserToDeleteRows = false;
        TestSuite.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        TestSuite.MultiSelect = false;
        TestSuite.DataSource = testSource;
        RecoverySuite.AllowUserToAddRows = false;
        RecoverySuite.AllowUserToDeleteRows = false;
        RecoverySuite.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        RecoverySuite.MultiSelect = false;
        RecoverySuite.DataSource = recoverySource;
        TestSuiteTable.DefaultView.Sort = "Order ASC";
        RecoverySuiteTable.DefaultView.Sort = "Order ASC";

        TestId.Text = "0"; // Assigned by ArrayEditor when a test is added.
        Phase.Text = "1";
        Order.Text = "1";
        Phase.TextChanged += (_, _) => CheckValidity();
        TestSuite.SelectionChanged += (_, _) => UpdateSelectionButtons();
        RecoverySuite.SelectionChanged += (_, _) => UpdateSelectionButtons();
        CheckValidity();
    }
    public TestEditor(Test test) : this() {
        OriginalTest = test;
        ActiveTest = test.Clone();
        TestId.Text = ActiveTest.TestId.ToString();
        Phase.Text = ActiveTest.Phase.ToString();
        Order.Text = ActiveTest.Order.ToString();
        ConfigElevate.Checked = ActiveTest.Config.Elevated;
        ConfigRollback.Checked = ActiveTest.Config.AttemptRollback;
        ConfigWhatIf.Checked = ActiveTest.Config.WhatIf;
        ConfigTarget.SelectedItem = ActiveTest.Config.Target;
        foreach (TestCase tc in ActiveTest.TestSuite) {
            TestSuiteTable.Rows.Add(tc.Order, tc.Setting.Clone(), tc.ExpectedAfter.Clone(), tc.ExpectedResult, tc.Clone(withoutResults: true));
        }
        foreach (TestCase tc in ActiveTest.RecoverySuite) {
            RecoverySuiteTable.Rows.Add(tc.Order, tc.Setting.Clone(), tc.ExpectedAfter.Clone(), tc.ExpectedResult, tc.Clone(withoutResults: true));
        }
        CheckValidity();
    }

    private void AddTestCase_Click(object sender, EventArgs e) {
        TestCaseEditor testCaseEditor = new() {
            SimulatedRuleId = SimulatedRuleId,
        };
        if (testCaseEditor.ShowDialog() == DialogResult.OK && testCaseEditor.TestCase is TestCase tc) {
            TestCase testCase = tc.Clone(withoutResults: true);
            if (TestSuiteTable.Rows.Count == 0) {
                testCase.Order = 0;
            } else {
                testCase.Order = (int)TestSuiteTable.Compute("MAX(Order)", "") + 1;
            }
            TestSuiteTable.Rows.Add(testCase.Order, testCase.Setting, testCase.ExpectedAfter, testCase.ExpectedResult, testCase);
        }
        CheckValidity();
    }

    private void EditTestCase_Click(object sender, EventArgs e) {
        if (GetSelectedRow(TestSuite) is not DataGridViewRow row || 
                TestSuiteTable.Rows.Find(row.Cells["Order"].Value) is not DataRow dRow) {
            return;
        }
        TestCaseEditor testCaseEditor = new(((TestCase)dRow["TestCase"]).Clone()) {
            SimulatedRuleId = SimulatedRuleId,
        };
        if (testCaseEditor.ShowDialog() == DialogResult.OK && testCaseEditor.TestCase is TestCase testCase) {
            dRow["Setting"] = testCase.Setting;
            dRow["ExpectedAfter"] = testCase.ExpectedAfter;
            dRow["ExpectedResult"] = testCase.ExpectedResult;
            dRow["TestCase"] = testCase;
        }
        CheckValidity();
    }

    private void RemoveTestCase_Click(object sender, EventArgs e) {
        if (GetSelectedRow(TestSuite) is not DataGridViewRow row) {
            return;
        }
        RemoveCase(TestSuiteTable, row);
        CheckValidity();
    }

    private void AddRecoveryCase_Click(object sender, EventArgs e) {
        TestCaseEditor testCaseEditor = new() {
            SimulatedRuleId = SimulatedRuleId,
        };
        if (testCaseEditor.ShowDialog() == DialogResult.OK && testCaseEditor.TestCase is TestCase tc) {
            RecoverySuiteTable.Rows.Add(tc.Order, tc.Setting.Clone(), tc.ExpectedAfter.Clone(), tc.ExpectedResult, tc.Clone(withoutResults: true));
        }
        CheckValidity();
    }

    private void EditRecoveryCase_Click(object sender, EventArgs e) {
        if (GetSelectedRow(RecoverySuite) is not DataGridViewRow row ||
                RecoverySuiteTable.Rows.Find(row.Cells["Order"].Value) is not DataRow dRow) {
            return;
        }
        TestCaseEditor testCaseEditor = new(((TestCase)dRow["TestCase"]).Clone()) {
            SimulatedRuleId = SimulatedRuleId
        };
        if (testCaseEditor.ShowDialog() == DialogResult.OK && testCaseEditor.TestCase is TestCase testCase) {
            dRow["Setting"] = testCase.Setting;
            dRow["ExpectedAfter"] = testCase.ExpectedAfter;
            dRow["ExpectedResult"] = testCase.ExpectedResult;
            dRow["TestCase"] = testCase;
        }
        CheckValidity();
    }

    private void RemoveRecoveryCase_Click(object sender, EventArgs e) {
        if (GetSelectedRow(RecoverySuite) is not DataGridViewRow row) {
            return;
        }
        RemoveCase(RecoverySuiteTable, row);
        CheckValidity();
    }

    private void MoveTestUp_Click(object sender, EventArgs e) => MoveCase(TestSuite, TestSuiteTable, testSource, -1);

    private void MoveTestDown_Click(object sender, EventArgs e) => MoveCase(TestSuite, TestSuiteTable, testSource, 1);

    private void MoveRecoveryUp_Click(object sender, EventArgs e) => MoveCase(RecoverySuite, RecoverySuiteTable, recoverySource, -1);

    private void MoveRecoveryDown_Click(object sender, EventArgs e) => MoveCase(RecoverySuite, RecoverySuiteTable, recoverySource, 1);

    // Each DataRow owns its fixed sequence slot. Move the cases, not the row keys.
    // The editor deliberately leaves its SortedSets alone until OK reconstructs them.
    private static void SetCase(DataRow row, TestCase testCase) {
        row["Setting"] = testCase.Setting;
        row["ExpectedAfter"] = testCase.ExpectedAfter;
        row["ExpectedResult"] = testCase.ExpectedResult;
        row["TestCase"] = testCase;
    }

    private void MoveCase(DataGridView grid, DataTable table, BindingSource binding, int direction) {
        if (GetSelectedRow(grid) is not DataGridViewRow selection ||
                selection.Cells["Order"].Value is not int order) {
            return;
        }
        DataRow[] ordered = [.. table.Rows.Cast<DataRow>().OrderBy(row => (int)row["Order"])];
        int index = Array.FindIndex(ordered, row => (int)row["Order"] == order);
        int next = index + direction;
        if (index < 0 || next < 0 || next >= ordered.Length) {
            return;
        }

        DataRow current = ordered[index];
        DataRow adjacent = ordered[next];
        TestCase moving = ((TestCase)current["TestCase"]).Clone(withoutResults: true);
        TestCase displaced = ((TestCase)adjacent["TestCase"]).Clone(withoutResults: true);
        moving.Order = (int)adjacent["Order"];
        displaced.Order = (int)current["Order"];
        SetCase(current, displaced);
        SetCase(adjacent, moving);

        binding.ResetBindings(false);
        grid.ClearSelection();
        foreach (DataGridViewRow row in grid.Rows) {
            if (row.Cells["Order"].Value is int selectedOrder && selectedOrder == moving.Order) {
                row.Selected = true;
                break;
            }
        }
        UpdateSelectionButtons();
    }

    private static void RemoveCase(DataTable table, DataGridViewRow selection) {
        if (selection.Cells["Order"].Value is not int order ||
                table.Rows.Find(order) is not DataRow selected) {
            return;
        }
        table.Rows.Remove(selected);
        // Renumber in ascending order so every new primary key is currently vacant.
        int nextOrder = 1;
        foreach (DataRow row in table.Rows.Cast<DataRow>().OrderBy(row => (int)row["Order"])) {
            TestCase testCase = ((TestCase)row["TestCase"]).Clone(withoutResults: true);
            testCase.Order = nextOrder;
            row["Order"] = nextOrder;
            row["TestCase"] = testCase;
            nextOrder++;
        }
    }

    private void UpdateSelectionButtons() {
        UpdateSelectionButtons(TestSuite, TestSuiteTable, EditTestCase, RemoveTestCase, MoveTestUp, MoveTestDown);
        UpdateSelectionButtons(RecoverySuite, RecoverySuiteTable, EditRecoveryCase, RemoveRecoveryCase,
            MoveRecoveryUp, MoveRecoveryDown);
    }

    private static void UpdateSelectionButtons(DataGridView grid, DataTable table, Button edit, Button remove,
            Button up, Button down) {
        int? order = GetSelectedRow(grid)?.Cells["Order"].Value as int?;
        DataRow[] rows = [.. table.Rows.Cast<DataRow>().OrderBy(row => (int)row["Order"])];
        int index = order.HasValue ? Array.FindIndex(rows, row => (int)row["Order"] == order.Value) : -1;
        edit.Enabled = remove.Enabled = index >= 0;
        up.Enabled = index > 0;
        down.Enabled = index >= 0 && index < rows.Length - 1;
    }

    private void Ok_Click(object sender, EventArgs e) {
        if (!int.TryParse(Phase.Text, out int phase) || phase < 1 ||
                !int.TryParse(Order.Text, out int order) || order < 1 ||
                !int.TryParse(TestId.Text, out int testId) ||
                ConfigTarget.SelectedItem is not TestTarget target || target == TestTarget.Invalid ||
                TestSuiteTable.Rows.Count == 0) {
            DialogResult = DialogResult.None;
            return;
        }
        Test = new() {
            Config = new() {
                AttemptRollback = ConfigRollback.Checked,
                Elevated = ConfigElevate.Checked,
                Target = target,
                WhatIf = ConfigWhatIf.Checked
            },
            Order = order,
            Phase = phase,
            TestId = testId
        };
        foreach (DataRow dr in TestSuiteTable.Rows) {
            if (dr["TestCase"] is not TestCase tc) {
                continue;
            }
            Test.TestSuite.Add(tc);
        }
        foreach (DataRow dr in RecoverySuiteTable.Rows) {
            if (dr["TestCase"] is not TestCase tc) {
                continue;
            }
            Test.RecoverySuite.Add(tc);
        }
        Close();
    }

    private void Cancel_Click(object sender, EventArgs e) {
        Test = null;
        Close();
    }
    private void CheckValidity() {
        testSource.ResetBindings(false);
        recoverySource.ResetBindings(false);
        Ok.Enabled = TestSuiteTable.Rows.Count > 0 &&
            int.TryParse(Phase.Text, out int phase) && phase > 0 &&
            int.TryParse(Order.Text, out int order) && order > 0 &&
            ConfigTarget.SelectedItem is TestTarget target && target != TestTarget.Invalid;
        UpdateSelectionButtons();
    }
    private static DataGridViewRow? GetSelectedRow(DataGridView target) {
        if (target.SelectedRows.Count != 1) {
            return null;
        }
        return target.SelectedRows.OfType<DataGridViewRow>().FirstOrDefault();
    }
}
