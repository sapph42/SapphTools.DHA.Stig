using System.Data;
using System.Drawing;
using SapphTools.DHA.Stig.Remediator.TestSled.SledCatalog;

namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms;
public partial class TestSled : Form {
    private FileInfo? testSledDefinitionFI;
    private FileInfo? testSledResultsFI;
    private TestArray? testArrayDefinition;
    private TestArray? testArrayResults;
    private readonly DataTable ResultsTable;
    private readonly BindingSource source;
    private bool valid = false;
    private readonly Label definitionStatus = new() { AutoSize = true, Location = new Point(904, 15) };
    private readonly Label resultStatus = new() { AutoSize = true, Location = new Point(904, 53) };
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
        if (testSledDefinitionFI is { Exists: true } definition &&
                TryLoadDefinition(definition.FullName, out TestArray? loadedDefinition, out _)) {
            testArrayDefinition = loadedDefinition;
        }
        if (testSledResultsFI is { Exists: true } results) {
            testArrayResults = TestArray.LoadFromJson(results.FullName);
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
        Tests.DataSource = source;
        TestSledDefinitionPath.ReadOnly = true;
        TestSledResultsPath.ReadOnly = true;
        Controls.Add(definitionStatus);
        Controls.Add(resultStatus);
    }

    private void TestSled_Load(object sender, EventArgs e) {
        TestSledDefinitionPath.Text = testSledDefinitionFI is { Exists: true } definition
            ? definition.FullName : string.Empty;
        TestSledResultsPath.Text = testSledResultsFI is { Exists: true } results
            ? results.FullName : string.Empty;
        RefreshResults();
    }

    private void DefBrowse_Click(object sender, EventArgs e) {
        using OpenFileDialog dialog = CreateJsonFileDialog(testSledDefinitionFI);
        dialog.Title = "Select Test Sled definition";
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (!TryLoadDefinition(dialog.FileName, out TestArray? definition, out string error)) {
            MessageBox.Show(this, error, "Invalid Test Sled definition",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Do not alter the current selection until all checks, including signature, succeed.
        testSledDefinitionFI = new FileInfo(dialog.FileName);
        testArrayDefinition = definition;
        TestSledDefinitionPath.Text = testSledDefinitionFI.FullName;
        Settings.Default.TestSledDefinitionPath = testSledDefinitionFI.FullName;
        Settings.Default.Save();
        RefreshResults();
    }

    private void ResultBrowse_Click(object sender, EventArgs e) {
        using OpenFileDialog dialog = CreateJsonFileDialog(testSledResultsFI);
        dialog.Title = "Select Test Sled results";
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try {
            FileInfo file = new(dialog.FileName);
            if (!file.Exists || !file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidDataException("Select an existing JSON results file.");
            }
            TestArray loaded = TestArray.LoadFromJson(file.FullName)
                ?? throw new InvalidDataException("The results file could not be deserialized.");
            // A well-formed but invalid result file is still shown, with a red status indicator.
            testSledResultsFI = file;
            testArrayResults = loaded;
            TestSledResultsPath.Text = file.FullName;
            Settings.Default.TestSledResultsPath = file.FullName;
            Settings.Default.Save();
            RefreshResults();
        } catch (Exception ex) {
            MessageBox.Show(this, ex.Message, "Unable to load Test Sled results",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static bool TryLoadDefinition(string path, out TestArray? definition, out string error) {
        definition = null;
        error = string.Empty;
        try {
            FileInfo file = new(path);
            if (!file.Exists || !file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidDataException("Select an existing JSON definition file.");
            }
            TestArray candidate = TestArray.LoadFromJson(file.FullName)
                ?? throw new InvalidDataException("The definition file could not be deserialized.");
            if (candidate.Tests.Count == 0 || candidate.Tests.Any(test => !test.BuildsMatchCurrent)) {
                throw new InvalidDataException(
                    "The definition is empty or one or more tests reference a different assembly build.");
            }
            string catalogPath = Path.Combine(file.DirectoryName
                ?? throw new InvalidDataException("Cannot determine the definition directory."), "TestSled.cat");
            if (!File.Exists(catalogPath)) {
                throw new FileNotFoundException("No TestSled.cat was found alongside the definition.", catalogPath);
            }
            CatalogApi.VerifyCatalogMember(catalogPath, file.FullName);
            definition = candidate;
            return true;
        } catch (Exception ex) {
            error = ex.Message;
            return false;
        }
    }

    private void RefreshResults() {
        valid = testArrayDefinition is not null && testArrayResults is not null &&
            testSledDefinitionFI is { Exists: true } definition &&
            testArrayResults.ResultsAreValid(definition.FullName);

        definitionStatus.Text = testArrayDefinition is not null
            ? "Verified" : testSledDefinitionFI is { Exists: true } ? "Invalid" : "Not selected";
        definitionStatus.ForeColor = testArrayDefinition is not null
            ? Color.ForestGreen : testSledDefinitionFI is { Exists: true } ? Color.Firebrick : SystemColors.GrayText;

        resultStatus.Text = testArrayResults is null
            ? testSledResultsFI is { Exists: true } ? "Invalid" : "Not selected"
            : valid ? "Valid" : "Invalid";
        resultStatus.ForeColor = testArrayResults is null && testSledResultsFI is not { Exists: true }
            ? SystemColors.GrayText : valid ? Color.ForestGreen : Color.Firebrick;

        ResultsTable.Clear();
        if (testArrayResults is not null) {
            foreach (Test test in testArrayResults.Tests) {
                ResultsTable.Rows.Add(
                    test.TestId,
                    test.Phase,
                    test.Order,
                    test.TestSuite.Count,
                    test.RecoverySuite.Count,
                    test.TestPass.HasValue ? test.TestPass.Value : DBNull.Value,
                    test.RecoveryPass.HasValue ? test.RecoveryPass.Value : DBNull.Value,
                    test);
            }
        }
        source.ResetBindings(false);
    }

    private static OpenFileDialog CreateJsonFileDialog(FileInfo? previous) {
        OpenFileDialog dialog = new() {
            Filter = "JSON files (*.json)|*.json",
            FilterIndex = 1,
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            InitialDirectory = GetSafeInitialDirectory(previous)
        };
        return dialog;
    }

    private static string GetSafeInitialDirectory(FileInfo? previous) {
        // An unreachable UNC share or mapped network drive can freeze OpenFileDialog
        // before it appears. Do not probe those paths on the UI thread.
        string? preferred = previous?.DirectoryName;
        if (!string.IsNullOrWhiteSpace(preferred) && IsLocalPath(preferred) &&
                Directory.Exists(preferred)) {
            return preferred;
        }
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (!string.IsNullOrWhiteSpace(documents) && IsLocalPath(documents) &&
                Directory.Exists(documents)) {
            return documents;
        }
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    private static bool IsLocalPath(string path) {
        try {
            if (path.StartsWith(@"\\", StringComparison.Ordinal) ||
                    path.StartsWith("//", StringComparison.Ordinal) ||
                    !Path.IsPathFullyQualified(path)) {
                return false;
            }
            if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) && !uri.IsFile) {
                return false;
            }
            string? root = Path.GetPathRoot(path);
            return root is not null && new DriveInfo(root).DriveType != DriveType.Network;
        } catch {
            return false;
        }
    }
}
