using SapphTools.DHA.Stig.Remediator.TestSled.SledCatalog;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Rem = SapphTools.DHA.Stig.Remediator.Classes.Remediators.Remediator;
using Roll = SapphTools.DHA.Stig.Remediator.Classes.Remediators.Rollbacker;

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
                _ => None,
            };
        }
    }
    private class StateNotify : INotifyPropertyChanged {
        private TestSledState _state;
        protected void OnPropertyChanged(PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(this, e);
        }
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "") {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        public TestSledState Value {
            get => _state;
            set {
                if (value != _state) {
                    _state = value;
                    OnPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
    private FileInfo? testSledDefinitionFI;
    private FileInfo? testSledResultsFI;
    private TestArray? testArrayDefinition;
    private TestArray? testArrayResults;
    private int maxPhase = 0;
    private int targetPhase = 0;
    private readonly DataTable ResultsTable;
    private readonly BindingSource source;
    private readonly StateNotify state = new();

    public TestSled() {
        InitializeComponent();
        state.PropertyChanged += (_, __) => {
            Status.Text = Messages.GetMessage(state.Value);
            RunTests.Enabled = state.Value switch {
                var x when x.HasFlag(TestSledState.NoDefinition) => false,
                var x when x.HasFlag(TestSledState.NoCatalog) => false,
                var x when x.HasFlag(TestSledState.Unsigned) => false,
                _ => true
            };
        };
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
        PhaseTarget.DropDownStyle = ComboBoxStyle.DropDownList;
        UpdatePhaseTargets();
    }

    private void TestSled_Load(object sender, EventArgs e) {
        Verify(testSledDefinitionFI?.FullName, testSledResultsFI?.FullName);
    }
    private void DefBrowse_Click(object sender, EventArgs e) {
        using OpenFileDialog dialog = CreateJsonFileDialog(testSledDefinitionFI);
        dialog.Title = "Select Test Sled definition";
        if (dialog.ShowDialog() != DialogResult.OK) {
            return;
        }
        Verify(dialog.FileName, TestSledResultsPath.Text);
    }
    private void ResultBrowse_Click(object sender, EventArgs e) {
        using OpenFileDialog dialog = CreateJsonFileDialog(testSledResultsFI);
        dialog.Title = "Select Test Sled results";
        if (dialog.ShowDialog() != DialogResult.OK) {
            return;
        }
        Verify(TestSledDefinitionPath.Text, dialog.FileName);
    }
    private void RunTests_Click(object sender, EventArgs e) {
        if (!Verify() || targetPhase < 1) {
            return;
        }

        // Results are evidence for exactly one signed definition and build.
        // A changed definition must not execute stale TestCase objects.
        bool resetResults = state.Value.HasFlag(TestSledState.DefinitionMismatch) ||
            state.Value.HasFlag(TestSledState.VersionMismatch);
        TestArray results = !resetResults && testArrayResults is not null
            ? testArrayResults.Clone()
            : testArrayDefinition.Clone(withoutResults: true);
        Guid batch = Guid.NewGuid();

        foreach (Test test in results.Tests) {
            if (test.Phase > targetPhase || !test.Config.ContextMatch() || test.Pass) {
                continue;
            }

            string[] targets = test.Config.Target switch {
                TestTarget.Invalid => throw new NotImplementedException(),
                TestTarget.Local => [results.LocalTarget],
                TestTarget.SingleRemoteDev => [results.RemoteDevTarget.First()],
                TestTarget.SingleRemoteProd => [results.RemoteProdTarget.First()],
                TestTarget.FullRemoteDev => [.. results.RemoteDevTarget],
                TestTarget.FullRemoteProd => [.. results.RemoteProdTarget],
            };
            test.RunTargets = [.. targets.Distinct(StringComparer.OrdinalIgnoreCase)];
            bool passed = true;

            foreach (TestCase testCase in test.TestSuite) {
                foreach (string target in test.RunTargets) {
                    Logger logger = new();
                    List<RemediationAction> actions = [];
                    void Logger_LogWritten(object? sender, LogEventArgs e) => actions.Add(e.RemediationAction);
                    logger.LogWritten += Logger_LogWritten;
                    TestResult result = new() {
                        RollbackPass = test.Config.AttemptRollback ? true : null
                    };
                    try {
                        TestCase cloned = testCase.Clone(withoutResults: true);
                        cloned.Setting.Order = 0;
                        string ruleId = $"T{test.TestId}P{test.Phase}O{test.Order}C{testCase.Order}";
                        string description = $"Test {test.TestId}, Phase {test.Phase}, Order {test.Order}, TestCase {testCase.Order}";
                        RemediationActionResult action = Rem.ExecuteTestCase(
                            cloned, batch, target, ruleId, description, test.Config.WhatIf, logger);
                        result.RemediationAction = action.HasValue ? action.Value : null;
                        result.ActualAfter = Rem.GetActualState(testCase.Setting.Data, target);
                    } catch (Exception ex) {
                        // Keep the failure in the persisted result, even if state
                        // observation throws (e.g. an unsupported target type).
                        result.Error = $"{ex.GetType().Name}: {ex.Message}";
                    } finally {
                        logger.LogWritten -= Logger_LogWritten;
                        if (test.Config.AttemptRollback) {
                            foreach (RemediationAction action in actions.OrderByDescending(a => a.ActionNumber)) {
                                // Only successful changes can have been applied.
                                if (action.RollbackCapability != RollbackCapability.Automatic ||
                                        action.Result != ActionResult.ActionSuccess) {
                                    continue;
                                }
                                try {
                                    RemediationActionResult rollback = Roll.Execute(batch, action);
                                    if (!rollback.IsSuccess || !rollback.HasValue ||
                                            rollback.Value.Result == ActionResult.ActionFailure) {
                                        result.RollbackPass = false;
                                        result.RollbackError = $"Rollback failed: {action.TargetType}, action {action.ActionNumber}.";
                                    }
                                } catch (Exception ex) {
                                    result.RollbackPass = false;
                                    result.RollbackError = $"{ex.GetType().Name}: {ex.Message}";
                                }
                            }
                        }
                        testCase.Results[target] = result;
                    }
                    passed &= testCase.Pass == true;
                }
            }

            // Recovery is a separate, non-WhatIf operation. Each case is
            // independently guarded so one failure cannot skip later cleanup.
            Logger recoveryLogger = new();
            foreach (TestCase testCase in test.RecoverySuite) {
                foreach (string target in test.RunTargets) {
                    TestResult result = new();
                    try {
                        TestCase cloned = testCase.Clone(withoutResults: true);
                        cloned.Setting.Order = 0;
                        string ruleId = $"T{test.TestId}P{test.Phase}O{test.Order}R{testCase.Order}";
                        string description = $"Test {test.TestId}, Phase {test.Phase}, Order {test.Order}, RecoveryCase {testCase.Order}";
                        RemediationActionResult action = Rem.ExecuteTestCase(
                            cloned, batch, target, ruleId, description, false, recoveryLogger);
                        result.RemediationAction = action.HasValue ? action.Value : null;
                        result.ActualAfter = Rem.GetActualState(testCase.Setting.Data, target);
                    } catch (Exception ex) {
                        result.Error = $"{ex.GetType().Name}: {ex.Message}";
                    }
                    testCase.Results[target] = result;
                    passed &= testCase.Pass == true;
                }
            }
            test.TestRun = true;
            passed &= test.Pass;
            if (!passed) {
                break;
            }
        }

        testArrayResults = results.Clone();
        string json = JsonSerializer.Serialize(testArrayResults, GlobalConstants.JsonSerializerOptions);
        File.WriteAllText(TestSledResultsPath.Text, json);
        RefreshResults();
    }
    private void PhaseTarget_SelectedIndexChanged(object sender, EventArgs e) {
        if (int.TryParse(PhaseTarget.SelectedItem?.ToString(), out int target)) {
            targetPhase = target;
        } else {
            targetPhase = 0;
        }
    }
    private void RefreshResults() {
        if (testArrayResults is null) {
            return;
        }
        ResultsTable.Clear();
        if (testArrayResults?.Tests is not null) {
            foreach (Test test in testArrayResults.Tests) {
                if (test is null) {
                    continue;
                }
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

    [MemberNotNullWhen(true, nameof(testArrayDefinition))]
    private bool Verify() {
        Verify(TestSledDefinitionPath.Text, TestSledResultsPath.Text);
        return state.Value switch {
            var x when x.HasFlag(TestSledState.NoDefinition) => false,
            var x when x.HasFlag(TestSledState.NoCatalog) => false,
            var x when x.HasFlag(TestSledState.Unsigned) => false,
            _ => true
        };
    }
    private void Verify(string? defPath, string? resPath) {
        Status.Text = Messages.None;
        TestSledState pendingState = TestSledState.Valid;
        if (string.IsNullOrWhiteSpace(defPath) ||
                !File.Exists(defPath) ||
                TestArray.LoadFromJson(defPath) is not TestArray def) {
            pendingState |= TestSledState.NoDefinition;
        } else {
            testSledDefinitionFI = new(defPath);
            testArrayDefinition = def;
            TestSledDefinitionPath.Text = defPath;
            Settings.Default.TestSledDefinitionPath = defPath;
            Settings.Default.Save();
            string catPath = Path.Combine(testSledDefinitionFI.DirectoryName!, "TestSled.cat");
            if (!File.Exists(catPath)) {
                pendingState |= TestSledState.NoCatalog | TestSledState.Unsigned;
            } else if (!CatalogApi.VerifyCatalogFile(catPath, testSledDefinitionFI.FullName)) {
                pendingState |= TestSledState.Unsigned;
            }
        }
        if (string.IsNullOrWhiteSpace(resPath) ||
                !File.Exists(resPath) ||
                TestArray.LoadFromJson(resPath) is not TestArray res) {
            pendingState |= TestSledState.NoResults;
        } else if (testSledDefinitionFI is not null && testArrayDefinition is not null) {
            testSledResultsFI = new FileInfo(resPath);
            testArrayResults = res;
            TestSledResultsPath.Text = resPath;
            Settings.Default.TestSledResultsPath = resPath;
            Settings.Default.Save();
            RefreshResults();
            if (!res.BuildsMatchCurrent) {
                pendingState |= TestSledState.VersionMismatch;
            }
            if (!testArrayDefinition.DefinitionEquals(res)) {
                pendingState |= TestSledState.DefinitionMismatch;
            }
            if (!res.Success) {
                pendingState |= TestSledState.ResultsFail;
            }
        }
        state.Value = pendingState;
        UpdatePhaseTargets();
    }

    private void UpdatePhaseTargets() {
        int currentMax = testArrayDefinition?.Tests.Count > 0
            ? testArrayDefinition.Tests.Max(t => t.Phase)
            : 0;
        if (currentMax == maxPhase && PhaseTarget.Items.Count == currentMax) {
            return;
        }
        int selected = targetPhase;
        maxPhase = currentMax;
        PhaseTarget.Items.Clear();
        for (int i = 1; i <= maxPhase; i++) {
            PhaseTarget.Items.Add(i);
        }
        PhaseTarget.Enabled = maxPhase > 0;
        // Default to phase one: increasing scope is an explicit user action.
        PhaseTarget.SelectedItem = maxPhase == 0 ? null : Math.Clamp(selected, 1, maxPhase);
    }
    private static OpenFileDialog CreateJsonFileDialog(FileInfo? previous) => CreateJsonFileDialog(previous?.DirectoryName);
    private static OpenFileDialog CreateJsonFileDialog(string? previous) {
        string startPath = previous ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Task timeout = Task.Delay(500);
        static async Task FastCheck(string server) {
            using CancellationTokenSource cts = new(100);
            _ = await System.Net.Dns.GetHostEntryAsync(server, cts.Token);
            System.Net.NetworkInformation.Ping p = new();
            cts.TryReset();
            if ((await p.SendPingAsync(server, TimeSpan.FromMilliseconds(100), new byte[1], null, cts.Token)).Status !=
                    System.Net.NetworkInformation.IPStatus.Success) {
                throw new Exception();
            }
        }
        if (Path.GetPathRoot(startPath) is string root) {
            if (root.StartsWith("\\\\")) {
                root = root[2..root.IndexOf('\\', 2)];
                try {
                    Task[] tasks = [timeout, FastCheck(root)];
                    if (Task.WaitAny(timeout, FastCheck(root)) == 0 || !tasks[1].IsCompletedSuccessfully) {
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
}
