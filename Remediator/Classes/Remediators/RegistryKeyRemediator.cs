namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class RegistryKeyRemediator : IRemediator {
    private RegistryKeyRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not RegistryKeyValue val) {
            throw new ArgumentException($"Expected {target} to be of type RegistryKeyValue, was {target.GetType().Name}");
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
                using RegKey? tk = targetKey.OpenSubKey(val.Name);
                if (tk is null) {
                    return null;
                }
                return tk.GetKeyValue();
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
        logger ??= new();
        IValue data = dto.Setting.Data.Clone();
        if (data is not RegistryKeyValue val) {
            return new ArgumentException($"Expected {nameof(dto)}.Setting.Data to be of type RegistryKeyValue, was {data.GetType().Name}");
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        RegKey? key = null;
        try {
            return SubRemediate(val.Target, targetHost, pre, out key, dto.WhatIf, logger);
        } finally {
            key?.Dispose();
        }
    }
    public static RemediationActionResult SubRemediate(string targetPath, string targetHost, RemediationPreAction pre, out RegKey? key, bool whatIf, Logger logger) {
        key = null;
        RegKey? ancestor = null;
        try {
            ancestor = RegKey.ClosestExtantAncestor(targetPath, targetHost);
            if (ancestor is null) {
                return logger.LogError(pre, TargetType.RegistryKey, targetPath, "Could not find any valid ancestor keys", whatIf);
            }
            return CreateRegistryPath(ancestor, targetPath, pre, out key, whatIf, logger);
        } catch (RegKeyException rkEx) {
            return logger.LogError(pre, TargetType.RegistryKey, targetPath, rkEx.ReasonToString(), whatIf);
        } catch (Exception ex) {
            return logger.LogError(pre, TargetType.RegistryKey, targetPath, ex.Message, whatIf);
        } finally {
            if (!ReferenceEquals(ancestor, key)) {
                ancestor?.Dispose();
            }
        }
    }
    public static RemediationActionResult SubRemediate(RegKey parentKey, string branchName, RemediationPreAction pre, out RegKey? key, bool whatIf, Logger logger) {
        key = parentKey;
        try {
            string targetPath = parentKey.FullName + '\\' + branchName.TrimStart('\\');
            return CreateRegistryPath(parentKey, targetPath, pre, out key, whatIf, logger);
        } catch (RegKeyException rkEx) {
            return logger.LogError(pre, TargetType.RegistryKey, parentKey.FullName + '\\' + branchName, rkEx.ReasonToString(), whatIf);
        } catch (Exception ex) {
            return logger.LogError(pre, TargetType.RegistryKey, parentKey.FullName + '\\' + branchName, ex.Message, whatIf);
        }
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger = null) {
        if (logEntry.After is null) {
            return new ArgumentException("Cannot rollback a log entry with a null After property.");
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
                return logger.LogError(pre, TargetType.RegistryKey, newVal.Target, "Could not find parent key", false);
            }
            if (!parent.FullName.Equals(newVal.Target, StringComparison.OrdinalIgnoreCase)) {
                return logger.LogNoAction(pre, TargetType.RegistryKey, newVal.Target + '\\' + newVal.Name, null);
            }
            target = parent.OpenSubKey(newVal.Name);
            if (target is null) {
                return logger.LogNoAction(pre, TargetType.RegistryKey, parent.FullName + '\\' + newVal.Name, null);
            }
            return TryRemoveRegistryKey(target, pre, logger);
        } catch (RegKeyException rkEx) {
            return logger.LogError(pre, TargetType.RegistryKey, newVal.Target, rkEx.ReasonToString(), false);
        } catch (Exception ex) {
            return logger.LogError(pre, TargetType.RegistryKey, newVal.Target, ex.Message, false);
        } finally {
            parent?.Dispose();
            target?.Dispose();
        }
    }
    public static RemediationActionResult CreateRegistryPath(RegKey existingParent, string targetPath, RemediationPreAction preAction, out RegKey? key, bool whatIf, Logger logger) {
        RegistryKeyValue before = new() {
            Target = RegKey.SplitPath(existingParent.FullName, true) ?? string.Empty,
            Name = existingParent.Name,
        };
        RegistryKeyValue after;
        key = null;
        if (existingParent.FullName.Equals(targetPath, StringComparison.OrdinalIgnoreCase)) {
            key = existingParent;
            try {
                return logger.LogNoAction(preAction, TargetType.RegistryKey, targetPath, before);
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
                        return logger.LogWhatIf(preAction, TargetType.RegistryKey, targetPath, RollbackCapability.NotApplicable, before, null);
                    }
                    nextChild = RegKey.CreateKey(nextChild, targetPath, recurse: false, transferOwnership: true);
                    after = new() {
                        Target = RegKey.SplitPath(nextChild.FullName, true) ?? string.Empty,
                        Name = nextChild.Name,
                    };
                    final = logger.LogSuccess(
                        preAction,
                        TargetType.RegistryKey,
                        targetPath,
                        RollbackCapability.Automatic,
                        null,
                        after);
                } catch (RegKeyException rkEx) {
                    return logger.LogError(preAction, TargetType.RegistryKey, targetPath, rkEx.ReasonToString(), whatIf);
                } catch (Exception ex) {
                    return logger.LogError(preAction, TargetType.RegistryKey, targetPath, ex.Message, whatIf);
                } finally {
                    preAction.ActionNumber++;
                }
            }
            key = nextChild;
            return final!;
        }
    }
    private static RemediationActionResult TryRemoveRegistryKey(RegKey key, RemediationPreAction preAction, Logger logger) {
        RegistryKeyValue before = new() {
            Target = RegKey.SplitPath(key.FullName, true) ?? string.Empty,
            Name = key.Name,
        };
        using (key) {
            try {
                if (key.SubKeyCount + key.ValueCount > 0) {
                    return logger.LogError(
                        preAction,
                        TargetType.RegistryKey,
                        key.FullName,
                        "Cannot rollback a non-empty key.",
                        whatIf: false
                    );
                }
                key.Delete(false);
                return logger.LogSuccess(
                    preAction,
                    TargetType.RegistryKey,
                    key.FullName,
                    RollbackCapability.NotApplicable,
                    before,
                    null);
            } catch (RegKeyException rkEx) {
                return logger.LogError(preAction, TargetType.RegistryKey, key.FullName, rkEx.ReasonToString(), false);
            } catch (Exception ex) {
                return logger.LogError(preAction, TargetType.RegistryKey, key.FullName, ex.Message, false);
            } finally {
                preAction.ActionNumber++;
            }
        }
    }
}
