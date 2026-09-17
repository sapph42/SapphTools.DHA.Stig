using Microsoft.Win32;
using System.Security.AccessControl;
using static SapphTools.DHA.Stig.Remediator.Classes.RegistryCommon;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class RegistryAclRemediator : IRemediator {
    private RegistryAclRemediator() { }

    public static void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            throw new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not RegistryAclValue val) {
            throw new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type RegistryAclValue, was {data.GetType().Name}");
        }
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = batch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex
        };
        SetAcl(pre, val, whatIf);
    }
    private static void SetAcl(RemediationPreAction preAction, RegistryAclValue value, bool whatIf = true) {
        if (!BasicChecks(preAction, value.Target, out RegistryKey? targetKey, whatIf)) {
            return;
        }
        try {
            RegistryAclValue before = new() {
                Target = value.Target,
                Sddl = new(
                    targetKey
                        .GetAccessControl()
                        .GetSecurityDescriptorSddlForm(AccessControlSections.All), 
                    SecurityDescriptor.Enums.ObjectType.RegistryKey)
            };
            if (whatIf) {
                Logger.LogWhatIf(
                    preAction,
                    TargetType.RegistryAcl,
                    value.Target,
                    RollbackCapability.NotApplicable,
                    before,
                    value
                );
                return;
            }
            using (targetKey) {
                targetKey.SetAccessControl(value.Sddl.ToRegistrySecurity());
            }
            Logger.LogSuccess(
                preAction,
                TargetType.RegistryAcl,
                value.Target,
                RollbackCapability.Automatic,
                before,
                value
            );
        } catch (Exception ex) {
            Logger.LogError(preAction, TargetType.RegistryAcl, value.Target, $"Exception thrown during ACL operation: {ex.Message}", whatIf);
        }
    }
}
