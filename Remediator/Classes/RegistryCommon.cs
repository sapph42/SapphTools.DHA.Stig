using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Remediator.Classes;
internal static partial class RegistryCommon {
    public static bool BasicChecks(RemediationPreAction preAction, string target, [NotNullWhen(true)] out RegistryKey? leaf, bool whatIf) {
        leaf = null;
        if (GetHive(target) is not RegistryHive hive) {
            Logger.LogError(preAction, TargetType.RegistryKey, target, "Could not resolve hive from path", whatIf);
            return false;
        }
        string? leafPath = GetPath(target);
        if (leafPath is null) {
            Logger.LogError(preAction, TargetType.RegistryKey, target, "Key path was not well-formed.", whatIf);
            return false;
        }
        RegistryKey baseKey;
        try {
            baseKey = preAction.ComputerName switch {
                string t when t is not null => RegistryKey.OpenRemoteBaseKey(hive, t),
                _ => RegistryKey.OpenBaseKey(hive, RegistryView.Registry64)
            };
        } catch {
            Logger.LogError(preAction, TargetType.RegistryKey, hive.ToString(), "Could not open hive.", whatIf);
            return false;
        }
        return TryOpenSubKey(preAction, baseKey, leafPath, out leaf, whatIf);
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
    public static bool TryGetHiveKey(RemediationPreAction preAction, RegistryHive hive, [NotNullWhen(true)] out RegistryKey? leaf,  bool whatIf) {
        string path = hive switch {
            RegistryHive.ClassesRoot     => "HKEY_CLASSES_ROOT",
            RegistryHive.CurrentUser     => "HKEY_CURRENT_USER",
            RegistryHive.LocalMachine    => "HKEY_LOCAL_MACHINE",
            RegistryHive.Users           => "HKEY_USERS",
            RegistryHive.PerformanceData => "HKEY_PERFORMANCE_DATA",
            RegistryHive.CurrentConfig   => "HKEY_CURRENT_CONFIG",
            _ => throw new ArgumentException("Invalid RegistryHive", nameof(hive)),
        };
        leaf = null;
        try {
            leaf = preAction.ComputerName switch {
                string t when t is not null => RegistryKey.OpenRemoteBaseKey(hive, t),
                _ => RegistryKey.OpenBaseKey(hive, RegistryView.Registry64)
            };
            return true;
        } catch {
            Logger.LogError(preAction, TargetType.RegistryKey, hive.ToString(), "Could not open hive.", whatIf);
            return false;
        }
    }
    public static bool TryOpenSubKey(RemediationPreAction preAction, RegistryKey key, string subKeyName, [NotNullWhen(true)] out RegistryKey? subkey, bool whatIf) {
        try {
            subkey = key.OpenSubKey(subKeyName, true);
            if (subkey is null) {
                return false;
            }
            return true;
        } catch (Exception ex) {
            subkey = null;
            Logger.LogError(preAction, TargetType.RegistryKey, key.Name + "\\" + subKeyName, ex.Message, whatIf);
            return false;
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
