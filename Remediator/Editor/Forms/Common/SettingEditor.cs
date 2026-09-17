using System.Diagnostics.CodeAnalysis;


namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common;
public partial class SettingEditor : Form {
    public required string RuleId { get; set; }
    public Setting? Setting { get; private set; }
    public SettingEditor() {
        InitializeComponent();
        foreach (TargetType type in Enum.GetValues(typeof(TargetType))) {
            ActionType.Items.Add(type);
        }
        foreach (SettingContext type in Enum.GetValues(typeof(SettingContext))) {
            RequiredContext.Items.Add(type);
        }
        ActionType.Format += (_, e) => {
            if (e.ListItem is TargetType type) {
                e.Value = type.GetUiDisplay();
            }
        };
        RequiredContext.Format += (_, e) => {
            if (e.ListItem is SettingContext type) {
                e.Value = type.GetUiDisplay();
            }
        };
        RequiredContext.SelectedItem = SettingContext.Administrator;
    }
    [SetsRequiredMembers]
    public SettingEditor(Setting setting, string ruleId) : this() {
        Setting = setting;
        ActionType.SelectedItem = setting.Data.Action;
        RequiredContext.SelectedItem = setting.RequiredContext;
        SettingsContainer.Controls.Remove(ValueSetting);
        ValueSetting = setting.Data.Action switch {
            TargetType.FileSystemAcl => new FileSystemAclSettings(Setting),
            TargetType.RegistryAcl => new RegistryAclSettings(Setting),
            TargetType.RegistryValue => new RegistryValueSettings(Setting),
            TargetType.RegistryValuePattern => new RegistryValuePatternSettings(Setting),
            TargetType.SeRight => new SeRightsSettings(Setting),
            TargetType.CertStore => new CertificatesValueSettings(Setting, ruleId),
            _ => new ValueSettings()
        };
        Dangerous.Checked = setting.Dangerous;
        ValueSetting.Location = new Point(6, 22);
        SettingsContainer.Controls.Add(ValueSetting);
        ResumeLayout();
        RuleId = ruleId;
    }

    private void ActionType_SelectedIndexChanged(object sender, EventArgs e) {
        SuspendLayout();
        SettingsContainer.Controls.Remove(ValueSetting);
        if (ActionType.SelectedItem is not TargetType type) {
            ValueSetting = new ValueSettings();
        } else {
            ValueSetting = type switch {
                TargetType.FileSystemAcl => new FileSystemAclSettings(Setting),
                TargetType.RegistryAcl => new RegistryAclSettings(Setting),
                TargetType.RegistryValue => new RegistryValueSettings(Setting),
                TargetType.RegistryValuePattern => new RegistryValuePatternSettings(Setting),
                TargetType.SeRight => new SeRightsSettings(Setting),
                TargetType.CertStore => new CertificatesValueSettings(Setting, RuleId),
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
        if (ValueSetting.Value is null) {
            return;
        }
        if (Setting is not null) {
            Setting.RequiredContext = (SettingContext)RequiredContext.SelectedItem!;
            Setting.Dangerous = Dangerous.Checked;
            Setting.Data = ValueSetting.Value;
        } else {
            Setting = new() {
                Order = -1,
                RequiredContext = (SettingContext)RequiredContext.SelectedItem!,
                Dangerous = Dangerous.Checked,
                Data = ValueSetting.Value,
            };
        }
        Close();
    }

    private void Cancel_Click(object sender, EventArgs e) {
        Close();
    }
}
