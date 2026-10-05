using Microsoft.Win32;
using SapphTools.DHA.Stig.Common.Interfaces;
using System.Diagnostics;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal partial class RegistryValueRemediator : IRemediator {
    private RegistryValueRemediator() { }
    public static RemediationActionResult Remediate(Rule rule, int settingIndex, Guid remBatch, Guid ruleBatch, string? computerName, bool whatIf = true) {
        Debug.WriteLine($"Starting remediation of {rule.RuleId} on {computerName}");
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            return new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not RegistryValueValue val) {
            return new ArgumentException(
                $"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type RegistryValueValue, was {data.GetType().Name}"
            );
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
        RemediationActionResult createRes = RegistryKeyRemediator.SubRemediate(val.Target, targetHost, pre, out RegKey? key, whatIf);
        if (createRes.IsSuccess && key is not null) {
            try {
                RegistryValueValue before = new() {
                    Target = val.Target,
                    Name = val.Name,
                    Data = key.GetValue(val.Name, val.Kind),
                    Kind = key.GetValueKind(val.Name)
                };
                return SubRemediate(pre, before, key, val, whatIf);
            } catch (Exception ex) {
                return Logger.LogError(pre, TargetType.RegistryValue, val.Target, ex.Message, false);
            }
        }
        return createRes;
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) {
        Debug.WriteLine($"Starting rollback of {logEntry.RuleId} on {logEntry.ComputerName}");
        if (logEntry.After is null) {
            return new ArgumentException("Cannot rollback a log entry with a null After property.");
        }
        IValue? original = logEntry.Before;
        IValue current = logEntry.After;
        if (original is not RegistryValueValue oldVal) {
            return new ArgumentException(
                $"Expected {nameof(logEntry)}.Before to be of type RegistryValueValue, was {original?.GetType().Name}"
            );
        }
        if (current is not RegistryValueValue newVal) {
            return new ArgumentException(
                $"Expected {nameof(logEntry)}.After to be of type RegistryValueValue, was {current.GetType().Name}"
            );
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
            Source = ActionSource.Rollback,
        };
        RegKey? key = new(newVal.Target, targetHost);
        if (key is null) {
            try {
                if (oldVal.Data is null) {
                    return Logger.LogNoAction(
                        pre,
                        TargetType.RegistryValue,
                        newVal.Target,
                        oldVal
                    );
                } else {
                    return Logger.LogError(pre, TargetType.RegistryValue, oldVal.Target, "Parent key no longer exists", false);
                }
            } finally {
                pre.ActionNumber++;
            }
        } else {
            try {
                RegistryValueValue before = new() {
                    Target = newVal.Target,
                    Name = newVal.Name,
                    Data = key.GetValue(newVal.Name, newVal.Kind),
                    Kind = newVal.Kind
                };
                return SubRemediate(pre, before, key, oldVal, false);
            } catch (Exception ex) {
                return Logger.LogError(pre, TargetType.RegistryValue, oldVal.Target, ex.Message, false);
            }
        }
    }
    public static RemediationActionResult SubRemediate(RemediationPreAction preAction, IValue before, RegKey key, RegistryValueValue value, bool whatIf = true) {
        try {
            RegistryValueValue? currentState;
            if (before is RegistryValueValue standard) {
                currentState = standard;
            } else if (before is RegistryValuePatternValue patterned && patterned.ResolvedTarget is RegistryValueValue val) {
                currentState = val;
            } else {
                return Logger.LogError(preAction, TargetType.RegistryValue, value.Target, "Invalid before value provided", whatIf);
            }
            if (value.Data is null) {
                if (currentState.Data is null) {
                    return Logger.LogNoAction(
                        preAction,
                        TargetType.RegistryValue,
                        key.FullName,
                        before
                    );
                }
                if (whatIf) {
                    return Logger.LogWhatIf(preAction, TargetType.RegistryKey, key.FullName, RollbackCapability.NotApplicable, before, null);
                }
                try {
                    key.DeleteValue(value.Name, false);
                    RegistryValueValue after = new() {
                        Target = value.Target,
                        Name = value.Name,
                        Data = key.GetValue(value.Name, RegistryValueKind.None),
                        Kind = value.Kind
                    };
                    return Logger.LogSuccess(preAction, TargetType.RegistryValue, value.Target, RollbackCapability.Automatic, before, after);
                } catch (RegKeyException rkEx) {
                    return Logger.LogError(preAction, TargetType.RegistryValue, value.Target, rkEx.ReasonToString(), whatIf);
                } catch (Exception ex) {
                    return Logger.LogError(preAction, TargetType.RegistryValue, value.Target, ex.Message, whatIf);
                }
            } else {
                if (value.Data.Equals(currentState.Data)) {
                    return Logger.LogNoAction(
                        preAction,
                        TargetType.RegistryValue,
                        key.FullName,
                        before
                    );
                }
                try {
                    if (whatIf) {
                        return Logger.LogWhatIf(preAction, TargetType.RegistryKey, key.FullName, RollbackCapability.NotApplicable, before, null);
                    }
                    if (currentState.Data is not null && !value.Overwrite) {
                        return Logger.LogNoAction(
                            preAction,
                            TargetType.RegistryValue,
                            key.FullName,
                            before
                        );
                    }
                    key.SetValue(value.Name, value.Data, value.Kind);
                    RegistryValueValue after = new() {
                        Target = value.Target,
                        Name = value.Name,
                        Data = key.GetValue(value.Name, value.Kind),
                        Kind = value.Kind
                    };
                    return Logger.LogSuccess(preAction, TargetType.RegistryValue, value.Target, RollbackCapability.Automatic, before, after);
                } catch (RegKeyException rkEx) {
                    return Logger.LogError(preAction, TargetType.RegistryValue, value.Target, rkEx.ReasonToString(), whatIf);
                } catch (Exception ex) {
                    return Logger.LogError(preAction, TargetType.RegistryValue, value.Target, ex.Message, whatIf);
                }
            }
        } catch (RegKeyException rkEx) {
            return Logger.LogError(preAction, TargetType.RegistryValue, value.Target, rkEx.ReasonToString(), whatIf);
        } catch (Exception ex) {
            return Logger.LogError(preAction, TargetType.RegistryValue, value.Target, ex.Message, whatIf);
        } finally {
            preAction.ActionNumber++;
        }
    }
}
