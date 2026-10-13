using System.Data;
using System.Text.Json;

namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms;
public partial class TestSled : Form {
    private FileInfo? testSledDefinitionFI;
    private FileInfo? testSledResultsFI;
    private TestArray? testArrayDefinition;
    private TestArray? testArrayResults;
    private readonly DataTable ResultsTable;
    private readonly BindingSource source;
    private bool valid = false;
    public TestSled() {
        InitializeComponent();
        string? procPath = Environment.ProcessPath;
        string? procDir = null;
        if (procPath is not null) {
            procDir = Path.GetDirectoryName(procPath);
        }
        Settings.Default.TestSledDefinitionPath ??= procDir;
        Settings.Default.TestSledResultsPath ??= procDir;
        Settings.Default.Save();
        if (!string.IsNullOrWhiteSpace(Settings.Default.TestSledDefinitionPath)) {
            testSledDefinitionFI = new(Settings.Default.TestSledDefinitionPath);
        }
        if (!string.IsNullOrWhiteSpace(Settings.Default.TestSledResultsPath)) {
            testSledResultsFI = new(Settings.Default.TestSledResultsPath);
        }
        if (testSledDefinitionFI is not null && testSledDefinitionFI.Exists) {
            try {
                testArrayDefinition = TestArray.LoadFromJson(testSledDefinitionFI.FullName);
                if (testSledResultsFI is not null && testSledResultsFI.Exists) {
                    testArrayResults = TestArray.LoadFromJson(testSledResultsFI.FullName);
                    valid = testArrayResults?.ResultsAreValid(testSledDefinitionFI.FullName) ?? false;
                }
            } catch { }
        }
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
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.Fill,
                DataPropertyName = "TestSuitePass",
                HeaderText = "Test Suite Pass",
                Name = "TestSuitePass",
                ReadOnly = true,
                ValueType = typeof(bool),
                Visible = true,
            },
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode= DataGridViewAutoSizeColumnMode.Fill,
                DataPropertyName = "RecoverySuitePass",
                HeaderText = "Recovery Suite Pass",
                Name = "RecoverySuitePass",
                ReadOnly = true,
                ValueType = typeof(bool),
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
        ResultsTable = new();
        ResultsTable.Columns.AddRange([
            new("TestId", typeof(int)),
            new("Phase", typeof(int)),
            new("Order", typeof(int)),
            new("TestSuiteCount", typeof(int)),
            new("RecoverySuiteCount", typeof(int)),
            new("TestSuitePass", typeof(bool)),
            new("RecoverySuitePass", typeof(bool)),
            new("Test", typeof(Test))
        ]);
        source = new() {
            DataSource = ResultsTable
        };
    }

    private void TestSled_Load(object sender, EventArgs e) {
        if (testSledDefinitionFI is not null && testArrayDefinition is not null) {
            TestSledDefinitionPath.Text = testSledDefinitionFI.FullName;
            // surface in UI that all Tests in Tests have BuildsMatchCurrent
        }
        if (testSledResultsFI is not null && testArrayResults is not null) {
            TestSledResultsPath.Text = testSledResultsFI.FullName;
            //TODO Surface state of valid in UI - maybe green/red border on TestSledResultsPath,
            // maybe a PictureBox control with Green Check or Red X Image Resources
            foreach (Test test in testArrayResults.Tests) {
                ResultsTable.Rows.Add(
                    test.TestId,
                    test.Phase,
                    test.Order,
                    test.TestSuite.Count,
                    test.RecoverySuite.Count,
                    test.TestPass ?? false,
                    test.RecoveryPass ?? false,
                    test
                );
            }
            source.ResetBindings(false);
        }
    }

    private void DefBrowse_Click(object sender, EventArgs e) {
        // TODO OpenFileDialog, limited to json files
        // OFD is SLOW SLOW SLOW if initial directory is remote and unreachable
        // Check if initialDir is Uri, and if so, do a fast check to see if it is reachable
        // If not, replace with something quasi-static and local (like My Documents, as it is now)
        // After OFD returns, confirm file exists, and attempt deserialization
        // Only after success write private fields and textbox Text property
        // check all tests have BuildsMatchCurrent
        // confirm catalog exists in same directory and confirm signature
    }

    private void ResultBrowse_Click(object sender, EventArgs e) {
        string initialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // TODO OpenFileDialog, limited to json files
        // OFD is SLOW SLOW SLOW if initial directory is remote and unreachable
        // Check if initialDir is Uri, and if so, do a fast check to see if it is reachable
        // If not, replace with something quasi-static and local (like My Documents, as it is now)
        // After OFD returns, confirm file exists, and attempt deserialization
        // Only after success write private fields and textbox Text property
        // also recalc valid with valid = testArrayResults.ResultsAreValid(testSledDefinitionFI.FullName);
        // and clear ResultsTable and repopulate
    }
}
