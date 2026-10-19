using System.Data;

namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms;
public partial class TestCaseEditor : Form {
    private readonly TestCase? OriginalTestCase;
    private readonly DataTable SettingTable;
    private readonly BindingSource settingSource;
    private TestCase? ActiveTestCase;
    private Setting? ActiveSetting;
    private IValue? ActiveExpectedAfter;
    private TargetType? ActiveEaType => ActiveSetting?.Data.Action;
    private ActionResult? ActiveExpectedResult;
    public required string SimulatedRuleId { get; set; }
    public TestCase? TestCase;
    public TestCaseEditor() {
        InitializeComponent();
        SettingView.AutoGenerateColumns = false;
        SettingView.Columns.AddRange([
            new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "TargetType",
                HeaderText = "Target Type",
                Name = "TargetType",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            },new DataGridViewTextBoxColumn() {
                CellTemplate = new DataGridViewTextBoxCell(),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DataPropertyName = "Target",
                HeaderText = "Target",
                Name = "Target",
                ReadOnly = true,
                ValueType = typeof(string),
                Visible = true,
            }
        ]);
        SettingView.CellFormatting += (s, e) => {
            if (SettingView.Columns[e.ColumnIndex].Name == "Target" && e.Value is IValue val) {
                e.Value = val.TargetString;
                e.FormattingApplied = true;
            }
        };
        SettingTable = new();
        SettingTable.Columns.AddRange([
            new("TargetType", typeof(TargetType)),
            new("Target", typeof(IValue))
        ]);
        settingSource = new() {
            DataSource = SettingTable
        };
        ExpectedResult.DropDownStyle = ComboBoxStyle.DropDownList;
        ExpectedResult.DataSource = Enum.GetValues<ActionResult>()
            .ToArray();
        ExpectedResult.SelectedItem = ActionResult.ActionSuccess;
        SettingView.DataSource = settingSource;
    }
    public TestCaseEditor(TestCase testCase) : this() {
        OriginalTestCase = testCase;
        ActiveTestCase = testCase.Clone();
        ActiveSetting = ActiveTestCase.Setting;
        ActiveExpectedAfter = ActiveTestCase.ExpectedAfter;
        ActiveExpectedResult = ActiveTestCase.ExpectedResult;
        ExpectedResult.SelectedItem = ActiveExpectedResult;
        UpdateSettingTable();
        UpdateSettingsControl();
    }
    private void EditSetting_Click(object sender, EventArgs e) {
        SettingEditor settingEditor;
        if (ActiveSetting is not null) {
            settingEditor = new(ActiveSetting.Clone(), SimulatedRuleId);
        } else {
            settingEditor = new() {
                RuleId = SimulatedRuleId
            };
        }
        if (settingEditor.ShowDialog() == DialogResult.OK && settingEditor.Setting is Setting setting) {
            ActiveSetting = setting.Clone();
            ActiveSetting.Order = 0;
            UpdateSettingTable();
            if (!ActiveSetting.Data.GetType().Equals(ActiveExpectedAfter?.GetType())) {
                ActiveExpectedAfter = null;
                UpdateSettingsControl();
            }
        }
    }

    private void UpdateSettingTable() {
        SettingTable.Rows.Clear();
        if (ActiveSetting is null) {
            return;
        }
        SettingTable.Rows.Add(ActiveSetting.Data.Action, ActiveSetting.Data);
    }
    private void UpdateSettingsControl() {
        SuspendLayout();
        SettingsContainer.Controls.Remove(ValueSetting);
        if (ActiveSetting is null) {
            ValueSetting = new ValueSettings();
        } else {
            ValueSetting = ActiveEaType switch {
                TargetType.FileSystemAcl => new FileSystemAclSettings(ActiveExpectedAfter as FileSystemAclValue),
                TargetType.RegistryAcl => new RegistryAclSettings(ActiveExpectedAfter as RegistryAclValue),
                TargetType.RegistryValue => new RegistryValueSettings(ActiveExpectedAfter as RegistryValueValue),
                TargetType.RegistryValuePattern => new RegistryValuePatternSettings(ActiveExpectedAfter as RegistryValuePatternValue),
                TargetType.SeRight => new SeRightsSettings(ActiveExpectedAfter as SeRightsValue),
                TargetType.CertStore => new CertificatesValueSettings(
                    ActiveExpectedAfter as CertificatesValue,
                    SimulatedRuleId
                ),
                _ => new ValueSettings()
            };
        }
        ValueSetting.Location = new Point(6, 22);
        SettingsContainer.Controls.Add(ValueSetting);
        ResumeLayout();
    }
    private void Ok_Click(object sender, EventArgs e) {
        if (!ValueSetting.IsValid) {
            return;
        }
        if (ValueSetting.Value is null || ActiveSetting is null || ActiveExpectedResult is null) {
            return;
        }
        ActiveExpectedAfter = ValueSetting.Value;
        TestCase = new() {
            Order = OriginalTestCase?.Order ?? -1,
            Setting = ActiveSetting.Clone(),
            ExpectedAfter = ActiveExpectedAfter.Clone(),
            ExpectedResult = ActiveExpectedResult.Value
        };
        Close();
    }
    private void Cancel_Click(object sender, EventArgs e) {
        TestCase = null;
        Close();
    }
    private void ExpectedResult_SelectedIndexChanged(object sender, EventArgs e) {
        if (ExpectedResult.SelectedItem is ActionResult res) {
            ActiveExpectedResult = res;
        } else {
            ActiveExpectedResult = null;
        }
    }
}
