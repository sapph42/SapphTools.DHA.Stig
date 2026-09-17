using Microsoft.Win32;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using static SapphTools.DHA.Stig.Remediator.Classes.RegistryCommon;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal partial class RegistryValuePatternRemediator : IRemediator {
    private RegistryValuePatternRemediator() { }
    public static void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            throw new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not RegistryValuePatternValue val) {
            throw new ArgumentException(
                $"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type " + 
                $"RegistryValuePatternValue, was {data.GetType().Name}"
            );
        }
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = batch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex
        };
        if (GetHive(val.Target) is not RegistryHive hive) {
            Logger.LogError(pre, TargetType.RegistryKey, val.Target, "Key path was not well-formed.", whatIf);
            return;
        }
        if (
            !TryGetHiveKey(pre, hive, out RegistryKey? hiveKey, whatIf) || 
            !CreateRegistryKey(hiveKey, val.Target, pre, out RegistryKey? targetKey, false, whatIf)
        ) {
            hiveKey?.Dispose();
            return;
        }
        hiveKey.Dispose();
        IEnumerable<RegistryKey> keys = [targetKey];
        keys = ExpandMatchingSubKeys(pre, keys, val.TargetPattern, transferOwnership: true, whatIf);
        keys = OpenSubkeys(pre, keys, val.SubPath, transferOwnership: true, whatIf);
        keys = ExpandMatchingSubKeys(pre, keys, val.PathPattern, transferOwnership: true, whatIf);
        foreach (RegistryKey key in keys) {
            CreateRegistryValue(key, val, pre, transferOwnership: true, whatIf);
        }
    }
    private static bool CreateRegistryKey(RegistryKey hive, string path, RemediationPreAction preAction, [NotNullWhen(true)] out RegistryKey? result, bool parsed = false, bool whatIf = true) {
        result = null;
        if (!parsed) {
            string? p = GetPath(path);
            if (p is null) {
                Logger.LogError(preAction, TargetType.RegistryKey, path, "Key path was not well-formed.", whatIf);
                return false;
            }
            path = p;
        }
        try {
            RegistryKey? key = hive.OpenSubKey(path);
            if (key is not null) {
                result = key;
                return true;
            }
        } catch (Exception ex) {
            Logger.LogError(preAction, TargetType.RegistryKey, path, $"Exception thrown opening key.: {ex.Message}", whatIf);
            return false;
        }
        string? parent = SplitPath(path, parent:true);
        string? leaf = SplitPath(path, parent:false);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf)) {
            Logger.LogError(preAction, TargetType.RegistryKey, path, "Attempted to create null key", whatIf);
            return false;
        }
        try {
            RegistryKey? parentKey = hive.OpenSubKey(parent);
            if (parentKey is null) {
                if (!CreateRegistryKey(hive, parent, preAction, out parentKey, true)) {
                    return false;
                }
                if (parentKey is null) {
                    Logger.LogError(preAction, TargetType.RegistryKey, parent, "Key did not exist after apparently successful creation.", whatIf);
                    return false;
                }
            }
            try {
                if (whatIf) {
                    Logger.LogWhatIf(
                        preAction,
                        TargetType.RegistryKey,
                        parent,
                        RollbackCapability.NotApplicable,
                        null,
                        new RegistryKeyValue() {
                            Target = parentKey!.ToString(),
                            Name = leaf
                        }
                    );
                    result = hive.OpenSubKey(parent)!;
                    return true;
                }
                result = parentKey.CreateSubKey(leaf);
                if (result is null) {
                    Logger.LogError(preAction, TargetType.RegistryKey, leaf, "Failed to create key", whatIf: false);
                    return false;
                }
                Logger.LogSuccess(
                    preAction,
                    TargetType.RegistryKey,
                    parent,
                    RollbackCapability.Automatic,
                    null,
                    new RegistryKeyValue() { 
                        Target = parentKey.ToString(),
                        Name = result.ToString()
                    }
                );
                return true;
            } catch {
                Logger.LogError(preAction, TargetType.RegistryKey, leaf, "Failed to create key", whatIf);
                return false;
            } finally {
                parentKey?.Dispose();
            }
        } catch (SecurityException) {
            Logger.LogError(preAction, TargetType.RegistryKey, parent, "Access denied attempting to open parent key", whatIf);
            return false;
        }
    }
    private static bool CreateRegistryValue(RegistryKey parent, RegistryValuePatternValue value, RemediationPreAction preAction, bool transferOwnership, bool whatIf = true) {
        object? currentVal = parent.GetValue(value.Name);
        RegistryValuePatternValue? before = null;
        if (currentVal is not null) {
            RegistryValueKind currKind = parent.GetValueKind(value.Name);
            before = new() {
                Target = value.Target,
                TargetPattern = value.TargetPattern,
                SubPath = value.SubPath,
                PathPattern = value.PathPattern,
                Name = value.Name,
                Data = value.Data,
                Kind = value.Kind,
                ResolvedTarget = new RegistryValueValue() {
                    Target = parent.ToString(),
                    Name = value.Name,
                    Data = currentVal,
                    Kind = currKind,
                    Overwrite = value.Overwrite
                }
            };
        }
        try {
            if (before is not null && !value.Overwrite) {
                Logger.LogNoAction(
                    preAction,
                    TargetType.RegistryValuePattern,
                    parent.ToString(),
                    before
                );
                return true;
            }
            if (before is not null && before.Data!.Equals(value.Data) && before.Kind.Equals(value.Kind)) {
                Logger.LogNoAction(
                    preAction,
                    TargetType.RegistryValuePattern,
                    parent.ToString(),
                    before
                );
                return true;
            }
            if (whatIf) {
                Logger.LogWhatIf(
                    preAction,
                    TargetType.RegistryValue,
                    parent.ToString(),
                    RollbackCapability.NotApplicable,
                    before,
                    new RegistryValueValue() {
                        Target = parent.ToString(),
                        Name = value.Name,
                        Data = value.Data,
                        Kind = value.Kind
                    }
                );
                return true;
            }
            try {
                parent.SetValue(value.Name, value.Data!, value.Kind);
                Logger.LogSuccess(
                    preAction,
                    TargetType.RegistryValuePattern,
                    parent.ToString(),
                    RollbackCapability.Automatic,
                    before,
                    new RegistryValuePatternValue() {
                        Target = value.Target,
                        TargetPattern = value.TargetPattern,
                        SubPath = value.SubPath,
                        PathPattern = value.PathPattern,
                        Name = value.Name,
                        Data = value.Data,
                        Kind = value.Kind,
                        ResolvedTarget = new RegistryValueValue() {
                            Target = parent.ToString(),
                            Name = value.Name,
                            Data = value.Data,
                            Kind = value.Kind
                        }
                    }
                );
                return true;
            } catch (Exception ex) {
                Logger.LogError(
                    preAction,
                    TargetType.RegistryValuePattern,
                    parent.ToString(),
                    $"Exception thrown setting value {value.Name} to kind {value.Kind} and data {value.Data}: {ex.Message}",
                    whatIf: false
                );
                return false;
            }
        } finally {
            if (transferOwnership) {
                parent.Dispose();
            }
        }
    }
    private static IEnumerable<RegistryKey> OpenSubkeys(RemediationPreAction preAction, IEnumerable<RegistryKey>? keys, string? subPath, bool transferOwnership, bool whatIf = true) {
        if (keys is null) {
            yield break;
        }
        if (string.IsNullOrWhiteSpace(subPath)) {
            foreach (RegistryKey key in keys) {
                yield return key;
            }
            yield break;
        }
        foreach (RegistryKey key in keys) {
            if (TryOpenSubKey(preAction, key, subPath, out RegistryKey? subkey, whatIf)) {
                yield return subkey;
            }
            if (TryCreateSubKey(preAction, key, subPath, out RegistryKey? newsubkey, whatIf)) {
                yield return newsubkey;
            }
            if (transferOwnership) {
                key.Dispose();
            }
        }
    }
    private static IEnumerable<RegistryKey> ExpandMatchingSubKeys(
            RemediationPreAction preAction, 
            IEnumerable<RegistryKey>? keys, 
            Regex? pattern,
            bool transferOwnership,
            bool whatIf) {
        if (keys is null) {
            yield break;
        }
        if (pattern is null) {
            foreach (RegistryKey key in keys) {
                yield return key;
            }
            yield break;
        }
        foreach (RegistryKey key in keys) {
            foreach (string subkeyName in key.GetSubKeyNames()) {
                if (pattern.Match(subkeyName).Success) {
                    if (TryOpenSubKey(preAction, key, subkeyName, out RegistryKey? subkey, whatIf)) {
                        yield return subkey;
                    }
                }
            }
            if (transferOwnership) {
                key.Dispose();
            }
        }
    }
    private static bool TryCreateSubKey(RemediationPreAction preAction, RegistryKey parent, string subkeyName, [NotNullWhen(true)] out RegistryKey? leaf, bool whatIf = true) {
        try {
            leaf = parent.OpenSubKey(subkeyName);
            if (leaf is not null) {
                return true;
            }
        } catch { }
        try {
            if (whatIf) {
                Logger.LogWhatIf(
                    preAction,
                    TargetType.RegistryKey,
                    parent.ToString(),
                    RollbackCapability.NotApplicable,
                    null,
                    new RegistryKeyValue() {
                        Target = parent.ToString(),
                        Name = subkeyName
                    }
                );
                leaf = parent;
                return true;
            }
            leaf = parent.CreateSubKey(subkeyName);
            if (leaf is null) {
                Logger.LogError(preAction, TargetType.RegistryKey, parent.Name + "\\" + subkeyName, "Failed to create key", whatIf: false);
                return false;
            }
            Logger.LogSuccess(
                preAction,
                TargetType.RegistryKey,
                parent.ToString(),
                RollbackCapability.Automatic,
                null,
                new RegistryKeyValue() {
                    Target = parent.ToString(),
                    Name = leaf.ToString()
                }
            );
            return true;
        } catch {
            leaf = null;
            Logger.LogError(preAction, TargetType.RegistryKey, parent.Name + "\\" + subkeyName, "Failed to create key", whatIf: false);
            return false;
        }
    }
}
