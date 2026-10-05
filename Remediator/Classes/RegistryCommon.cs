using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Remediator.Classes;
internal static partial class RegistryCommon {
    public static RemediationActionResult BasicChecks(RemediationPreAction preAction, string target, [NotNullWhen(true)] out RegistryKey? leaf, bool whatIf) {
        leaf = null;
        if (GetHive(target) is not RegistryHive hive) {
            return Logger.LogError(preAction, TargetType.RegistryKey, target, "Could not resolve hive from path", whatIf);
        }
        string? leafPath = GetPath(target);
        if (leafPath is null) {
            return Logger.LogError(preAction, TargetType.RegistryKey, target, "Key path was not well-formed.", whatIf);
        }
        RegistryKey baseKey;
        try {
            baseKey = preAction.ComputerName switch {
                string t when t is not null => RegistryKey.OpenRemoteBaseKey(hive, t),
                _ => RegistryKey.OpenBaseKey(hive, RegistryView.Registry64)
            };
        } catch {
            return Logger.LogError(preAction, TargetType.RegistryKey, hive.ToString(), "Could not open hive.", whatIf);
        }
        return TryOpenSubKey(preAction, baseKey, leafPath, out leaf, whatIf);
    }
    public static bool CheckRegistryKey(RegistryKey hive, string path, [NotNullWhen(true)] out RegistryKey? key, bool parsed = false) {
        if (!parsed) {
            string? p = GetPath(path) ?? throw new ArgumentException("Key path was not well-formed.", nameof(path));
            path = p;
        }
        string? parent = SplitPath(path, parent:true);
        string? leaf = SplitPath(path, parent:false);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf)) {
            throw new ArgumentException("Attempted to open null key", nameof(path));
        }
        key = hive.OpenSubKey(path, true);
        if (key is null) {
            return false;
        }
        return true;
    }
    public static RemediationActionResult CheckRegistryKey(RegistryKey hive, string path, RemediationPreAction preAction, bool parsed = false, bool whatIf = true) {
        try {
            if (CheckRegistryKey(hive, path, out RegistryKey? ret, parsed)) {
                using (ret) { }
                return RemediationActionResult.IntermediateSuccessFactory();
            } else {
                return RemediationActionResult.GenerateWithoutLog(
                    preAction,
                    TargetType.RegistryKey,
                    path,
                    RollbackCapability.NotApplicable,
                    null,
                    null,
                    ActionResult.NoActionTaken
                );
            }
        } catch (ArgumentException argEx) {
            return Logger.LogError(preAction, TargetType.RegistryKey, path, argEx.Message, whatIf);
        } catch (Exception ex) {
            return Logger.LogError(preAction, TargetType.RegistryKey, path, $"Exception thrown opening key.: {ex.Message}", whatIf);
        }
    }
    public static RegistryHive? GetHive(string path) {
        Match match = PowerShellHivePattern().Match(path);
        if (match.Success) {
            return match.Value switch {
                "HKLM" => RegistryHive.LocalMachine,
                "HKCU" => RegistryHive.CurrentUser,
                "HKCR" => RegistryHive.ClassesRoot,
                "HKU" => RegistryHive.Users,
                _ => null
            };
        }
        match = StandardHivePattern().Match(path);
        if (match.Success) {
            return match.Value switch {
                "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
                "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
                "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
                "HKEY_USERS" => RegistryHive.Users,
                _ => null
            };
        }
        return null;
    }
    public static string? GetPath(string path) {
        Match match = PowerShellKeyPattern().Match(path);
        if (match.Success) {
            return match.Value;
        } else {
            match = StandardKeyPattern().Match(path);
            if (match.Success) {
                return match.Value;
            } else {
                return null;
            }
        }
    }
    public static string? SplitPath(string path, bool parent) {
        string[] parts = path.Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) {
            return null;
        }
        if (parent) {
            return string.Join('\\', parts[..^1]);
        }
        if (parts.Length == 1) {
            return null;
        }
        return parts[^1];
    }
    public static RemediationActionResult TryGetHiveKey(RemediationPreAction preAction, RegistryHive hive, out RegistryKey? leaf, bool whatIf) {
        leaf = null;
        try {
            leaf = preAction.ComputerName switch {
                string t when t is not null => RegistryKey.OpenRemoteBaseKey(hive, t),
                _ => RegistryKey.OpenBaseKey(hive, RegistryView.Registry64)
            };
            return RemediationActionResult.IntermediateSuccessFactory();
        } catch {
            return Logger.LogError(preAction, TargetType.RegistryKey, hive.ToString(), "Could not open hive.", whatIf);
        }
    }
    public static RemediationActionResult TryOpenSubKey(
            RemediationPreAction preAction, 
            RegistryKey key, 
            string subKeyName, 
            out RegistryKey? subkey, 
            bool whatIf) {
        try {
            subkey = key.OpenSubKey(subKeyName, true);
            if (subkey is null) {
                return Logger.LogError(
                    preAction, 
                    TargetType.RegistryKey, 
                    key.Name + "\\" + subKeyName, 
                    "Could not open key", 
                    whatIf
                );
            }
            return RemediationActionResult.IntermediateSuccessFactory();
        } catch (Exception ex) {
            subkey = null;
            return Logger.LogError(preAction, TargetType.RegistryKey, key.Name + "\\" + subKeyName, ex.Message, whatIf);
        }
    }

    [GeneratedRegex(@"(HKCU|HKLM|HKCR|HKU)(?=:)")]
    private static partial Regex PowerShellHivePattern();

    [GeneratedRegex(@"HKEY_[^:\\]+")]
    private static partial Regex StandardHivePattern();

    [GeneratedRegex(@"(?<=(?:HKCU|HKLM|HKCR|HKU):\\).*")]
    private static partial Regex PowerShellKeyPattern();

    [GeneratedRegex(@"(?<=HKEY_[A-Za-z_]+:?\\).*")]
    private static partial Regex StandardKeyPattern();
}
