using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal partial class RegistryValuePatternRemediator : IRemediator {
    private RegistryValuePatternRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        throw new NotImplementedException(); // Think about if this is even possible
    }
    public static RemediationActionResult Remediate(RemediatorDto dto) {
        IValue data = dto.Setting.Data.Clone();
        if (data is not RegistryValuePatternValue val) {
            return new ArgumentException(
                $"Expected {nameof(dto)}.Setting.Data to be of type " + 
                $"RegistryValuePatternValue, was {data.GetType().Name}"
            );
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        RemediationActionResult createRes = RegistryKeyRemediator.SubRemediate(val.Target, targetHost, pre, out RegKey? targetKey, dto.WhatIf);
        if (!createRes.IsSuccess || targetKey is null) {
            return createRes;
        }
        DisposableList<RegKey>? keys = null;
        DisposableList<RegKey>? firstRoundKeys = null;
        DisposableList<RegKey>? secondRoundKeys = null;
        DisposableList<RegKey>? thirdRoundKeys = null;
        try {
            keys = [targetKey];
            firstRoundKeys = ExpandMatchingSubKeys(keys, val.TargetPattern);
            secondRoundKeys = OpenSubkeys(pre, firstRoundKeys, val.SubPath, dto.WhatIf);
            thirdRoundKeys = ExpandMatchingSubKeys(secondRoundKeys, val.PathPattern);
            RemediationActionResult? res = null;
            foreach (RegKey key in thirdRoundKeys) {
                object? currentVal = key.GetValue(val.Name, val.Kind);
                RegistryValueKind currKind = key.GetValueKind(val.Name);
                RegistryValueValue resolvedBefore = ResolveBefore(key, val);
                RegistryValueValue resolvedTarget = ResolveTarget(key, val);
                RegistryValuePatternValue before = new() {
                    Target = val.Target,
                    TargetPattern = val.TargetPattern,
                    SubPath = val.SubPath,
                    PathPattern = val.PathPattern,
                    Name = val.Name,
                    Data = resolvedBefore.Data,
                    Kind = resolvedBefore.Kind,
                    ResolvedTarget = resolvedBefore
                };
                RemediationActionResult thisRes = RegistryValueRemediator.SubRemediate(pre, before, key, resolvedTarget, dto.WhatIf);
                if (res is null || !thisRes.IsSuccess) {
                    res = thisRes;
                }
            }
            if (res is null) {
                return Logger.LogNoAction(pre, TargetType.RegistryValuePattern, targetKey.ToString(), null);
            }
            return res;
        } finally {
            thirdRoundKeys?.Dispose();
            secondRoundKeys?.Dispose();
            firstRoundKeys?.Dispose();
            keys?.Dispose();
        }
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) {
        if (logEntry.After is null) {
            return new ArgumentException("Cannot rollback a log entry with a null After property.");
        }
        if (logEntry.Before is not RegistryValuePatternValue oldPattern) {
            return new ArgumentException(
                $"Expected {nameof(logEntry)}.Before to be of type RegistryValuePatternValue, was {logEntry.Before?.GetType().Name}"
            );
        }
        if (oldPattern.ResolvedTarget is not RegistryValueValue oldVal) {
            return new ArgumentException(
                $"Expected {nameof(logEntry)}.Before.ResolvedTarget to be of type RegistryValueValue, was {oldPattern.ResolvedTarget?.GetType().Name}"
            );
        }
        if (logEntry.After is not RegistryValueValue newVal) {
            return new ArgumentException(
                $"Expected {nameof(logEntry)}.After to be of type RegistryValueValue, was {logEntry.After?.GetType().Name}"
            );
        }
        RemediationAction ephemiral = new(logEntry.ToPreAction(), logEntry.Target) {
            TargetType = logEntry.TargetType,
            RollbackCapability = logEntry.RollbackCapability,
            Before = oldVal,
            After = newVal
        };
        return RegistryValueRemediator.Rollback(remBatch, ruleBatch, ephemiral);
    }
    private static DisposableList<RegKey> ExpandMatchingSubKeys(IEnumerable<RegKey>? keys, Regex? pattern) {
        DisposableList<RegKey> returnList = [];
        try {
            if (keys is null) {
                return returnList;
            }
            if (pattern is null) {
                foreach (RegKey key in keys) {
                    returnList.Add(key);
                }
                return returnList;
            }
            foreach (RegKey key in keys) {
                foreach (RegKey subKey in key.GetSubKeys(pattern)) {
                    returnList.Add(subKey);
                }
            }
            return returnList;
        } catch {
            returnList.Dispose();
            throw;
        }
    }
    private static DisposableList<RegKey> OpenSubkeys(RemediationPreAction preAction, IEnumerable<RegKey>? keys, string? subPath, bool whatIf = true) {
        DisposableList<RegKey> returnList = [];
        try { 
            if (keys is null) {
                return returnList;
            }
            if (string.IsNullOrWhiteSpace(subPath)) {
                foreach (RegKey key in keys) {
                    returnList.Add(key);
                }
                return returnList;
            }
            foreach (RegKey key in keys) {
                if (key.OpenSubKey(subPath) is RegKey subKey) {
                    returnList.Add(subKey);
                } else {
                    if (RegistryKeyRemediator.SubRemediate(key, subPath, preAction, out RegKey? newKey, whatIf).IsSuccess && newKey is not null) {
                        returnList.Add(newKey);
                    }
                }
            }
            return returnList;
        } catch {
            returnList.Dispose();
            throw;
        }
    }
    private static RegistryValueValue ResolveBefore(RegKey key, RegistryValuePatternValue val) {
        object? currentVal = key.GetValue(val.Name);
        RegistryValueKind currKind = key.GetValueKind(val.Name);
        return new() {
            Target = key.FullName,
            Name = val.Name,
            Data = currentVal,
            Kind = currKind,
            Overwrite = val.Overwrite
        };
    }
    private static RegistryValueValue ResolveTarget(RegKey key, RegistryValuePatternValue val) {
        return new() {
            Target = key.FullName,
            Name = val.Name,
            Data = val.Data,
            Kind = val.Kind,
            Overwrite = val.Overwrite
        };
    }
}
