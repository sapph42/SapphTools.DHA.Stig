namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class RegKeyPathTests {
    [DataTestMethod]
    [DataRow(@"HKCU:\Software\Test", RegistryHive.CurrentUser, @"Software\Test")]
    [DataRow(@"HKLM:\Software\Test", RegistryHive.LocalMachine, @"Software\Test")]
    [DataRow(@"HKCR:\Directory\shell", RegistryHive.ClassesRoot, @"Directory\shell")]
    [DataRow(@"HKU:\S-1-5-18", RegistryHive.Users, "S-1-5-18")]
    [DataRow(@"HKEY_CURRENT_USER\Software\Test", RegistryHive.CurrentUser, @"Software\Test")]
    [DataRow(@"HKEY_LOCAL_MACHINE\Software", RegistryHive.LocalMachine, "Software")]
    [DataRow(@"HKEY_CLASSES_ROOT\Directory", RegistryHive.ClassesRoot, "Directory")]
    [DataRow(@"HKEY_USERS\.DEFAULT", RegistryHive.Users, ".DEFAULT")]
    [DataRow(@"\\host\HKEY_LOCAL_MACHINE\Software", RegistryHive.LocalMachine, "Software")]
    [DataRow("HKEY_CURRENT_USER", RegistryHive.CurrentUser, "")]
    public void HiveParsingReturnsExpectedHiveAndRelativeRemainder(string path, RegistryHive hive, string remaining) {
        Assert.AreEqual(hive, RegKey.GetHive(path, out string actual)); Assert.AreEqual(remaining, actual);
    }
    [DataTestMethod] [DataRow("")] [DataRow(@"Software\Adobe")] [DataRow("not a registry path")]
    [DataRow(@"\\host")] [DataRow(@"HKEY_UNKNOWN\Test")]
    public void UnresolvableHiveReturnsNullAndConstructorPreservesReason(string path) {
        Assert.IsNull(RegKey.GetHive(path, out _));
        RegKeyException error = Assert.ThrowsException<RegKeyException>(() => new RegKey(path));
        Assert.AreEqual(RegKeyExceptionReason.NoResolvableHive, error.Reason);
    }
    [DataTestMethod] [DataRow("", true, null)] [DataRow("", false, null)]
    [DataRow(@"HKEY_CURRENT_USER\\Software\\Test\\", true, @"HKEY_CURRENT_USER\Software")]
    [DataRow(@"HKEY_CURRENT_USER\\Software\\Test\\", false, "Test")]
    [DataRow(@"\\host\HKEY_USERS\A\B", true, @"HKEY_USERS\A")]
    [DataRow(@" HKEY_CURRENT_USER \ Software \ Test ", false, "Test")]
    public void SplitPathHandlesEmptyRepeatedSeparatorsAndWhitespace(string path, bool parent, string? expected) {
        Assert.AreEqual(expected, RegKey.SplitPath(path, parent));
    }
    [DataTestMethod] [DataRow(RegKeyExceptionReason.NoResolvableHive)] [DataRow(RegKeyExceptionReason.NoSuchKey)]
    [DataRow(RegKeyExceptionReason.CouldNotOpenHive)] [DataRow(RegKeyExceptionReason.NoParentKey)]
    [DataRow(RegKeyExceptionReason.KeyPathNotWellFormed)] [DataRow(RegKeyExceptionReason.NullKey)]
    public void SpecificExceptionReasonsHaveUsefulDescriptions(RegKeyExceptionReason reason) {
        RegKeyException error = new(reason); Assert.AreEqual(reason, error.Reason); Assert.IsFalse(string.IsNullOrWhiteSpace(error.ReasonToString()));
    }
    [TestMethod] public void NativeExceptionPreservesInnerCauseAndMessage() {
        Exception cause = new UnauthorizedAccessException("access denied"); RegKeyException error = new(RegKeyExceptionReason.NativeException, cause);
        Assert.AreSame(cause, error.InnerException); Assert.AreEqual(cause.Message, error.Message); Assert.AreEqual(cause.Message, error.ReasonToString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(new RegKeyException(RegKeyExceptionReason.NativeException).ReasonToString()));
    }
}
