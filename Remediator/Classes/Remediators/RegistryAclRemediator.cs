namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class RegistryAclRemediator : IRemediator {
    private RegistryAclRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not RegistryAclValue val) {
            throw new ArgumentException($"Expected {target} to be of type RegistryAclValue, was {target.GetType().Name}");
        }
        RegKey? targetKey = null;
        try {
            try {
                targetKey = new(val.Target, computerName);
                if (targetKey is null) {
                    return null;
                }
            } catch {
                return null;
            }
            try {
                return targetKey.GetAclValue();
            } catch {
                return null;
            }
        } catch {
            return null;
        } finally {
            targetKey?.Dispose();
        }
    }
    public static RemediationActionResult Remediate(RemediatorDto dto, Logger? logger = null) {
        IValue data = dto.Setting.Data.Clone();
        if (data is not RegistryAclValue val) {
            return new ArgumentException($"Expected {nameof(dto)}.Setting.Data to be of type RegistryAclValue, was {data.GetType().Name}");
        }
        logger ??= new();
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        return SetAcl(pre, val, logger, dto.WhatIf);
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger = null) {
        IValue? original = logEntry.Before;
        if (original is null) {
            return new ArgumentException("Cannot rollback an entry with null Before property");
        }
        if (original is not RegistryAclValue val) {
            return new ArgumentException($"Expected {nameof(logEntry)}.Before to be of type RegistryAclValue, was {original.GetType().Name}");
        }
        logger ??= new();
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
        return SetAcl(pre, val, logger, whatIf: false);
    }
    private static RemediationActionResult SetAcl(RemediationPreAction preAction, RegistryAclValue value, Logger logger, bool whatIf = true) {
        RegKey target;
        try {
            target = new(value.Target, preAction.ComputerName);
        } catch (RegKeyException rkEx) {
            return logger.LogError(preAction, TargetType.RegistryKey, value.Target, rkEx.ReasonToString(), whatIf);
        }
        try {
            RegistryAclValue before = target.GetAclValue();
            if (whatIf) {
                return logger.LogWhatIf(
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
            return logger.LogSuccess(
                preAction,
                TargetType.RegistryAcl,
                value.Target,
                RollbackCapability.Automatic,
                before,
                after
            );
        } catch (Exception ex) {
            return logger.LogError(preAction, TargetType.RegistryAcl, value.Target, $"Exception thrown during ACL operation: {ex.Message}", whatIf);
        }
    }
}
