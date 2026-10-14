using SapphTools.DHA.Stig.Remediator.TestSled.SledCatalog;
using System.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms;
public partial class TestSled : Form {
    private static class Messages {
        public const string Unsigned = "The definition file has not been signed. TestSled is not available on unsigned TestArrays.";
        public const string Version = "The results were obtained against a previous version. TestSled must be run again for valid results.";
        public const string NoDefinition = "No TestSled definition file has been selected, or the selected file could not be parsed.";
        public const string NoResults = "No TestSled results file has been selected, or the selected file could not be parsed.";
        public const string Fail = "The selected results file shows one or more test failures.";
        public const string Mismatch = "The selected results file does not match the definition file.";
        public const string Catalog = "No valid catalog file was found in the directory with the definition file.";
        public const string None = "";
        public static string GetMessage(TestSledState state) {
            return state switch {
                var x when x.HasFlag(TestSledState.NoDefinition) => NoDefinition,
                var x when x.HasFlag(TestSledState.NoResults) => NoResults,
                var x when x.HasFlag(TestSledState.DefinitionMismatch) => Mismatch,
                var x when x.HasFlag(TestSledState.VersionMismatch) => Version,
                var x when x.HasFlag(TestSledState.ResultsFail) => Fail,
                var x when x.HasFlag(TestSledState.NoCatalog) => Catalog,
                var x when x.HasFlag(TestSledState.Unsigned) => Unsigned,
                TestSledState.Valid => None,
            };
        }
    }
    private FileInfo? testSledDefinitionFI;
    private FileInfo? testSledResultsFI;
    private TestArray? testArrayDefinition;
    private TestArray? testArrayResults;
    private readonly DataTable ResultsTable;
    private readonly BindingSource source;
    private TestSledState state = TestSledState.Valid;
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
        //if (IsExistingLocalJsonFile(testSledDefinitionFI) &&
        //        testSledDefinitionFI is FileInfo definition &&
        //        TryLoadDefinition(definition.FullName, out TestArray? loadedDefinition, out _)) {
        //    testArrayDefinition = loadedDefinition;
        //}
        //if (IsExistingLocalJsonFile(testSledResultsFI) && testSledResultsFI is FileInfo results) {
        //    testArrayResults = TestArray.LoadFromJson(results.FullName);
        //}
        //LoadFromJson does all this work, I think. DRY
        testArrayDefinition = TestArray.LoadFromJson(Settings.Default.TestSledDefinitionPath);
        testArrayResults = TestArray.LoadFromJson(Settings.Default.TestSledResultsPath);
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
        //TestSledDefinitionPath.Text = HasSelectedJsonPath(testSledDefinitionFI)
        //    ? testSledDefinitionFI!.FullName : string.Empty;
        //TestSledResultsPath.Text = HasSelectedJsonPath(testSledResultsFI)
        //    ? testSledResultsFI!.FullName : string.Empty;
        // I don't need to care if the file specifically ends in .json just because that's how I define OFD
        // Correct content matters more than metadata
        //TestSledDefinitionPath.Text = testSledDefinitionFI?.FullName ?? string.Empty;
        //TestSledResultsPath.Text = testSledResultsFI?.FullName ?? string.Empty;
        //RefreshResults();
        Verify(testSledDefinitionFI?.FullName, testSledResultsFI?.FullName);
    }

    private void DefBrowse_Click(object sender, EventArgs e) {
        using OpenFileDialog dialog = CreateJsonFileDialog(testSledDefinitionFI);
        dialog.Title = "Select Test Sled definition";
        if (dialog.ShowDialog() != DialogResult.OK) {
            return;
        }
        //if (!TryLoadDefinition(dialog.FileName, out TestArray? definition, out string error)) {
        //    MessageBox.Show(this, error, "Invalid Test Sled definition",
        //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    return;
        //}

        //// A readable definition is editable even when its catalog is absent,
        //// its signature is invalid, or its build IDs are out of date.
        //testSledDefinitionFI = new FileInfo(dialog.FileName);
        //testArrayDefinition = definition;
        //TestSledDefinitionPath.Text = testSledDefinitionFI.FullName;
        //Settings.Default.TestSledDefinitionPath = testSledDefinitionFI.FullName;
        //Settings.Default.Save();
        //RefreshResults();
        //I think Verify will handle all this
        Verify(dialog.FileName, TestSledResultsPath.Text);
    }

    private void ResultBrowse_Click(object sender, EventArgs e) {
        using OpenFileDialog dialog = CreateJsonFileDialog(testSledResultsFI);
        dialog.Title = "Select Test Sled results";
        if (dialog.ShowDialog() != DialogResult.OK) {
            return;
        }
        //I think Verify will handle all this
        Verify(TestSledDefinitionPath.Text, dialog.FileName);
    }

    // Parsing and checking file existence are the only prerequisites for editing.
    // Signature and MVID checks are separate qualification predicates.
    //private static bool TryLoadDefinition(string path, out TestArray? definition, out string error) {
    //    definition = null;
    //    error = string.Empty;
    //    try {
    //        FileInfo file = new(path);
    //        if (!file.Exists || !file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase)) {
    //            throw new InvalidDataException("Select an existing JSON definition file.");
    //        }
    //        definition = TestArray.LoadFromJson(file.FullName)
    //            ?? throw new InvalidDataException("The definition file could not be deserialized.");
    //        return true;
    //    } catch (Exception ex) {
    //        error = ex.Message;
    //        return false;
    //    }
    //}

    //Kinda like this better. Thinking about what parts to replace with this
    private void Verify(string? defPath, string? resPath) {
        Status.Text = Messages.None;
        state = TestSledState.Valid;
        if (string.IsNullOrWhiteSpace(defPath) || 
                !File.Exists(defPath) ||
                TestArray.LoadFromJson(defPath) is not TestArray def) {
            state |= TestSledState.NoDefinition;
        } else {
            testSledDefinitionFI = new(defPath);
            testArrayDefinition = def;
            TestSledDefinitionPath.Text = defPath;
            Settings.Default.TestSledDefinitionPath = defPath;
            Settings.Default.Save();
        }
        if (string.IsNullOrWhiteSpace(resPath) ||
                !File.Exists(resPath) || 
                TestArray.LoadFromJson(resPath) is not TestArray res) {
            state |= TestSledState.NoResults;
        } else if (testSledDefinitionFI is not null && testArrayDefinition is not null) {
            testSledResultsFI = new FileInfo(resPath);
            testArrayResults = res;
            TestSledResultsPath.Text = resPath;
            Settings.Default.TestSledResultsPath = defPath;
            Settings.Default.Save();
            RefreshResults();
            string catPath = Path.Combine(testSledDefinitionFI.DirectoryName!, "TestSled.cat");
            if (!res.BuildsMatchCurrent) {
                state |= TestSledState.VersionMismatch;
            }
            if (!testArrayDefinition.DefinitionEquals(res)) {
                state |= TestSledState.DefinitionMismatch;
            }
            if (!File.Exists(catPath)) {
                state |= TestSledState.NoCatalog | TestSledState.Unsigned;
            } else if (!CatalogApi.VerifyCatalogFile(catPath, testSledDefinitionFI.FullName)) {
                state |= TestSledState.Unsigned;
            }
            if (!res.Success) {
                state |= TestSledState.ResultsFail;
            }
        }
        Status.Text = Messages.GetMessage(state);
    }
    private static bool VerifyDefinition(FileInfo definitionFile, TestArray definition) {
        try {
            if (definition.Tests is null || definition.Tests.Count == 0 ||
                    definition.Tests.Any(test => test is null) || 
                    !definition.BuildsMatchCurrent) {
                return false;
            }
            // A refreshed editor copy is not verified until the bytes on disk and
            // their signed catalog have actually been regenerated.
            TestArray? onDisk = TestArray.LoadFromJson(definitionFile.FullName);
            if (onDisk?.Tests is null || onDisk.Tests.Count == 0 ||
                    onDisk.Tests.Any(test => test is null) ||
                    !definition.DefinitionEquals(onDisk)) {
                return false;
            }
            string catalogPath = Path.Combine(definitionFile.DirectoryName
                ?? throw new InvalidDataException("Cannot determine the definition directory."), "TestSled.cat");
            if (!File.Exists(catalogPath)) return false;
            return CatalogApi.VerifyCatalogFile(catalogPath, definitionFile.FullName);
        } catch {
            return false;
        }
    }

    private void RefreshResults() {
        //DefinitionVerified = testArrayDefinition is not null &&
        //    testSledDefinitionFI is FileInfo definition &&
        //    VerifyDefinition(definition, testArrayDefinition);
        //ResultsValid = false;
        //if (DefinitionVerified && testArrayResults is not null &&
        //        testSledDefinitionFI is FileInfo definitionPath) {
        //    try {
        //        ResultsValid = testArrayResults.ResultsAreValid(definitionPath.FullName);
        //    } catch {
        //        ResultsValid = false;
        //    }
        //}

        //definitionStatus.Text = testArrayDefinition is null
        //    ? HasSelectedJsonPath(testSledDefinitionFI) ? "Not loaded" : "Not selected"
        //    : DefinitionVerified ? "Verified" : "Unverified";
        //definitionStatus.ForeColor = testArrayDefinition is null && !HasSelectedJsonPath(testSledDefinitionFI)
        //    ? SystemColors.GrayText : DefinitionVerified ? Color.ForestGreen : Color.Firebrick;

        //resultStatus.Text = testArrayResults is null
        //    ? HasSelectedJsonPath(testSledResultsFI) ? "Not loaded" : "Not selected"
        //    : ResultsValid ? "Valid" : "Invalid";
        //resultStatus.ForeColor = testArrayResults is null && !HasSelectedJsonPath(testSledResultsFI)
        //    ? SystemColors.GrayText : ResultsValid ? Color.ForestGreen : Color.Firebrick;
        if (testArrayResults is null) {
            return;
        }
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

    //private static bool HasSelectedJsonPath(FileInfo? file) =>
    //    file is not null && file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase);

    // Never probe a remembered network location on the UI thread. If the user
    // explicitly picks it in the file dialog, normal loading is still supported.
    //private static bool IsExistingLocalJsonFile(FileInfo? file) =>
    //    HasSelectedJsonPath(file) && IsLocalPath(file!.FullName) && file.Exists;
    private static OpenFileDialog CreateJsonFileDialog(FileInfo? previous) => CreateJsonFileDialog(previous?.DirectoryName);
    private static OpenFileDialog CreateJsonFileDialog(string? previous) {
        string startPath = previous ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Task timeout = Task.Delay(500);
        static async Task FastCheck(string server) {
            CancellationTokenSource cts = new(100);
            _ = await System.Net.Dns.GetHostEntryAsync(server, cts.Token);
            System.Net.NetworkInformation.Ping p = new();
            cts = new(100);
            if ((await p.SendPingAsync(server, TimeSpan.FromMilliseconds(100), new byte[1], null, cts.Token)).Status != 
                    System.Net.NetworkInformation.IPStatus.Success) {
                throw new Exception();
            }
        }
        if (Path.GetPathRoot(startPath) is string root) {
            if (root.StartsWith("\\\\")) {
                root = root[2..root.IndexOf('\\', 2)];
                try {
                    if (Task.WaitAny(timeout, FastCheck(root)) == 0) {
                        startPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }
                } catch {
                    startPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            } else if (!DriveInfo.GetDrives().Any(d => d.Name.Equals(root[..3], StringComparison.OrdinalIgnoreCase) && d.IsReady)) {
                startPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }
        OpenFileDialog dialog = new() {
            Filter = "JSON files (*.json)|*.json",
            FilterIndex = 1,
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            InitialDirectory = startPath
        };
        return dialog;
    }
    //private static string GetSafeInitialDirectory(FileInfo? previous) {
    //    // An unreachable UNC share or mapped network drive can freeze OpenFileDialog
    //    // before it appears. Do not probe those paths on the UI thread.
    //    string? preferred = previous?.DirectoryName;
    //    if (!string.IsNullOrWhiteSpace(preferred) && IsLocalPath(preferred) &&
    //            Directory.Exists(preferred)) {
    //        return preferred;
    //    }
    //    string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    //    if (!string.IsNullOrWhiteSpace(documents) && IsLocalPath(documents) &&
    //            Directory.Exists(documents)) {
    //        return documents;
    //    }
    //    return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    //}

    //private static bool IsLocalPath(string path) {
    //    try {
    //        if (path.StartsWith(@"\\", StringComparison.Ordinal) ||
    //                path.StartsWith("//", StringComparison.Ordinal) ||
    //                !Path.IsPathFullyQualified(path)) {
    //            return false;
    //        }
    //        if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) && !uri.IsFile) {
    //            return false;
    //        }
    //        string? root = Path.GetPathRoot(path);
    //        return root is not null && new DriveInfo(root).DriveType != DriveType.Network;
    //    } catch {
    //        return false;
    //    }
    //}
}
