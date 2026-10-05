using System.Security.Principal;

namespace UnitTests.Fixtures;
/// <summary>Owns exactly one uniquely named leaf under HKCU\Software. No shared parent is deleted.</summary>
internal sealed class RegistrySandbox : IDisposable {
    private readonly string relativePath = @"Software\StigRemediator.UnitTests." + Guid.NewGuid().ToString("N");
    internal string FullPath => @"HKEY_CURRENT_USER\" + relativePath;
    internal RegKey Root { get; }
    internal RegistrySandbox() {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("HKCU integration tests require Windows.");
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        if (new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
            Assert.Inconclusive("Run HKCU tests unelevated to verify non-admin behavior.");
        using RegistryKey hive = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        Root = new RegKey(hive.CreateSubKey(relativePath, true));
    }
    public void Dispose() {
        try { Root.Dispose(); }
        finally {
            using RegistryKey hive = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            // Delete only this fixture's GUID leaf, never Software or another test's key.
            hive.DeleteSubKeyTree(relativePath, false);
        }
    }
}
