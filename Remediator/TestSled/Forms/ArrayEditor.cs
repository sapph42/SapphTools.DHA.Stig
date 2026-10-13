using System.Data;

namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms;
public partial class ArrayEditor : Form {
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
        Tests.DataSource = source;
        TestTable.DefaultView.Sort = "Phase ASC, Order ASC";
        LocalTarget.Text = ActiveArray.LocalTarget;
        LocalTarget.TextChanged += (_, _) => CheckValidity();
        DevTargets.TextChanged += (_, _) => CheckValidity();
        ProdTargets.TextChanged += (_, _) => CheckValidity();
        CheckValidity();
    }
    public ArrayEditor(TestArray testArray) : this() {
        ActiveArray = testArray.Clone();
        LocalTarget.Text = ActiveArray.LocalTarget;
        DevTargets.Text = string.Join(Environment.NewLine, ActiveArray.RemoteDevTarget);
        ProdTargets.Text = string.Join(Environment.NewLine, ActiveArray.RemoteProdTarget);
        foreach (Test test in ActiveArray.Tests) {
            AddTestRow(test.Clone(withoutResults: true));
        }
        CheckValidity();
    }
    private void ArrayEditor_Load(object sender, EventArgs e) {
        CheckValidity();
    }
    private void AddTest_Click(object sender, EventArgs e) {
        TestEditor testEditor = new();
        if (testEditor.ShowDialog() == DialogResult.OK && testEditor.Test is Test test) {
            test.TestId = TestTable.Rows.Cast<DataRow>()
                .Select(row => (int)row["TestId"]).DefaultIfEmpty(0).Max() + 1;
            test.Order = TestTable.Rows.Cast<DataRow>()
                .Where(row => (int)row["Phase"] == test.Phase)
                .Select(row => (int)row["Order"]).DefaultIfEmpty(0).Max() + 1;
            AddTestRow(test.Clone(withoutResults: true));
        }
        CheckValidity();
    }
    private void EditTest_Click(object sender, EventArgs e) {
        if (GetSelectedRow() is not DataGridViewRow row || TestTable.Rows.Find(row.Cells["TestId"].Value) is not DataRow dRow) {
            return;
        }
        Test previous = (Test)dRow["Test"];
        TestEditor testEditor = new(previous.Clone(withoutResults: true));
        if (testEditor.ShowDialog() == DialogResult.OK && testEditor.Test is Test updated) {
            updated = updated.Clone(withoutResults: true);
            updated.TestId = previous.TestId;
            if (updated.Phase != previous.Phase) {
                updated.Order = TestTable.Rows.Cast<DataRow>()
                    .Where(item => item != dRow && (int)item["Phase"] == updated.Phase)
                    .Select(item => (int)item["Order"]).DefaultIfEmpty(0).Max() + 1;
            } else {
                updated.Order = previous.Order;
            }
            UpdateTestRow(dRow, updated);
            RenumberPhase(previous.Phase);
            if (updated.Phase != previous.Phase) RenumberPhase(updated.Phase);
        }
        CheckValidity();
    }
    private void RemoveTest_Click(object sender, EventArgs e) {
        if (GetSelectedRow() is not DataGridViewRow row) {
            return;
        }
        Test toRemove = (Test)row.Cells["Test"].Value;
        if (TestTable.Rows.Find(toRemove.TestId) is DataRow dataRow) {
            TestTable.Rows.Remove(dataRow);
            RenumberPhase(toRemove.Phase);
        }
        CheckValidity();
    }
    private void Ok_Click(object sender, EventArgs e) {
        CheckValidity();
        if (!Ok.Enabled) {
            DialogResult = DialogResult.None;
            return;
        }
        ActiveArray.LocalTarget = LocalTarget.Text.Trim();
        ActiveArray.RemoteDevTarget = GetTargets(DevTargets);
        ActiveArray.RemoteProdTarget = GetTargets(ProdTargets);
        ActiveArray.Tests.Clear();
        foreach (DataRow row in TestTable.Rows.Cast<DataRow>()) {
            // A saved definition is based on the current binaries. Prior execution
            // evidence cannot be carried across a definition edit or MVID update.
            if (!ActiveArray.Tests.Add(((Test)row["Test"]).Clone(
                    withoutResults: true, updateBuilds: true))) {
                throw new InvalidOperationException("The test array contains duplicate phase/order positions.");
            }
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
    private void AddTestRow(Test test) {
        TestTable.Rows.Add(test.TestId, test.Phase, test.Order,
            test.TestSuite.Count, test.RecoverySuite.Count, test);
    }

    private static void UpdateTestRow(DataRow row, Test test) {
        row["TestId"] = test.TestId;
        row["Phase"] = test.Phase;
        row["Order"] = test.Order;
        row["TestSuiteCount"] = test.TestSuite.Count;
        row["RecoverySuiteCount"] = test.RecoverySuite.Count;
        row["Test"] = test;
    }

    private void RenumberPhase(int phase) {
        int order = 1;
        foreach (DataRow row in TestTable.Rows.Cast<DataRow>()
                .Where(row => (int)row["Phase"] == phase)
                .OrderBy(row => (int)row["Order"])
                .ThenBy(row => (int)row["TestId"])) {
            Test test = ((Test)row["Test"]).Clone(withoutResults: true);
            test.Order = order;
            UpdateTestRow(row, test);
            order++;
        }
    }

    private static List<string> GetTargets(TextBox input) =>
        [.. input.Lines.Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    private void CheckValidity() {
        source.ResetBindings(false);
        var tests = TestTable.Rows.Cast<DataRow>().Select(row => (Test)row["Test"]).ToArray();
        List<string> dev = GetTargets(DevTargets);
        List<string> prod = GetTargets(ProdTargets);
        Ok.Enabled = !string.IsNullOrWhiteSpace(LocalTarget.Text) && tests.Length > 0 &&
            tests.All(test => test.Config.Target switch {
                TestTarget.Local => true,
                TestTarget.SingleRemoteDev => dev.Count >= 1,
                TestTarget.FullRemoteDev => dev.Count >= 1,
                TestTarget.SingleRemoteProd => prod.Count >= 1,
                TestTarget.FullRemoteProd => prod.Count >= 1,
                _ => false
            });
    }
    private DataGridViewRow? GetSelectedRow() {
        if (Tests.SelectedRows.Count != 1) {
            return null;
        }
        return Tests.SelectedRows.OfType<DataGridViewRow>().FirstOrDefault();
    }
}
