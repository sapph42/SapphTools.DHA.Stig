using static SapphTools.DHA.Stig.Common.WinApi.SamApi;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
public class SamRemediator : IRemediator {
    private SamRemediator() { }
    public static RemediationActionResult Remediate(Rule rule, int settingIndex, Guid remBatch, Guid ruleBatch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            return new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not SamValue val) {
            return new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type SamValue, was {data.GetType().Name}");
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = remBatch,
            RuleBatch = ruleBatch,
            SettingBatch = settingBatch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex,
            Source = ActionSource.Catalog
        };
        switch (val.Target) {
            case SamActionType.AccountLockoutPolicy:
                LockoutValue lockoutValue = (LockoutValue)val;
                return SetLockout(pre, lockoutValue, whatIf);
            default:
                return new NotImplementedException();
        }
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) {
        if (logEntry.Before is null) {
            return new ArgumentException("Cannot rollback a log entry with a null Before property.");
        }
        IValue original = logEntry.Before;
        if (original is not SamValue val) {
            return new ArgumentException($"Expected {nameof(logEntry)}.Before to be of type SamValue, was {original.GetType().Name}");
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = logEntry.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = remBatch,
            RuleBatch = ruleBatch,
            SettingBatch = settingBatch,
            RuleId = logEntry.RuleId,
            Description = logEntry.Description,
            ComputerName = targetHost,
            SettingIndex = logEntry.SettingIndex,
            Source = ActionSource.Rollback
        };
        switch (val.Target) {
            case SamActionType.AccountLockoutPolicy:
                LockoutValue lockoutValue = (LockoutValue)original;
                return SetLockout(pre, lockoutValue, whatIf: false);
            default:
                return new NotImplementedException();
        }
    }
    private static RemediationActionResult SetLockout(RemediationPreAction preAction, LockoutValue value, bool whatIf) {
        SamUserModalInfo3 current;
        try {
            current = GetSamData(preAction.ComputerName, value);
        } catch {
            return Logger.LogError(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                "Failed to get current account lockout policies.",
                whatIf
            );
        }
        if (value.SamStruct == current) {
            return Logger.LogNoAction(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                new LockoutValue() {
                    SamStruct = current
                }
            );
        }
        if (whatIf) {
            return Logger.LogWhatIf(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                RollbackCapability.NotApplicable,
                new LockoutValue() {
                    SamStruct = current
                },
                value
            );
        }
        try {
            if (SetSamData(preAction.ComputerName, value)) {
                return Logger.LogSuccess(
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
                return Logger.LogError(
                    preAction,
                    TargetType.Sam,
                    value.Target.ToString(),
                    "Failed to set account lockout policies.",
                    whatIf: false
                );
            }
        } catch (Exception ex) {
            return Logger.LogError(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                ex.Message,
                whatIf: false
            );
        } finally {
            preAction.ActionNumber++;
        }
    }
}
