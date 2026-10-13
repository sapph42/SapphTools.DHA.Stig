using System.Data;

namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms;
public partial class ArrayEditor : Form {
    private readonly TestArray? OriginalArray;
    private readonly DataTable TestTable;
    private readonly BindingSource source;
    private readonly TestArray ActiveArray;
    public TestArray? TestArray;
    public ArrayEditor() {
        InitializeComponent();
        Tests.AutoGenerateColumns = false;
        Tests.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "TestId",
                HeaderText = "TestId",
                Name = "TestId",
                ReadOnly = true,
                ValueType = typeof(int),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Phase",
                HeaderText = "Phase",
                Name = "Phase",
                ReadOnly = true,
                ValueType = typeof(int),
                Visible = true,
            },
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
                AutoSizeMode= DataGridViewAutoSizeColumnMode.Fill,
                DataPropertyName = "TestSuiteCount",
                HeaderText = "# of Test Items",
                Name = "TestSuiteCount",
                ReadOnly = true,
                ValueType = typeof(int),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.Fill,
                DataPropertyName = "RecoverySuiteCount",
                HeaderText = "# of Recovery Items",
                Name = "RecoverySuiteCount",
                ReadOnly = true,
                ValueType = typeof(int),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell() {
                    ValueType = typeof(Test)
                },
                DataPropertyName = "Test",
                Name = "Test",
                ReadOnly = true,
                ValueType = typeof(Test),
                Visible = false
            }
        ]);
        TestTable = new();
        TestTable.Columns.AddRange([
            new("TestId", typeof(int)),
            new("Phase", typeof(int)),
            new("Order", typeof(int)),
            new("TestSuiteCount", typeof(int)),
            new("RecoverySuiteCount", typeof(int)),
            new("Test", typeof(Test))
        ]);
        TestTable.PrimaryKey = [TestTable.Columns["TestId"]!];
        source = new() {
            DataSource = TestTable
        };
        ActiveArray = new();
    }
    public ArrayEditor(TestArray testArray) : this() {
        OriginalArray = testArray;
        ActiveArray = testArray.Clone();
        LocalTarget.Text = ActiveArray.LocalTarget;
        DevTargets.Text = string.Join(Environment.NewLine, ActiveArray.RemoteDevTarget);
        ProdTargets.Text = string.Join(Environment.NewLine, ActiveArray.RemoteProdTarget);
        foreach (Test test in OriginalArray.Tests) {
            TestTable.Rows.Add(test.TestId, test.Phase, test.Order, test.TestSuite.Count, test.RecoverySuite.Count, test.Clone());
        }
    }
    private void ArrayEditor_Load(object sender, EventArgs e) {
        Tests.DataSource = source;
    }
    private void AddTest_Click(object sender, EventArgs e) {
        TestEditor testEditor = new();
        if (testEditor.ShowDialog() == DialogResult.OK && testEditor.Test is Test test) {
            test.TestId = ActiveArray.Tests.Max(t => t.TestId) + 1;
            test.Order = ActiveArray.Tests.Where(t => t.Phase == test.Phase).Max(t => t.Order) + 1;
            if (ActiveArray.Tests.Add(test)) {
                TestTable.Rows.Add(test.TestId, test.Phase, test.Order, test.TestSuite.Count, test.RecoverySuite.Count, test.Clone());
            }
        }
        CheckValidity();
    }
    private void EditTest_Click(object sender, EventArgs e) {
        if (GetSelectedRow() is not DataGridViewRow row || TestTable.Rows.Find(row.Cells["TestId"].Value) is not DataRow dRow) {
            return;
        }
        Test test = ((Test)row.Cells["Test"].Value).Clone();
        TestEditor testEditor = new(test);
        if (testEditor.ShowDialog() == DialogResult.OK && testEditor.Test is not null) {
            test = testEditor.Test.Clone();
            dRow["TestId"] = test.TestId;
            dRow["Phase"] = test.Phase;
            dRow["Order"] = test.Order;
            dRow["TestSuiteCount"] = test.TestSuite.Count;
            dRow["RecoverySuiteCount"] = test.RecoverySuite.Count;
            dRow["Test"] = test;
        }
        CheckValidity();
    }
    private void RemoveTest_Click(object sender, EventArgs e) {
        if (GetSelectedRow() is not DataGridViewRow row) {
            return;
        }
        Test toRemove = (Test)row.Cells["Test"].Value;
        if (TestTable.Rows.Find(toRemove.TestId) is DataRow dataRow) {
            ActiveArray.Tests.Remove(toRemove);
            foreach (Test test in ActiveArray.Tests.Where(t => t.Phase == toRemove.Phase)) {
                if (test.Order > toRemove.Order) {
                    test.Order--;
                    if (TestTable.Rows.Find(test.TestId) is DataRow drSibling) {
                        drSibling["Order"] = test.Order;
                    }
                }
            }
            TestTable.Rows.Remove(dataRow);
        }
        CheckValidity();
    }
    private void Ok_Click(object sender, EventArgs e) {
        ActiveArray.LocalTarget = LocalTarget.Text;
        ActiveArray.RemoteDevTarget = [..DevTargets.Lines];
        ActiveArray.RemoteProdTarget = [.. ProdTargets.Lines];
        ActiveArray.Tests.Clear();
        foreach (DataRow row in TestTable.Rows.Cast<DataRow>()) {
            ActiveArray.Tests.Add((Test)row["Test"]);
        }
        TestArray = ActiveArray.Clone();
        Close();
    }
    private void Cancel_Click(object sender, EventArgs e) {
        TestArray = null;
        Close();
    }
    private void Tests_SelectionChanged(object sender, EventArgs e) {
        if (GetSelectedRow() is not null) {
            EditTest.Enabled = true;
            RemoveTest.Enabled = true;
        } else {
            EditTest.Enabled = false;
            RemoveTest.Enabled = false;
        }
    }
    private void CheckValidity() {
        source.ResetBindings(false);
        Ok.Enabled = !string.IsNullOrWhiteSpace(LocalTarget.Text) &&
            !string.IsNullOrWhiteSpace(DevTargets.Text) &&
            !string.IsNullOrWhiteSpace(ProdTargets.Text) &&
            Tests.Rows.Count > 0;
    }
    private DataGridViewRow? GetSelectedRow() {
        if (Tests.SelectedRows.Count != 1) {
            return null;
        }
        return Tests.SelectedRows.OfType<DataGridViewRow>().FirstOrDefault();
    }
}
