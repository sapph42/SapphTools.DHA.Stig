using Microsoft.Win32;

namespace SapphTools.DHA.Stig.Remediator.Extensions; 
public static class RegistryKeyExtensions {
    public static bool IsDescendentOf(this RegistryKey key, string candidate) {
        return key.Name.StartsWith(candidate + "\\", StringComparison.OrdinalIgnoreCase); 
    }
    public static bool IsAncestorOf(this RegistryKey key, string candidate) {
        return candidate.StartsWith(key.Name + "\\", StringComparison.OrdinalIgnoreCase);
    }
}
