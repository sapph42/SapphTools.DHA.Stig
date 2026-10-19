using Microsoft.Win32;
using System.Diagnostics;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal partial class RegistryValueRemediator : IRemediator {
    private RegistryValueRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not RegistryValueValue val) {
            throw new ArgumentException($"Expected {target} to be of type RegistryValueValue, was {target.GetType().Name}");
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
                return targetKey.GetValueValue(val.Name);
            } catch {
                return null;
            }
        } catch {
            return null;
        } finally {
            targetKey?.Dispose();
        }
    }
    public static RemediationActionResult Remediate(RemediatorDto dto) {
        Debug.WriteLine($"Starting remediation of {dto.RuleId} on {dto.ComputerName}");
        IValue data = dto.Setting.Data.Clone();
        if (data is not RegistryValueValue val) {
            return new ArgumentException(
                $"Expected {nameof(dto)}.Setting.Data to be of type RegistryValueValue, was {data.GetType().Name}"
            );
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        RemediationActionResult createRes = RegistryKeyRemediator.SubRemediate(val.Target, targetHost, pre, out RegKey? key, dto.WhatIf);
        if (createRes.Value?.Result == ActionResult.WhatIf) {
            key?.Dispose();
            return Logger.LogWhatIf(pre, TargetType.RegistryValue, val.TargetString, RollbackCapability.NotApplicable, null, null);
        }
        if (createRes.IsSuccess && key is not null) {
            try {
                RegistryValueValue before = new() {
                    Target = val.Target,
                    Name = val.Name,
                    Data = key.GetValue(val.Name),
                    Kind = key.GetValueKind(val.Name)
                };
                return SubRemediate(pre, before, key, val, dto.WhatIf);
            } catch (Exception ex) {
                return Logger.LogError(pre, TargetType.RegistryValue, val.Target, ex.Message, false);
            } finally {
                key.Dispose();
            }
        }
        return createRes;
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) {
        Debug.WriteLine($"Starting rollback of {logEntry.RuleId} on {logEntry.ComputerName}");
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
        try {
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
            try {
                RegKey key = new(newVal.Target, targetHost);
                try {
                    RegistryValueValue before = new() {
                        Target = newVal.Target,
                        Name = newVal.Name,
                        Data = key.GetValue(newVal.Name),
                        Kind = key.GetValueKind(newVal.Name)
                    };
                    return SubRemediate(pre, before, key, oldVal, false);
                } catch (Exception ex) {
                    return Logger.LogError(pre, TargetType.RegistryValue, oldVal.Target, ex.Message, false);
                } finally {
                    key.Dispose();
                }
            } catch (RegKeyException rkEx) when (rkEx.Reason == RegKeyExceptionReason.NoSuchKey) {
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
            } 
        } catch (RegKeyException rkEx) {
            return Logger.LogError(pre, TargetType.RegistryValue, logEntry.Before?.TargetString ?? string.Empty, rkEx.ReasonToString(), false);
        } catch (Exception ex) {
            return ex;
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
                    return Logger.LogWhatIf(preAction, TargetType.RegistryValue, key.FullName, RollbackCapability.NotApplicable, before, null);
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
                if (value.Data.Equals(currentState.Data) && value.Kind == currentState.Kind) {
                    return Logger.LogNoAction(
                        preAction,
                        TargetType.RegistryValue,
                        key.FullName,
                        before
                    );
                }
                try {
                    if (whatIf) {
                        return Logger.LogWhatIf(preAction, TargetType.RegistryValue, key.FullName, RollbackCapability.NotApplicable, before, null);
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
