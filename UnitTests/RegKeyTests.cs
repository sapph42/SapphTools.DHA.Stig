using SapphTools.DHA.Stig.Common.Classes;
using System.Diagnostics;

namespace UnitTests {
    [TestClass, TestCategory("Pure")]
    public sealed class RegKeyTests {
        readonly Dictionary<(string path, bool parent), string?> splitPathExpected = new() {
            // Standard local registry paths.
            [(@"HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion", true)] = 
                @"HKEY_CURRENT_USER\Microsoft\Windows",
            [(@"HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion", false)] =
                "CurrentVersion",
            [(@"HKEY_LOCAL_MACHINE\Software\Microsoft", true)] =
                @"HKEY_LOCAL_MACHINE\Software",
            [(@"HKEY_LOCAL_MACHINE\Software\Microsoft", false)] =
                "Microsoft",
            [(@"HKEY_CLASSES_ROOT\*\shell\open", true)] =
                @"HKEY_CLASSES_ROOT\*\shell",
            [(@"HKEY_CLASSES_ROOT\*\shell\open", false)] =
                "open",
            [(@"HKEY_USERS\.DEFAULT\Environment", true)] =
                @"HKEY_USERS\.DEFAULT",
            [(@"HKEY_USERS\.DEFAULT\Environment", false)] =
                "Environment",
            [(@"HKEY_CURRENT_CONFIG\System\CurrentControlSet", true)] =
                @"HKEY_CURRENT_CONFIG\System",
            [(@"HKEY_CURRENT_CONFIG\System\CurrentControlSet", false)] =
                "CurrentControlSet",
            // Hostname-prefixed registry paths.
            [(@"\\host\HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion", true)] =
                @"HKEY_CURRENT_USER\Microsoft\Windows",
            [(@"\\host\HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion", false)] =
                "CurrentVersion",
            [(@"\\server01\HKEY_LOCAL_MACHINE\Software", true)] =
                "HKEY_LOCAL_MACHINE",
            [(@"\\server01\HKEY_LOCAL_MACHINE\Software", false)] =
                "Software",
            [(@"\\computer.example.com\HKEY_USERS\S-1-5-18\Environment", true)] =
                @"HKEY_USERS\S-1-5-18",
            [(@"\\computer.example.com\HKEY_USERS\S-1-5-18\Environment", false)] =
                "Environment",
            [(@"\\remote-host\HKEY_CLASSES_ROOT\Directory\shell", true)] =
                @"HKEY_CLASSES_ROOT\Directory",
            [(@"\\remote-host\HKEY_CLASSES_ROOT\Directory\shell", false)] =
                "shell",
            // Hive-only paths.
            // parent == true has no parent and therefore returns null.
            // parent == false returns the hive itself.
            [(@"HKEY_CURRENT_USER", true)] =
                null,
            [(@"HKEY_CURRENT_USER", false)] =
                "HKEY_CURRENT_USER",
            [(@"HKEY_LOCAL_MACHINE", true)] =
                null,
            [(@"HKEY_LOCAL_MACHINE", false)] =
                "HKEY_LOCAL_MACHINE",
            [(@"HKEY_CLASSES_ROOT", true)] =
                null,
            [(@"HKEY_CLASSES_ROOT", false)] =
                "HKEY_CLASSES_ROOT",
            [(@"HKEY_USERS", true)] =
                null,
            [(@"HKEY_USERS", false)] =
                "HKEY_USERS",
            [(@"HKEY_CURRENT_CONFIG", true)] =
                null,
            [(@"HKEY_CURRENT_CONFIG", false)] =
                "HKEY_CURRENT_CONFIG",
            // Hostname-only paths have no registry hive and return null.
            [(@"\\host", true)] =
                null,
            [(@"\\host", false)] =
                null,
            [(@"\\server01", true)] =
                null,
            [(@"\\server01", false)] =
                null,
            // Paths with trailing separators.
            [(@"HKEY_CURRENT_USER\Software\", true)] =
                "HKEY_CURRENT_USER",
            [(@"HKEY_CURRENT_USER\Software\", false)] =
                "Software",
            [(@"\\host\HKEY_CURRENT_USER\Software\", true)] =
                "HKEY_CURRENT_USER",
            [(@"\\host\HKEY_CURRENT_USER\Software\", false)] =
                "Software",
            // Single-level paths below a hive.
            [(@"HKEY_CURRENT_USER\A", true)] =
                "HKEY_CURRENT_USER",
            [(@"HKEY_CURRENT_USER\A", false)] =
                "A",
            [(@"\\host\HKEY_CURRENT_USER\A", true)] =
                "HKEY_CURRENT_USER",
            [(@"\\host\HKEY_CURRENT_USER\A", false)] =
                "A",
            // Intermediate registry paths.
            [(@"HKEY_CURRENT_USER\Microsoft\Windows", true)] =
                @"HKEY_CURRENT_USER\Microsoft",
            [(@"HKEY_CURRENT_USER\Microsoft\Windows", false)] =
                "Windows",
            [(@"\\host\HKEY_CURRENT_USER\Microsoft\Windows", true)] =
                @"HKEY_CURRENT_USER\Microsoft",
            [(@"\\host\HKEY_CURRENT_USER\Microsoft\Windows", false)] =
                "Windows",
            // Deeper registry paths.
            [(@"HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion\Run", true)] =
                @"HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion",
            [(@"HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion\Run", false)] =
                "Run",
            [(@"\\host\HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion\Run", true)] =
                @"HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion",
            [(@"\\host\HKEY_CURRENT_USER\Microsoft\Windows\CurrentVersion\Run", false)] =
                "Run",
        };
        [TestMethod]
        public void SplitPath() {
            foreach((string path, bool parent) input in splitPathExpected.Keys) {
                try {
                    Assert.AreEqual(splitPathExpected[input], RegKey.SplitPath(input.path, input.parent));
                } catch {
                    Debug.WriteLine($"({input.path},{input.parent})=>{splitPathExpected[input]}");
                    throw;
                }
            }
        }
    }
}
