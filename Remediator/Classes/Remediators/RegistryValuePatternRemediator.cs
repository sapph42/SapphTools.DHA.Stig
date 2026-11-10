using Microsoft.Win32;
using SapphTools.IDisposableEnumerable;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal partial class RegistryValuePatternRemediator : IRemediator {
    private RegistryValuePatternRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not RegistryValuePatternValue val) {
            throw new ArgumentException($"Expected {nameof(target)} to be of type RegistryValuePatternValue, was {target.GetType().Name}.");
        }
        RegistryValueSetValue state = new(val);
        RegKey? baseKey;
        try {
            try {
                baseKey = new(val.Target, computerName);
                if (baseKey is null) {
                    return state;
                }
            } catch {
                return state;
            }
            try {
                using DisposableList<RegKey>? keys = [baseKey];
                using DisposableList<RegKey>? firstRoundKeys = ExpandMatchingSubKeys(keys, val.TargetPattern);
                using DisposableList<RegKey>? secondRoundKeys = OpenSubkeys(firstRoundKeys, val.SubPath);
                using DisposableList<RegKey>? thirdRoundKeys = ExpandMatchingSubKeys(secondRoundKeys, val.PathPattern);
                foreach (RegKey key in thirdRoundKeys) {
                    state.Values.Add(ResolveBefore(key, val));
                }
                return state;
            } catch {
                return state;
            }
        } catch {
            return state;
        }
    }
    public static RemediationActionResult Remediate(RemediatorDto dto, Logger? logger = null) {
        logger ??= new();
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
        RemediationActionResult createRes = RegistryKeyRemediator.SubRemediate(
            val.Target, targetHost, pre, out RegKey? targetKey, dto.WhatIf, logger, logExistingKey: false);
        if (!createRes.IsSuccess || targetKey is null) {
            return createRes;
        }
        RemediationActionResult? keyResult = createRes.HasValue ? createRes : null;
        DisposableList<RegKey>? keys = null;
        DisposableList<RegKey>? firstRoundKeys = null;
        DisposableList<RegKey>? secondRoundKeys = null;
        DisposableList<RegKey>? thirdRoundKeys = null;
        try {
            keys = [targetKey];
            firstRoundKeys = ExpandMatchingSubKeys(keys, val.TargetPattern);
            secondRoundKeys = OpenSubkeys(pre, firstRoundKeys, val.SubPath, targetHost, logger, dto.WhatIf, ref keyResult);
            thirdRoundKeys = ExpandMatchingSubKeys(secondRoundKeys, val.PathPattern);
            RemediationActionResult? res = null;
            foreach (RegKey key in thirdRoundKeys) {
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
                RemediationActionResult thisRes = RegistryValueRemediator.SubRemediate(pre, before, key, resolvedTarget, logger, dto.WhatIf);
                res = PreferResult(res, thisRes);
            }
            return PreferResult(res, keyResult) ??
                logger.LogNoAction(pre, TargetType.RegistryValuePattern, val.TargetString, null);
        } finally {
            thirdRoundKeys?.Dispose();
            secondRoundKeys?.Dispose();
            firstRoundKeys?.Dispose();
            keys?.Dispose();
        }
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger = null) {
        logger ??= new();
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
        return RegistryValueRemediator.Rollback(remBatch, ruleBatch, ephemiral, logger);
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
    private static RemediationActionResult? PreferResult(RemediationActionResult? current, RemediationActionResult? candidate) {
        if (candidate is null) {
            return current;
        }
        if (current is null || (current.IsSuccess && (!candidate.IsSuccess ||
                (current.Value?.Result == ActionResult.NoActionTaken &&
                 candidate.Value?.Result is ActionResult.ActionSuccess or ActionResult.WhatIf)))) {
            return candidate;
        }
        return current;
    }
    private static DisposableList<RegKey> OpenSubkeys(RemediationPreAction preAction, IEnumerable<RegKey>? keys, string? subPath, string targetHost, Logger logger, bool whatIf, ref RemediationActionResult? keyResult) {
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
                    string targetPath = key.FullName + '\\' + subPath.TrimStart('\\');
                    RemediationActionResult createResult = RegistryKeyRemediator.SubRemediate(
                        targetPath, targetHost, preAction, out RegKey? newKey, whatIf, logger, logExistingKey: false);
                    if (!createResult.IsSuccess || createResult.HasValue) {
                        keyResult = PreferResult(keyResult, createResult);
                    }
                    if (newKey is not null) {
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
    private static DisposableList<RegKey> OpenSubkeys(IEnumerable<RegKey>? keys, string? subPath) {
        DisposableList<RegKey> returnList = [];
        try {
            if (keys is null) {
                return returnList;
            }
            if (string.IsNullOrWhiteSpace(subPath)) {
                if (keys is DisposableList<RegKey> list) {
                    return list;
                }
                foreach (RegKey key in keys) {
                    returnList.Add(key);
                }
                return returnList;
            }
            foreach (RegKey key in keys) {
                if (key.OpenSubKey(subPath) is RegKey subKey) {
                    returnList.Add(subKey);
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
