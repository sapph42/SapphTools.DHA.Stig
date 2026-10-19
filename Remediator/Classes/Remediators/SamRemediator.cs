using SapphTools.DHA.Stig.Common.Interfaces;
using static SapphTools.DHA.Stig.Common.WinApi.SamApi;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
public class SamRemediator : IRemediator {
    private SamRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not SamValue val) {
            throw new ArgumentException($"Expected {target} to be of type SamValue, was {target.GetType().Name}");
        }
        SamUserModalInfo3 current;
        try {
            switch (val.Target) {
                case SamActionType.AccountLockoutPolicy:
                    LockoutValue lockoutValue = (LockoutValue)val;
                    current = GetSamData(computerName, lockoutValue);
                    return new LockoutValue() {
                        SamStruct = current
                    };
                default:
                    return null;
            }
        } catch {
            return null;
        }
    }
    public static RemediationActionResult Remediate(RemediatorDto dto, Logger? logger = null) {
        IValue data = dto.Setting.Data.Clone();
        if (data is not SamValue val) {
            return new ArgumentException($"Expected {nameof(dto)}.Setting.Data to be of type SamValue, was {data.GetType().Name}");
        }
        logger ??= new();
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        switch (val.Target) {
            case SamActionType.AccountLockoutPolicy:
                LockoutValue lockoutValue = (LockoutValue)val;
                return SetLockout(pre, lockoutValue, dto.WhatIf, logger);
            default:
                return new NotImplementedException();
        }
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger = null) {
        if (logEntry.Before is null) {
            return new ArgumentException("Cannot rollback a log entry with a null Before property.");
        }
        IValue original = logEntry.Before;
        if (original is not SamValue val) {
            return new ArgumentException($"Expected {nameof(logEntry)}.Before to be of type SamValue, was {original.GetType().Name}");
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
        switch (val.Target) {
            case SamActionType.AccountLockoutPolicy:
                LockoutValue lockoutValue = (LockoutValue)original;
                return SetLockout(pre, lockoutValue, whatIf: false, logger);
            default:
                return new NotImplementedException();
        }
    }
    private static RemediationActionResult SetLockout(RemediationPreAction preAction, LockoutValue value, bool whatIf, Logger logger) {
        SamUserModalInfo3 current;
        try {
            current = GetSamData(preAction.ComputerName, value);
        } catch {
            return logger.LogError(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                "Failed to get current account lockout policies.",
                whatIf
            );
        }
        if (value.SamStruct == current) {
            return logger.LogNoAction(
                preAction,
                TargetType.Sam,
                value.Target.ToString(),
                new LockoutValue() {
                    SamStruct = current
                }
            );
        }
        if (whatIf) {
            return logger.LogWhatIf(
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
                return logger.LogSuccess(
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
                return logger.LogError(
                    preAction,
                    TargetType.Sam,
                    value.Target.ToString(),
                    "Failed to set account lockout policies.",
                    whatIf: false
                );
            }
        } catch (Exception ex) {
            return logger.LogError(
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
