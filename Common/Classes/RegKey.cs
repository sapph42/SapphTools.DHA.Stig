using Microsoft.Win32;
using SapphTools.SecurityDescriptor;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Common.Classes; 
public partial class RegKey : IDisposable, IEquatable<RegistryKey>, IEquatable<RegKey> {
    protected readonly RegistryKey _key;
    protected readonly string _keyName;
    protected readonly string? computerName;
    protected Lazy<RegKey?> _parent;
    protected bool parentOwned = false;
    private bool disposedValue;

    public string FullName => _key.Name;
    public string Name => _keyName;
    public RegKey? Parent => _parent.Value;
    public int SubKeyCount => _key.SubKeyCount;
    public int ValueCount  => _key.ValueCount;

    private RegKey(RegistryKey key, string? remoteComputerName) : this(key) {
        computerName = remoteComputerName;
    }

    public RegKey(RegistryKey key) { 
        _key = key;
        _keyName = SplitPath(_key.Name, false)!;
        _parent = new(() => {
            string? parentPath = SplitPath(key.ToString(), true);
            if (parentPath is not null) {
                parentOwned = true;
                return new (parentPath, computerName);
            }
            return null;
        });

    }
    public RegKey(string path, string? computerName = null) : this(ResolvePath(path, computerName), computerName) { }

    public RegKey CreateSubKey(string name) {
        try {
            RegKey child = new(_key.CreateSubKey(name), computerName) {
                _parent = new(() => this),
                parentOwned = false
            };
            return child;
        } catch (Exception ex) {
            throw new RegKeyException(RegKeyExceptionReason.NativeException, ex);
        }
    }
    public void Delete(bool dispose) {
        if (Parent is null) {
            throw new RegKeyException(RegKeyExceptionReason.NoParentKey);
        }
        try {
            Parent._key.DeleteSubKey(Name, true);
        } catch (ArgumentException) {
            throw new RegKeyException(RegKeyExceptionReason.NoSuchKey);
        } catch (Exception ex) {
            throw new RegKeyException(RegKeyExceptionReason.NativeException, ex);
        } finally {
            if (dispose) {
                Dispose();
            }
        }
    }
    public void DeleteValue(string name, bool throwOnMissingValue) {
        _key.DeleteValue(name, throwOnMissingValue);
    }
    public RegistrySecurity GetAccessControl() {
        return _key.GetAccessControl();
    }
    public RegistryAclValue GetAclValue() {
        return new() {
            Target = FullName,
            Sddl = GetSddl()
        };
    }
    public RegistryKeyValue GetKeyValue() {
        return new() {
            Target = Parent?.Name ?? Name,
            Name = Parent?.Name is not null ? Name : string.Empty,
        };
    }
    public Sddl GetSddl(AccessControlSections includeSections = AccessControlSections.All) {
        return new(
            GetSecurityDescriptorSddlForm(includeSections),
            SecurityDescriptor.Enums.ObjectType.RegistryKey
        );
    }
    public IEnumerable<RegKey> GetSubKeys(Regex? pattern = null) {
        foreach (string subKeyName in _key.GetSubKeyNames()) {
            if (pattern is null || pattern.Match(subKeyName).Success) {
                RegKey? subKey = null;
                try {
                    subKey = OpenSubKey(subKeyName);
                } catch { }
                if (subKey is not null) {
                    yield return subKey;
                }
            }
        }
    }
    public object? GetValue(string? name, RegistryValueKind kind) {
        static int? SafeCastToInt(object? val) {
            if (val is null) return null;
            try {
                return (int)val;
            } catch {
                return null;
            }
        }
        static long? SafeCastToInt64(object? val) {
            if (val is null)
                return null;
            try {
                return (long)val;
            } catch {
                return null;
            }
        }
        return kind switch {
            RegistryValueKind.String => _key.GetValue(name) as string,
            RegistryValueKind.ExpandString => _key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string,
            RegistryValueKind.Binary => _key.GetValue(name) as byte[],
            RegistryValueKind.DWord => SafeCastToInt(_key.GetValue(name)),
            RegistryValueKind.MultiString => _key.GetValue(name) as string[],
            RegistryValueKind.QWord => SafeCastToInt64(_key.GetValue(name)),
            _ => _key.GetValue(name),
        };
    }
    public RegistryValueKind GetValueKind(string? name) {
        try {
            return _key.GetValueKind(name);
        } catch (IOException) {
            return RegistryValueKind.None;
        }
    }
    public RegKey? OpenKey(string path) {
        if (path.StartsWith(FullName)) {
            RegistryKey? key = _key.OpenSubKey(path[FullName.Length..]);
            if (key is null) {
                return null;
            }
            return new(key, computerName);
        }
        return new(path, computerName);
    }
    public RegKey? OpenSubKey(string name) {
        RegistryKey? sub = _key.OpenSubKey(name, true);
        if (sub is null) {
            return null;
        }
        RegKey subKey = new(sub, computerName) {
            parentOwned = true
        };
        return subKey;
    }
    public void SetSddl(Sddl sddl) {
        _key.SetAccessControl(sddl.ToRegistrySecurity());
    }
    public void SetValue(string? name, object value, RegistryValueKind kind) {
        if (kind == RegistryValueKind.None || kind == RegistryValueKind.Unknown) {
            _key.SetValue(name, value);
        } else {
            _key.SetValue(name, value, kind);
        }
    }

    public static RegKey CreateKey(string path, string? computerName, bool recurse) {
        RegKey? key;
        key = ClosestExtantAncestor(path, computerName);
        if (key is null) {
            throw new RegKeyException(RegKeyExceptionReason.CouldNotOpenHive);
        }
        if (key.FullName.Equals(path, StringComparison.OrdinalIgnoreCase)) {
            return key;
        }
        return CreateKey(key, path, recurse, true);
    }
    public static RegKey CreateKey(RegKey key, string path, bool recurse, bool transferOwnership) {
        if (key.FullName.Equals(path, StringComparison.OrdinalIgnoreCase)) {
            return key;
        }
        if (recurse) {
            return CreateKeyInternal(key, path);
        }
        string branch = path[key.FullName.Length..]
            .TrimStart('\\')
            .Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        RegKey child = key.CreateKey(branch);
        child._parent = new Lazy<RegKey?>(() => key);
        child.parentOwned = transferOwnership;
        _ = child.Parent;
        return child;
    }
    public static RegKey? ClosestExtantAncestor(string path, string? computerName) {
        try {
            return new(path, computerName);
        } catch { }
        string parentPath = SplitPath(path, true) ??
                throw new RegKeyException(RegKeyExceptionReason.KeyPathNotWellFormed);
        RegKey? parent;
        try {
            parent = new(ResolvePath(parentPath, computerName), computerName);
        } catch {
            parent = ClosestExtantAncestor(parentPath, computerName);
        }
        return parent;
    }
    public static RegistryHive? GetHive(string path, out string remainingPath) {
        Match match = PowerShellHivePattern().Match(path);
        if (match.Success) {
            remainingPath = PowerShellKeyPattern().Match(path).Value;
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
            remainingPath = StandardKeyPattern().Match(path).Value;
            return match.Value switch {
                "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
                "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
                "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
                "HKEY_USERS" => RegistryHive.Users,
                _ => null
            };
        }
        remainingPath = string.Empty;
        return null;
    }
    public static string? SplitPath(string path, bool parent) {
        if (path.StartsWith(@"\\")) {
            path = path[2..];
            string[] uriParts = path.Split('\\', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (uriParts.Length < 2) {
                return null;
            }
            path = uriParts[1];
        }
        string[] parts = path.Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) {
            return null;
        }
        if (parent) {
            string combined = string.Join('\\', parts[..^1]);
            if (string.IsNullOrWhiteSpace(combined.Trim('\\'))) {
                return null;
            }
            return combined;
        }
        if (parts.Length == 1) {
            return path;
        }
        return parts[^1];
    }

    private RegKey CreateKey(string name) {
        try {
            return new(_key.CreateSubKey(name, true), computerName);
        } catch (Exception ex) {
            throw new RegKeyException(RegKeyExceptionReason.NativeException, ex);
        }
    }
    private string GetSecurityDescriptorSddlForm(AccessControlSections includeSections) {
        return _key.GetAccessControl().GetSecurityDescriptorSddlForm(includeSections);
    }

    private static RegKey CreateKeyInternal(string path, string? computerName) {
        string parentPath = SplitPath(path, true) ??
                throw new RegKeyException(RegKeyExceptionReason.KeyPathNotWellFormed);
        RegKey parent;
        try {
            parent = new(ResolvePath(parentPath, computerName), computerName);
        } catch {
            parent = CreateKeyInternal(parentPath, computerName);
        }
        string leafName = SplitPath(path, false) ??
                throw new RegKeyException(RegKeyExceptionReason.KeyPathNotWellFormed);
        try {
            return parent.CreateKey(leafName);
        } catch (Exception ex) {
            throw new RegKeyException(RegKeyExceptionReason.NativeException, ex);
        }
    }
    private static RegKey CreateKeyInternal(RegKey key, string path) {
        if (key.Parent is null) {
            throw new RegKeyException(RegKeyExceptionReason.KeyPathNotWellFormed);
        }
        if (key.FullName.Equals(path, StringComparison.OrdinalIgnoreCase)) {
            return key;
        }
        string branch = path[key.FullName.Length..]
            .TrimStart('\\')
            .Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        RegKey child;
        try {
            child = key.CreateSubKey(branch);
            return CreateKeyInternal(child, path);
        } catch (RegKeyException) {
            throw;
        } catch (Exception ex) { 
            throw new RegKeyException(RegKeyExceptionReason.NativeException, ex);
        }
    }
    private static RegistryKey ResolvePath(string path, string? computerName) {
        try {
            if (GetHive(path, out string remainingPath) is not RegistryHive hive) {
                throw new RegKeyException(RegKeyExceptionReason.NoResolvableHive);
            }
            if (!TryGetHiveKey(hive, computerName, out RegistryKey? hiveKey)) {
                throw new RegKeyException(RegKeyExceptionReason.CouldNotOpenHive);
            }
            if (hiveKey.OpenSubKey(remainingPath, true) is not RegistryKey targetKey) {
                throw new RegKeyException(RegKeyExceptionReason.NoSuchKey);
            }
            return targetKey;
        } catch (RegKeyException) {
            throw;
        } catch (Exception ex) {
            throw new RegKeyException(RegKeyExceptionReason.NativeException, ex);
        }
    }
    private static bool TryGetHiveKey(RegistryHive hive, string? computerName, [NotNullWhen(true)] out RegistryKey? hiveKey) {
        hiveKey = null;
        try {
            hiveKey = computerName switch {
                string t when t is not null && 
                    !t.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) => 
                        RegistryKey.OpenRemoteBaseKey(hive, t),
                _ => RegistryKey.OpenBaseKey(hive, RegistryView.Registry64)
            };
            return true;
        } catch {
            return false;
        }
    }

    public RegKey Clone() {
        return new(_key.OpenSubKey("", true)!, computerName);
    }
    public bool Equals(RegKey? other) => Equals(other?._key);
    public bool Equals(RegistryKey? other) {
        return _key.ToString().Equals(other?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
    public override bool Equals(object? obj) {
        return Equals(obj as RegKey) || Equals(obj as RegistryKey);
    }
    public override int GetHashCode() {
        return _key.GetHashCode();
    }
    public override string ToString() {
        return _key.ToString();
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                _key.Dispose();
                if (_parent.IsValueCreated && parentOwned) {
                    _parent.Value?.Dispose();
                }
            }
            disposedValue = true;
        }
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
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
public sealed class RegKeyException(RegKeyExceptionReason reason, string? message, Exception? inner) : Exception(message, inner) {
    public RegKeyExceptionReason Reason { get; init; } = reason;
    public RegKeyException(RegKeyExceptionReason reason) : this(reason, null, null) { }
    public RegKeyException(RegKeyExceptionReason reason, string? message) : this(reason, message, null) { }
    public RegKeyException(RegKeyExceptionReason reason, Exception? inner) : this(reason, inner?.Message, inner) { }
    public string ReasonToString() {
        return Reason switch {
            RegKeyExceptionReason.NoResolvableHive => "Could not resolve hive from path.",
            RegKeyExceptionReason.KeyPathNotWellFormed => "Key path was not well-formed.",
            RegKeyExceptionReason.CouldNotOpenHive => "Could not open hive.",
            RegKeyExceptionReason.NullKey => "Attempted to open null key",
            RegKeyExceptionReason.NoSuchKey => "Key does not exist",
            RegKeyExceptionReason.NoParentKey => "Parent key does not exist (key is a hive).",
            RegKeyExceptionReason.NativeException => InnerException?.Message ?? "A native exception was thrown",
            _ => string.Empty,
        };
    }
}
public enum RegKeyExceptionReason {
    Unknown,
    NoResolvableHive,
    KeyPathNotWellFormed,
    CouldNotOpenHive,
    NullKey,
    NoSuchKey,
    NoParentKey,
    NativeException
}