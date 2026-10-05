using SapphTools.SecurityDescriptor.Classes;
using System.ComponentModel;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class LsaRemediator : IRemediator {
    private LsaRemediator() { }
    public static RemediationActionResult Remediate(Rule rule, int settingIndex, Guid remBatch, Guid ruleBatch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            return new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not SeRightsValue val) {
            return new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type SeRightsValue, was {data.GetType().Name}");
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
        return SetRights(pre, val, whatIf);
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) {
        if (logEntry.Before is null) {
            return new ArgumentException("Cannot rollback a log entry with a null Before property.", nameof(logEntry));
        }
        IValue data = logEntry.Before;
        if (data is not SeRightsValue val) {
            return new ArgumentException($"Expected {nameof(logEntry)}.Before to be of type SeRightsValue, was {data.GetType().Name}");
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
        return SetRights(pre, val, whatIf: false);
    }
    private static RemediationActionResult SetRights(RemediationPreAction preAction, SeRightsValue value, bool whatIf = true) {
        LsaConnection? conn = null;
        try {
            try {
                conn = new(preAction.ComputerName, value);
            } catch (ArgumentOutOfRangeException privEx) {
                return Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    value.Target,
                    $"Exception instantiating LsaConnection: {privEx.Message}",
                    whatIf);
            } catch (ArgumentException namesEx) {
                return Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Argument Exception instantiating LsaConnection: {namesEx.Message}",
                    whatIf);
            } catch (Win32Exception winEx) {
                return Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Win32 Exception instantiating LsaConnection: {winEx.Message}",
                    whatIf);
            } catch (LsaException lsaEx) {
                return Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Lsa Exception instantiating LsaConnection ({lsaEx.Reason}): {lsaEx.Message}",
                    whatIf);
            } catch (Exception ex) {
                return Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Exception instantiating LsaConnection: {ex.Message}",
                    whatIf);
            }
            try {
                SeRightsValue before = conn.GetBeforeValue();
                if (conn.RightMatchesList()) {
                    return Logger.LogNoAction(
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
                    return Logger.LogWhatIf(
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
                    return Logger.LogSuccess(
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
                    return Logger.LogNoAction(preAction, TargetType.SeRight, value.Target, before);
                } catch (Exception ex) {
                    return Logger.LogError(
                        preAction,
                        TargetType.SeRight,
                        value.Target,
                        ex.Message,
                        whatIf: false);
                }
            } catch (Exception ex) {
                return Logger.LogError(
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
