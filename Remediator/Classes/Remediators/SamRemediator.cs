using static SapphTools.DHA.Stig.Common.WinApi.SamApi;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
public class SamRemediator : IRemediator {
    private SamRemediator() { }
    public static void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            throw new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not SamValue val) {
            throw new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type SamValue, was {data.GetType().Name}");
        }
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = batch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex
        };
        switch (val.Target) {
            case SamActionType.AccountLockoutPolicy:
                LockoutValue lockoutValue = (LockoutValue)val;
                SetLockout(pre, lockoutValue, whatIf);
                break;
        }
    }
    private static void SetLockout(RemediationPreAction preAction, LockoutValue value, bool whatIf) {
        SamUserModalInfo3 current;
        try {
            current = GetSamData(preAction.ComputerName, value);
        } catch {
            Logger.LogError(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                "Failed to get current account lockout policies.",
                whatIf
            );
            return;
        }
        if (value.SamStruct == current) {
            Logger.LogNoAction(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                new LockoutValue() {
                    SamStruct = current
                }
            );
            return;
        }
        if (whatIf) {
            Logger.LogWhatIf(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                RollbackCapability.NotApplicable,
                new LockoutValue() {
                    SamStruct = current
                },
                value
            );
            return;
        }
        try {
            if (SetSamData(preAction.ComputerName, value)) {
                Logger.LogSuccess(
                    preAction,
                    TargetType.Sam,
                    value.Target.ToString(),
                    RollbackCapability.Automatic,
                    new LockoutValue() {
                        SamStruct = current
                    },
                    value
                );
            } else {
                Logger.LogError(
                    preAction,
                    TargetType.Sam,
                    value.Target.ToString(),
                    "Failed to set account lockout policies.",
                    whatIf: false
                );
            }
        } catch (Exception ex) {
            Logger.LogError(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                ex.Message,
                whatIf: false
            );
        }
    }
}
