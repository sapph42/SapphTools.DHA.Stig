using SapphTools.SecurityDescriptor.Classes;
using System.ComponentModel;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class LsaRemediator : IRemediator {
    private LsaRemediator() { }
    public static void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            throw new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not SeRightsValue val) {
            throw new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type SeRightsValue, was {data.GetType().Name}");
        }
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = batch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex
        };
        SetRights(pre, val, whatIf);
    }
    private static void SetRights(RemediationPreAction preAction, SeRightsValue value, bool whatIf = true) {
        LsaConnection? conn = null;
        try {
            try {
                conn = new(preAction.ComputerName, value);
            } catch (ArgumentOutOfRangeException privEx) {
                Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    value.Target,
                    $"Exception instantiating LsaConnection: {privEx.Message}",
                    whatIf);
                return;
            } catch (ArgumentException namesEx) {
                Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Argument Exception instantiating LsaConnection: {namesEx.Message}",
                    whatIf);
                return;
            } catch (Win32Exception winEx) {
                Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Win32 Exception instantiating LsaConnection: {winEx.Message}",
                    whatIf);
                return;
            } catch (LsaException lsaEx) {
                Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Lsa Exception instantiating LsaConnection ({lsaEx.Reason}): {lsaEx.Message}",
                    whatIf);
                return;
            } catch (Exception ex) {
                Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    string.Join(", ", value.AccountNames.Select(t => t.Sid)),
                    $"Exception instantiating LsaConnection: {ex.Message}",
                    whatIf);
                return;
            }
            try {
                SeRightsValue before = conn.GetBeforeValue();
                if (conn.RightMatchesList()) {
                    Logger.LogNoAction(
                        preAction,
                        TargetType.SeRight,
                        value.Target,
                        before
                    );
                    return;
                }
                Trustee[] accounts = new Trustee[value.AccountNames.Length];
                for (int i = 0; i < accounts.Length; i++) {
                    accounts[i] = value.AccountNames[i].Clone();
                }
                if (whatIf) {
                    Logger.LogWhatIf(
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
                    return;
                }
                try {
                    conn.SetAccountRights();
                    Logger.LogSuccess(
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
                    Logger.LogNoAction(preAction, TargetType.SeRight, value.Target, before);
                } catch (Exception ex) {
                    Logger.LogError(
                        preAction,
                        TargetType.SeRight,
                        value.Target,
                        ex.Message,
                        whatIf: false);
                    return;
                }
            } catch (Exception ex) {
                Logger.LogError(
                    preAction,
                    TargetType.SeRight,
                    value.Target,
                    ex.Message,
                    whatIf);
                return;
            }
        } finally {
            conn?.Dispose();
        }
    }
}
