using Microsoft.Win32;
using System.IO;
using static SapphTools.DHA.Stig.Remediator.Classes.RegistryCommon;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class RegistryKeyRemediator : IRemediator {
    private RegistryKeyRemediator() { }
    public static RemediationActionResult Remediate(Rule rule, int settingIndex, Guid remBatch, Guid ruleBatch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            return new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not RegistryKeyValue val) {
            return new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type RegistryKeyValue, was {data.GetType().Name}");
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
        RegKey? key = null;
        try {
            return SubRemediate(val.Target, targetHost, pre, out key, whatIf);
        } finally {
            key?.Dispose();
        }
    }
    public static RemediationActionResult SubRemediate(string targetPath, string targetHost, RemediationPreAction pre, out RegKey? key, bool whatIf) {
        key = null;
        RegKey? ancestor = null;
        try {
            ancestor = RegKey.ClosestExtantAncestor(targetPath, targetHost);
            if (ancestor is null) {
                return Logger.LogError(pre, TargetType.RegistryKey, targetPath, "Could not find any valid ancestor keys", whatIf);
            }
            return CreateRegistryPath(ancestor, targetPath, pre, out key, whatIf);
        } catch (RegKeyException rkEx) {
            return Logger.LogError(pre, TargetType.RegistryKey, targetPath, rkEx.ReasonToString(), whatIf);
        } catch (Exception ex) {
            return Logger.LogError(pre, TargetType.RegistryKey, targetPath, ex.Message, whatIf);
        } finally {
            ancestor?.Dispose();
        }
    }
    public static RemediationActionResult SubRemediate(RegKey parentKey, string branchName, RemediationPreAction pre, out RegKey? key, bool whatIf) {
        key = parentKey;
        try {
            return CreateRegistryPath(parentKey, branchName, pre, out key, whatIf);
        } catch (RegKeyException rkEx) {
            return Logger.LogError(pre, TargetType.RegistryKey, parentKey.FullName + '\\' + branchName, rkEx.ReasonToString(), whatIf);
        } catch (Exception ex) {
            return Logger.LogError(pre, TargetType.RegistryKey, parentKey.FullName + '\\' + branchName, ex.Message, whatIf);
        }
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) {
        if (logEntry.After is null) {
            return new ArgumentException("Cannot rollback a log entry with a null After property.");
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
        if (logEntry.Before is not null) {
            return RemediationActionResult.GenerateWithoutLog(
                pre,
                TargetType.RegistryKey,
                logEntry.Before.TargetString,
                RollbackCapability.NotApplicable,
                null,
                null,
                ActionResult.NoActionTaken
            );
        }
        IValue current = logEntry.After;
        if (current is not RegistryKeyValue newVal) {
            return new ArgumentException(
                $"Expected {nameof(logEntry)}.After to be of type RegistryKeyValue, was {current.GetType().Name}"
            );
        }
        RegKey? parent = null;
        RegKey? target = null;
        try {
            parent = RegKey.ClosestExtantAncestor(newVal.Target, targetHost);
            if (parent is null) {
                return Logger.LogError(pre, TargetType.RegistryKey, newVal.Target, "Could not find parent key", false);
            }
            if (!parent.FullName.Equals(newVal.Target, StringComparison.OrdinalIgnoreCase)) {
                return Logger.LogNoAction(pre, TargetType.RegistryKey, newVal.Target, null);
            }
            target = parent.OpenSubKey(newVal.Name);
            if (target is null) {
                return Logger.LogNoAction(pre, TargetType.RegistryKey, parent.FullName + '\\' + newVal.Name, null);
            }
            return TryRemoveRegistryKey(target, pre);
        } catch (RegKeyException rkEx) {
            return Logger.LogError(pre, TargetType.RegistryKey, newVal.Target, rkEx.ReasonToString(), false);
        } catch (Exception ex) {
            return Logger.LogError(pre, TargetType.RegistryKey, newVal.Target, ex.Message, false);
        } finally {
            parent?.Dispose();
            target?.Dispose();
        }
    }
    public static RemediationActionResult CreateRegistryPath(RegKey existingParent, string targetPath, RemediationPreAction preAction, out RegKey? key, bool whatIf) {
        RegistryKeyValue before = new() {
            Target = RegKey.SplitPath(existingParent.FullName, true) ?? string.Empty,
            Name = existingParent.Name,
        };
        RegistryKeyValue after;
        key = null;
        if (existingParent.FullName.Equals(targetPath, StringComparison.OrdinalIgnoreCase)) {
            key = existingParent;
            try {
                return Logger.LogNoAction(preAction, TargetType.RegistryKey, targetPath, before);
            } finally {
                preAction.ActionNumber++;
            }
        }
        RemediationActionResult? final = null;
        using (existingParent) {
            RegKey nextChild = existingParent.Clone();
            while (!nextChild.FullName.Equals(targetPath, StringComparison.OrdinalIgnoreCase)) {
                try {
                    if (whatIf) {
                        key = nextChild;
                        return Logger.LogWhatIf(preAction, TargetType.RegistryKey, targetPath, RollbackCapability.NotApplicable, before, null);
                    }
                    nextChild = RegKey.CreateKey(nextChild, targetPath, recurse: false, transferOwnership: true);
                    after = new() {
                        Target = RegKey.SplitPath(nextChild.FullName, true) ?? string.Empty,
                        Name = nextChild.Name,
                    };
                    final = Logger.LogSuccess(
                        preAction,
                        TargetType.RegistryKey,
                        targetPath,
                        RollbackCapability.Automatic,
                        null,
                        after);
                } catch (RegKeyException rkEx) {
                    return Logger.LogError(preAction, TargetType.RegistryKey, targetPath, rkEx.ReasonToString(), whatIf);
                } catch (Exception ex) {
                    return Logger.LogError(preAction, TargetType.RegistryKey, targetPath, ex.Message, whatIf);
                } finally {
                    preAction.ActionNumber++;
                }
            }
            key = nextChild;
            return final!;
        }
    }
    private static RemediationActionResult TryRemoveRegistryKey(RegKey key, RemediationPreAction preAction) {
        RegistryKeyValue before = new() {
            Target = RegKey.SplitPath(key.FullName, true) ?? string.Empty,
            Name = key.Name,
        };
        using (key) {
            try {
                if (key.SubKeyCount + key.ValueCount > 0) {
                    return Logger.LogError(
                        preAction,
                        TargetType.RegistryKey,
                        key.FullName,
                        "Cannot rollback a non-empty key.",
                        whatIf: false
                    );
                }
                key.Delete(false);
                return Logger.LogSuccess(
                    preAction,
                    TargetType.RegistryKey,
                    key.FullName,
                    RollbackCapability.NotApplicable,
                    before,
                    null);
            } catch (RegKeyException rkEx) {
                return Logger.LogError(preAction, TargetType.RegistryKey, key.FullName, rkEx.ReasonToString(), false);
            } catch (Exception ex) {
                return Logger.LogError(preAction, TargetType.RegistryKey, key.FullName, ex.Message, false);
            } finally {
                preAction.ActionNumber++;
            }
        }
    }
}
