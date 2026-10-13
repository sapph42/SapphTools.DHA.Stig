using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Rule = SapphTools.DHA.Stig.Common.Classes.Rule;

namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms;
public partial class TestEditor : Form {
    private readonly Test? OriginalTest;
    private readonly DataTable TestSuiteTable;
    private readonly DataTable RecoverySuiteTable;
    private readonly BindingSource testSource;
    private readonly BindingSource recoverySource;
    private readonly Test ActiveTest;
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
                DataPropertyName = "RuleId",
                HeaderText = "RuleId",
                Name = "RuleId",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Description",
                HeaderText = "Description",
                Name = "Description",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "CountOfSettings",
                HeaderText = "O# of Settings",
                Name = "CountOfSettings",
                ReadOnly = true,
                ValueType = typeof(int),
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
                DataPropertyName = "RuleId",
                HeaderText = "RuleId",
                Name = "RuleId",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Description",
                HeaderText = "Description",
                Name = "Description",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "CountOfSettings",
                HeaderText = "O# of Settings",
                Name = "CountOfSettings",
                ReadOnly = true,
                ValueType = typeof(int),
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
        TestSuiteTable = new();
        DataColumn testOrder = new("Order", typeof(int));
        TestSuiteTable.Columns.AddRange([
            testOrder,
            new("RuleId", typeof(string)),
            new("Description", typeof(string)),
            new("CountOfSettings", typeof(int)),
            new("TestCase", typeof(TestCase))
        ]);
        RecoverySuiteTable = new();
        DataColumn recoveryOrder = new("Order", typeof(int));
        RecoverySuiteTable.Columns.AddRange([
            recoveryOrder,
            new("RuleId", typeof(string)),
            new("Description", typeof(string)),
            new("CountOfSettings", typeof(int)),
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
        //TODO: populate ConfigTarget.Items with TestTarget enum
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
            TestSuiteTable.Rows.Add(tc.Order, tc.Rule.RuleId, tc.Rule.Description, tc.Rule.Settings.Count, tc);
        }
        foreach (TestCase tc in ActiveTest.RecoverySuite) {
            RecoverySuiteTable.Rows.Add(tc.Order, tc.Rule.RuleId, tc.Rule.Description, tc.Rule.Settings.Count, tc);
        }
        CheckValidity();
    }

    private void AddTestCase_Click(object sender, EventArgs e) {
        RuleEditor ruleEditor = new();
        if (ruleEditor.ShowDialog() == DialogResult.OK && ruleEditor.ActiveRule is Rule rule) {
            TestCase tc = new() {
                Order = TestSuiteTable.Rows.Cast<DataRow>().Select(r => (int)r["Order"]).Max() + 1,
                Rule = rule.Clone()
            };
            TestSuiteTable.Rows.Add(tc.Order, rule.RuleId, rule.Description, rule.Settings.Count, tc);
        }
        CheckValidity();
    }

    private void EditTestCase_Click(object sender, EventArgs e) {
        if (GetSelectedRow(TestSuite) is not DataGridViewRow row || 
                TestSuiteTable.Rows.Find(row.Cells["Order"].Value) is not DataRow dRow) {
            return;
        }
        TestCase targetCase = ((TestCase)dRow["TestCase"]).Clone();
        RuleEditor ruleEditor = new(targetCase.Rule.Clone());
        if (ruleEditor.ShowDialog() == DialogResult.OK && ruleEditor.ActiveRule is Rule rule) {
            TestCase tc = new() {
                Order = targetCase.Order,
                Rule = rule.Clone()
            };
            dRow["RuleId"] = rule.RuleId;
            dRow["Description"] = rule.Description;
            dRow["CountOfSettings"] = rule.Settings.Count;
            dRow["TestCase"] = tc;
        }
        CheckValidity();
    }

    private void RemoveTestCase_Click(object sender, EventArgs e) {
        if (GetSelectedRow(TestSuite) is not DataGridViewRow row) {
            return;
        }
        TestCase toRemove = (TestCase)row.Cells["TestCase"].Value;
        if (TestSuiteTable.Rows.Find(toRemove.Order) is DataRow dataRow) {
            ActiveTest.TestSuite.Remove(toRemove);
            foreach (TestCase testCase in ActiveTest.TestSuite.Where(t => t.Order > toRemove.Order)) {
                if (TestSuiteTable.Rows.Find(testCase.Order--) is DataRow drSibling) {
                    drSibling["Order"] = testCase.Order;
                }
            }
            TestSuiteTable.Rows.Remove(dataRow);
        }
        CheckValidity();
    }

    private void AddRecoveryCase_Click(object sender, EventArgs e) {
        RuleEditor ruleEditor = new();
        if (ruleEditor.ShowDialog() == DialogResult.OK && ruleEditor.ActiveRule is Rule rule) {
            TestCase tc = new() {
                Order = RecoverySuiteTable.Rows.Cast<DataRow>().Select(r => (int)r["Order"]).Max() + 1,
                Rule = rule.Clone()
            };
            RecoverySuiteTable.Rows.Add(tc.Order, rule.RuleId, rule.Description, rule.Settings.Count, tc);
        }
        CheckValidity();
    }

    private void EditRecoveryCase_Click(object sender, EventArgs e) {
        if (GetSelectedRow(RecoverySuite) is not DataGridViewRow row ||
                RecoverySuiteTable.Rows.Find(row.Cells["Order"].Value) is not DataRow dRow) {
            return;
        }
        TestCase targetCase = ((TestCase)dRow["TestCase"]).Clone();
        RuleEditor ruleEditor = new(targetCase.Rule.Clone());
        if (ruleEditor.ShowDialog() == DialogResult.OK && ruleEditor.ActiveRule is Rule rule) {
            TestCase tc = new() {
                Order = targetCase.Order,
                Rule = rule.Clone()
            };
            dRow["RuleId"] = rule.RuleId;
            dRow["Description"] = rule.Description;
            dRow["CountOfSettings"] = rule.Settings.Count;
            dRow["TestCase"] = tc;
        }
        CheckValidity();
    }

    private void RemoveRecoveryCase_Click(object sender, EventArgs e) {
        if (GetSelectedRow(RecoverySuite) is not DataGridViewRow row) {
            return;
        }
        TestCase toRemove = (TestCase)row.Cells["TestCase"].Value;
        if (RecoverySuiteTable.Rows.Find(toRemove.Order) is DataRow dataRow) {
            ActiveTest.TestSuite.Remove(toRemove);
            foreach (TestCase testCase in ActiveTest.TestSuite.Where(t => t.Order > toRemove.Order)) {
                if (RecoverySuiteTable.Rows.Find(testCase.Order--) is DataRow drSibling) {
                    drSibling["Order"] = testCase.Order;
                }
            }
            RecoverySuiteTable.Rows.Remove(dataRow);
        }
        CheckValidity();
    }

    private void MoveTestUp_Click(object sender, EventArgs e) {
        //TODO: Implement reordering
    }

    private void MoveTestDown_Click(object sender, EventArgs e) {
        //TODO: Implement reordering
    }

    private void MoveRecoveryUp_Click(object sender, EventArgs e) {
        //TODO: Implement reordering
    }

    private void MoveRecoveryDown_Click(object sender, EventArgs e) {
        //TODO: Implement reordering
    }

    private void Ok_Click(object sender, EventArgs e) {
        Test = new() {
            Build = ActiveTest.Build,
            Config = new() {
                AttemptRollback = ConfigRollback.Checked,
                Elevated = ConfigElevate.Checked,
                Target = (TestTarget)(ConfigTarget.SelectedItem ?? TestTarget.Local),
                WhatIf = ConfigWhatIf.Checked
            },
            Order = int.Parse(Order.Text),
            Phase = int.Parse(Phase.Text),
            TestId = int.Parse(TestId.Text)
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
        Ok.Enabled = TestSuiteTable.Rows.Count > 0;
    }
    private DataGridViewRow? GetSelectedRow(DataGridView target) {
        if (target.SelectedRows.Count != 1) {
            return null;
        }
        return target.SelectedRows.OfType<DataGridViewRow>().FirstOrDefault();
    }
}
