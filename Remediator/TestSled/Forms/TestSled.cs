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
    // Loading a definition is not evidence that its signature or build is valid.
    public bool DefinitionVerified { get; private set; }
    public bool ResultsValid { get; private set; }
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
        if (IsExistingLocalJsonFile(testSledDefinitionFI) &&
                testSledDefinitionFI is FileInfo definition &&
                TryLoadDefinition(definition.FullName, out TestArray? loadedDefinition, out _)) {
            testArrayDefinition = loadedDefinition;
        }
        if (IsExistingLocalJsonFile(testSledResultsFI) && testSledResultsFI is FileInfo results) {
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
        TestSledDefinitionPath.Text = HasSelectedJsonPath(testSledDefinitionFI)
            ? testSledDefinitionFI!.FullName : string.Empty;
        TestSledResultsPath.Text = HasSelectedJsonPath(testSledResultsFI)
            ? testSledResultsFI!.FullName : string.Empty;
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

        // A readable definition is editable even when its catalog is absent,
        // its signature is invalid, or its build IDs are out of date.
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

    // Parsing and checking file existence are the only prerequisites for editing.
    // Signature and MVID checks are separate qualification predicates.
    private static bool TryLoadDefinition(string path, out TestArray? definition, out string error) {
        definition = null;
        error = string.Empty;
        try {
            FileInfo file = new(path);
            if (!file.Exists || !file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidDataException("Select an existing JSON definition file.");
            }
            definition = TestArray.LoadFromJson(file.FullName)
                ?? throw new InvalidDataException("The definition file could not be deserialized.");
            return true;
        } catch (Exception ex) {
            error = ex.Message;
            return false;
        }
    }

    private static bool VerifyDefinition(FileInfo definitionFile, TestArray definition) {
        try {
            if (definition.Tests is null || definition.Tests.Count == 0 ||
                    definition.Tests.Any(test => test is null || !test.BuildsMatchCurrent)) {
                return false;
            }
            // A refreshed editor copy is not verified until the bytes on disk and
            // their signed catalog have actually been regenerated.
            TestArray? onDisk = TestArray.LoadFromJson(definitionFile.FullName);
            if (onDisk?.Tests is null || onDisk.Tests.Count == 0 ||
                    onDisk.Tests.Any(test => test is null || !test.BuildsMatchCurrent) ||
                    !definition.DefinitionEquals(onDisk)) {
                return false;
            }
            string catalogPath = Path.Combine(definitionFile.DirectoryName
                ?? throw new InvalidDataException("Cannot determine the definition directory."), "TestSled.cat");
            if (!File.Exists(catalogPath)) return false;
            CatalogApi.VerifyCatalogMember(catalogPath, definitionFile.FullName);
            return true;
        } catch {
            return false;
        }
    }

    private void RefreshResults() {
        DefinitionVerified = testArrayDefinition is not null &&
            testSledDefinitionFI is FileInfo definition &&
            VerifyDefinition(definition, testArrayDefinition);
        ResultsValid = false;
        if (DefinitionVerified && testArrayResults is not null &&
                testSledDefinitionFI is FileInfo definitionPath) {
            try {
                ResultsValid = testArrayResults.ResultsAreValid(definitionPath.FullName);
            } catch {
                ResultsValid = false;
            }
        }

        definitionStatus.Text = testArrayDefinition is null
            ? HasSelectedJsonPath(testSledDefinitionFI) ? "Not loaded" : "Not selected"
            : DefinitionVerified ? "Verified" : "Unverified";
        definitionStatus.ForeColor = testArrayDefinition is null && !HasSelectedJsonPath(testSledDefinitionFI)
            ? SystemColors.GrayText : DefinitionVerified ? Color.ForestGreen : Color.Firebrick;

        resultStatus.Text = testArrayResults is null
            ? HasSelectedJsonPath(testSledResultsFI) ? "Not loaded" : "Not selected"
            : ResultsValid ? "Valid" : "Invalid";
        resultStatus.ForeColor = testArrayResults is null && !HasSelectedJsonPath(testSledResultsFI)
            ? SystemColors.GrayText : ResultsValid ? Color.ForestGreen : Color.Firebrick;

        ResultsTable.Clear();
        if (testArrayResults?.Tests is not null) {
            foreach (Test test in testArrayResults.Tests) {
                if (test is null) continue;
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

    private static bool HasSelectedJsonPath(FileInfo? file) =>
        file is not null && file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase);

    // Never probe a remembered network location on the UI thread. If the user
    // explicitly picks it in the file dialog, normal loading is still supported.
    private static bool IsExistingLocalJsonFile(FileInfo? file) =>
        HasSelectedJsonPath(file) && IsLocalPath(file!.FullName) && file.Exists;

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
