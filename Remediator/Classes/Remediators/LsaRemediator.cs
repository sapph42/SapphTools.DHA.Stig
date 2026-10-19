using SapphTools.SecurityDescriptor.Classes;
using System.ComponentModel;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class LsaRemediator : IRemediator {
    private LsaRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not SeRightsValue val) {
            throw new ArgumentException($"Expected {target} to be of type SeRightsValue, was {target.GetType().Name}");
        }
        LsaConnection? conn = null;
        try {
            try {
                conn = new(computerName, val);
                return conn.GetBeforeValue();
            } catch {
                return null;
            }
        } finally {
            conn?.Dispose();
        }
    }
    public static RemediationActionResult Remediate(RemediatorDto dto, Logger? logger = null) {
        IValue data = dto.Setting.Data.Clone();
        if (data is not SeRightsValue val) {
            return new ArgumentException($"Expected {nameof(dto)}.Setting.Data to be of type SeRightsValue, was {data.GetType().Name}");
        }
        logger ??= new();
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        return SetRights(pre, val, logger, dto.WhatIf);
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger = null) {
        if (logEntry.Before is null) {
            return new ArgumentException("Cannot rollback a log entry with a null Before property.", nameof(logEntry));
        }
        IValue data = logEntry.Before;
        if (data is not SeRightsValue val) {
            return new ArgumentException($"Expected {nameof(logEntry)}.Before to be of type SeRightsValue, was {data.GetType().Name}");
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
        return SetRights(pre, val, logger, whatIf: false);
    }
    private static RemediationActionResult SetRights(RemediationPreAction preAction, SeRightsValue value, Logger logger, bool whatIf = true) {
        LsaConnection? conn = null;
        try {
            try {
                conn = new(preAction.ComputerName, value);
            } catch (ArgumentOutOfRangeException privEx) {
                return logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    value.Target,
                    $"Exception instantiating LsaConnection: {privEx.Message}",
                    whatIf);
            } catch (ArgumentException namesEx) {
                return logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Argument Exception instantiating LsaConnection: {namesEx.Message}",
                    whatIf);
            } catch (Win32Exception winEx) {
                return logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Win32 Exception instantiating LsaConnection: {winEx.Message}",
                    whatIf);
            } catch (LsaException lsaEx) {
                return logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Lsa Exception instantiating LsaConnection ({lsaEx.Reason}): {lsaEx.Message}",
                    whatIf);
            } catch (Exception ex) {
                return logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Exception instantiating LsaConnection: {ex.Message}",
                    whatIf);
            }
            try {
                SeRightsValue before = conn.GetBeforeValue();
                if (conn.RightMatchesList()) {
                    return logger.LogNoAction(
                        preAction,
                        TargetType.SeRight,
                        value.Target,
                        before
                    );
                }
                Trustee[] accounts = new Trustee[value.AccountNames.Length];
                for (int i = 0; i < accounts.Length; i++) {
                    accounts[i] = value.AccountNames[i].Clone();
                }
                if (whatIf) {
                    return logger.LogWhatIf(
                        preAction,
                        TargetType.SeRight,
                        value.Target,
                        RollbackCapability.NotApplicable,
                        before,
                        new SeRightsValue() {
                            Target = before.Target,
                            AccountNames = accounts
                        }
                    );
                }
                try {
                    conn.SetAccountRights();
                    return logger.LogSuccess(
                        preAction,
                        TargetType.SeRight,
                        value.Target,
                        RollbackCapability.Automatic,
                        before,
                        new SeRightsValue() {
                            Target = before.Target,
                            AccountNames = accounts
                        }
                    );
                } catch (LsaException lex) when (lex.Reason == LsaExceptionReason.ObjectNameNotFound) {
                    return logger.LogNoAction(preAction, TargetType.SeRight, value.Target, before);
                } catch (Exception ex) {
                    return logger.LogError(
                        preAction,
                        TargetType.SeRight,
                        value.Target,
                        ex.Message,
                        whatIf: false);
                }
            } catch (Exception ex) {
                return logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    value.Target,
                    ex.Message,
                    whatIf);
            }
        } finally {
            conn?.Dispose();
        }
    }
}
