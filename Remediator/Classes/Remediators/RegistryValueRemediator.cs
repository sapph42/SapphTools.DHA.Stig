using Microsoft.Win32;
using SapphTools.DHA.Stig.Common.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Security;
using static SapphTools.DHA.Stig.Remediator.Classes.RegistryCommon;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal partial class RegistryValueRemediator : IRemediator {
    private RegistryValueRemediator() { }
    public static void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true) {
        Debug.WriteLine($"Starting remediation of {rule.RuleId} on {computerName}");
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            throw new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not RegistryValueValue val) {
            throw new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type RegistryValueValue, was {data.GetType().Name}");
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
        if (TryGetHiveKey(pre, hive, out RegistryKey? hiveKey, whatIf) && CreateRegistryKey(hiveKey, val.Target, pre, false, whatIf)) {
            SetValue(pre, val, whatIf);
        }
    }
    internal static bool CreateRegistryKey(RegistryKey hive, string path, RemediationPreAction preAction, bool parsed = false, bool whatIf = true) {
        if (!parsed) {
            string? p = GetPath(path);
            if (p is null) {
                Logger.LogError(preAction, TargetType.RegistryKey, path, "Key path was not well-formed.", whatIf);
                return false;
            }
            path = p;
        }
        string? parent = SplitPath(path, parent:true);
        string? leaf = SplitPath(path, parent:false);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf)) {
            Logger.LogError(preAction, TargetType.RegistryKey, path, "Attempted to create null key", whatIf);
            return false;
        }
        try {
            using RegistryKey? key = hive.OpenSubKey(path);
            if (key is not null) {
                using RegistryKey? parentKey = hive.OpenSubKey(parent!, true);
                Logger.LogNoAction(
                    preAction,
                    TargetType.RegistryKey,
                    path,
                    new RegistryKeyValue() {
                        Target = parentKey!.ToString(),
                        Name = leaf
                    }
                );
                return true;
            }
        } catch (Exception ex) {
            Logger.LogError(preAction, TargetType.RegistryKey, path, $"Exception thrown opening key.: {ex.Message}", whatIf);
            return false;
        }
        try {
            RegistryKey? parentKey = hive.OpenSubKey(parent, true);
            if (parentKey is null) {
                if (!CreateRegistryKey(hive, parent, preAction, true, whatIf)) {
                    return false;
                }
                parentKey = hive.OpenSubKey(parent, true);
                if (parentKey is null) {
                    Logger.LogError(preAction, TargetType.RegistryKey, parent, "Key did not exist after apparently successful creation.", whatIf);
                    return false;
                }
            }
            RegistryKey? leafKey = null;
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
                return true;
            }
            try {
                leafKey = parentKey.CreateSubKey(leaf);
                if (leafKey is null) {
                    Logger.LogError(preAction, TargetType.RegistryKey, leaf, "Failed to create key", whatIf);
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
                        Name = leaf
                    }
                );
                return true;
            } catch {
                Logger.LogError(preAction, TargetType.RegistryKey, leaf, "Failed to create key", whatIf);
                return false;
            } finally {
                parentKey?.Dispose();
                leafKey?.Dispose();
            }
        } catch (SecurityException) {
            Logger.LogError(preAction, TargetType.RegistryKey, parent, "Access denied attempting to open parent key", whatIf);
            return false;
        }
    }
    private static bool CreateRegistryValue(RegistryKey parent, RegistryValueValue value, RemediationPreAction preAction, bool whatIf = true) {
        object? currentVal = parent.GetValue(value.Name);
        RegistryValueValue? before = null;
        if (currentVal is not null) {
            RegistryValueKind currKind = parent.GetValueKind(value.Name);
            before = new() {
                Target = value.Target,
                Name = value.Name,
                Data = currentVal,
                Kind = currKind,
                Overwrite = value.Overwrite
            };
        } 
        if (before is not null && !value.Overwrite) {
            Logger.LogNoAction(
                preAction,
                TargetType.RegistryValue,
                parent.ToString(),
                before
            );
            return true;
        }
        if (before is not null && before.Data!.Equals(value.Data) && before.Kind.Equals(value.Kind)) {
            Logger.LogNoAction(
                preAction,
                TargetType.RegistryValue,
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
                TargetType.RegistryValue,
                parent.ToString(),
                RollbackCapability.Automatic,
                before,
                new RegistryValueValue() {
                    Target = parent.ToString(),
                    Name = value.Name,
                    Data = value.Data,
                    Kind = value.Kind
                }
            );
            return true;
        } catch (Exception ex) {
            Logger.LogError(
                preAction,
                TargetType.RegistryValue,
                parent.ToString(),
                $"Exception thrown setting value {value.Name} to kind {value.Kind} and data {value.Data}: {ex.Message}",
                whatIf: false
            );
            return false;
        }
    }
    private static void SetValue(
            RemediationPreAction preAction,
            RegistryValueValue value,
            bool whatIf = true) {
        if (!BasicChecks(preAction, value.Target, out RegistryKey? targetKey, whatIf)) {
            return;
        }
        using (targetKey) {
            CreateRegistryValue(targetKey, value, preAction, whatIf);
        }
    }
}
