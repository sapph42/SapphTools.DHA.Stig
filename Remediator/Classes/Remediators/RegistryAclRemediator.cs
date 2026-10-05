namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class RegistryAclRemediator : IRemediator {
    private RegistryAclRemediator() { }

    public static RemediationActionResult Remediate(Rule rule, int settingIndex, Guid remBatch, Guid ruleBatch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            return new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not RegistryAclValue val) {
            return new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type RegistryAclValue, was {data.GetType().Name}");
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
        return SetAcl(pre, val, whatIf);
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) {
        IValue? original = logEntry.Before;
        if (original is null) {
            return new ArgumentException("Cannot rollback an entry with null Before property");
        }
        if (original is not RegistryAclValue val) {
            return new ArgumentException($"Expected {nameof(logEntry)}.Before to be of type RegistryAclValue, was {original.GetType().Name}");
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
        return SetAcl(pre, val, whatIf: false);
    }
    private static RemediationActionResult SetAcl(RemediationPreAction preAction, RegistryAclValue value, bool whatIf = true) {
        RegKey target;
        try {
            target = new(value.Target, preAction.ComputerName);
        } catch (RegKeyException rkEx) {
            return Logger.LogError(preAction, TargetType.RegistryKey, value.Target, rkEx.ReasonToString(), whatIf);
        }
        try {
            RegistryAclValue before = target.GetAclValue();
            if (whatIf) {
                return Logger.LogWhatIf(
                    preAction,
                    TargetType.RegistryAcl,
                    value.Target,
                    RollbackCapability.NotApplicable,
                    before,
                    value
                );
            }
            RegistryAclValue after;
            using (target) {
                target.SetSddl(value.Sddl);
                after = target.GetAclValue();
            }
            return Logger.LogSuccess(
                preAction,
                TargetType.RegistryAcl,
                value.Target,
                RollbackCapability.Automatic,
                before,
                after
            );
        } catch (Exception ex) {
            return Logger.LogError(preAction, TargetType.RegistryAcl, value.Target, $"Exception thrown during ACL operation: {ex.Message}", whatIf);
        }
    }
}
