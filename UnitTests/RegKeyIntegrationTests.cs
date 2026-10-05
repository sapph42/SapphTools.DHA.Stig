using System.Text.RegularExpressions;
using UnitTests.Fixtures;

namespace UnitTests;
[TestClass, TestCategory("WindowsHKCU")]
public sealed class RegKeyIntegrationTests {
    private RegistrySandbox sandbox = null!;
    [TestInitialize] public void Initialize() => sandbox = new();
    [TestCleanup] public void Cleanup() => sandbox?.Dispose();
    public static IEnumerable<object[]> RegistryCases() => TestData.RegistryData();
    [DataTestMethod, DynamicData(nameof(RegistryCases), DynamicDataSourceType.Method)]
    public void SetAndGetPreserveActualKindAndRawData(RegistryValueKind kind, object data) {
        sandbox.Root.SetValue("value", data, kind);
        Assert.AreEqual(kind, sandbox.Root.GetValueKind("value"));
        TestData.SameData(data, sandbox.Root.GetValue("value"));
        TestData.SameData(data, sandbox.Root.GetValue("value", kind));
        using RegKey reopened = new(sandbox.FullPath);
        TestData.SameData(data, reopened.GetValue("value"));
    }
    [DataTestMethod] [DataRow(RegistryValueKind.DWord)] [DataRow(RegistryValueKind.QWord)]
    [DataRow(RegistryValueKind.String)] [DataRow(RegistryValueKind.ExpandString)]
    [DataRow(RegistryValueKind.Binary)] [DataRow(RegistryValueKind.MultiString)]
    public void MissingValueIsAbsentRatherThanAZeroOrEmptyPayload(RegistryValueKind kind) {
        Assert.AreEqual(RegistryValueKind.None, sandbox.Root.GetValueKind("missing"));
        Assert.IsNull(sandbox.Root.GetValue("missing")); Assert.IsNull(sandbox.Root.GetValue("missing", kind));
    }
    [TestMethod] public void WrongDesiredTypeDoesNotDestroyAgnosticSnapshot() {
        sandbox.Root.SetValue("value", "1", RegistryValueKind.String);
        Assert.IsNull(sandbox.Root.GetValue("value", RegistryValueKind.DWord));
        Assert.AreEqual("1", sandbox.Root.GetValue("value")); Assert.AreEqual(RegistryValueKind.String, sandbox.Root.GetValueKind("value"));
        sandbox.Root.SetValue("value", 1, RegistryValueKind.DWord);
        Assert.IsNull(sandbox.Root.GetValue("value", RegistryValueKind.QWord)); Assert.AreEqual(1, sandbox.Root.GetValue("value"));
    }
    [TestMethod] public void ExpandStringRemainsUnexpandedInSnapshotAndRestore() {
        const string original = @"%SystemRoot%\UnitTests";
        sandbox.Root.SetValue("value", original, RegistryValueKind.ExpandString);
        object before = sandbox.Root.GetValue("value")!;
        sandbox.Root.SetValue("value", "replacement", RegistryValueKind.String);
        sandbox.Root.SetValue("value", before, RegistryValueKind.ExpandString);
        Assert.AreEqual(original, sandbox.Root.GetValue("value"));
    }
    [TestMethod] public void DefaultValueIsAbsentUntilExplicitlyCreatedAndCanBeDeleted() {
        Assert.IsNull(sandbox.Root.GetValue(null)); Assert.AreEqual(RegistryValueKind.None, sandbox.Root.GetValueKind(""));
        sandbox.Root.SetValue(null, "default data", RegistryValueKind.String);
        Assert.AreEqual("default data", sandbox.Root.GetValue("")); Assert.AreEqual(1, sandbox.Root.ValueCount);
        sandbox.Root.DeleteValue("", true); Assert.IsNull(sandbox.Root.GetValue(null)); Assert.AreEqual(0, sandbox.Root.ValueCount);
    }
    [TestMethod] public void ValueNamesAreCaseInsensitiveAndDeletionHonorsMissingFlag() {
        sandbox.Root.SetValue("MiXeD", 0, RegistryValueKind.DWord); Assert.AreEqual(0, sandbox.Root.GetValue("mixed"));
        sandbox.Root.DeleteValue("MIXED", true); sandbox.Root.DeleteValue("MIXED", false);
        Assert.ThrowsException<ArgumentException>(() => sandbox.Root.DeleteValue("missing", true));
    }
    [TestMethod] public void ChildParentIsBorrowedAndChildDisposalLeavesParentUsable() {
        using RegKey child = sandbox.Root.CreateSubKey("child");
        Assert.AreSame(sandbox.Root, child.Parent); Assert.AreEqual("child", child.Name);
        Assert.AreEqual(sandbox.FullPath + @"\child", child.FullName);
        child.Dispose(); child.Dispose(); sandbox.Root.SetValue("alive", 1, RegistryValueKind.DWord);
        Assert.AreEqual(1, sandbox.Root.GetValue("alive"));
    }
    [TestMethod] public void LazyParentOfReopenedChildHasTheFullParentPath() {
        using (RegKey created = sandbox.Root.CreateSubKey("child")) { }
        using RegKey child = new(sandbox.FullPath + @"\child");
        Assert.AreEqual(sandbox.FullPath, child.Parent!.FullName); Assert.AreSame(child.Parent, child.Parent);
        child.Dispose(); Assert.AreEqual(1, sandbox.Root.SubKeyCount);
    }
    [TestMethod] public void CloneOwnsAnIndependentHandleAndSurvivesOriginalDisposal() {
        RegKey original = sandbox.Root.CreateSubKey("child"); using RegKey clone = original.Clone();
        original.Dispose(); clone.SetValue("alive", 2, RegistryValueKind.DWord); Assert.AreEqual(2, clone.GetValue("alive"));
    }
    [TestMethod] public void OpenMissingSubKeyReturnsNullWithoutCreatingIt() {
        Assert.IsNull(sandbox.Root.OpenSubKey("missing")); Assert.AreEqual(0, sandbox.Root.SubKeyCount);
    }
    [TestMethod] public void ConstructorPreservesSpecificNoSuchKeyReason() {
        RegKeyException error = Assert.ThrowsException<RegKeyException>(() => new RegKey(sandbox.FullPath + @"\missing"));
        Assert.AreEqual(RegKeyExceptionReason.NoSuchKey, error.Reason);
    }
    [TestMethod] public void ClosestAncestorReturnsActualExistingPathWithoutCreatingMissingBranches() {
        using RegKey ancestor = RegKey.ClosestExtantAncestor(sandbox.FullPath + @"\missing\deeper", null)!;
        Assert.AreEqual(sandbox.FullPath, ancestor.FullName); Assert.AreEqual(0, sandbox.Root.SubKeyCount);
    }
    [TestMethod] public void NonRecursiveCreateCreatesExactlyOneMissingLevelAndBorrowsParent() {
        using RegKey child = RegKey.CreateKey(sandbox.Root, sandbox.FullPath + @"\one\two", false, false);
        Assert.AreEqual(sandbox.FullPath + @"\one", child.FullName); Assert.IsNull(child.OpenSubKey("two"));
        child.Dispose(); sandbox.Root.SetValue("alive", 1, RegistryValueKind.DWord);
    }
    [TestMethod] public void RecursiveCreateReachesRequestedLeaf() {
        RegKey leaf = RegKey.CreateKey(sandbox.Root, sandbox.FullPath + @"\one\two\three", true, false);
        try {
            Assert.AreEqual(sandbox.FullPath + @"\one\two\three", leaf.FullName);
            leaf.SetValue("value", 1, RegistryValueKind.DWord); Assert.AreEqual(1, leaf.GetValue("value"));
        } finally {
            // Recursive creation borrows parents. Close every generated wrapper up to our fixture root.
            RegKey? current = leaf;
            while (current is not null && !ReferenceEquals(current, sandbox.Root)) {
                RegKey? parent = current.Parent; current.Dispose(); current = parent;
            }
        }
    }
    [TestMethod] public void CreateExistingPathReturnsSameObjectWithoutDisposingIt() {
        RegKey existing = RegKey.CreateKey(sandbox.Root, sandbox.FullPath, false, false);
        Assert.AreSame(sandbox.Root, existing); existing.SetValue("alive", 1, RegistryValueKind.DWord);
    }
    [TestMethod] public void EnumerationMatchesLeafNamesAndLeavesNonmatchingKeysUntouched() {
        foreach (string name in new[] { "SID-1", "SID-2", ".DEFAULT" }) using (sandbox.Root.CreateSubKey(name)) { }
        List<RegKey> owned = [];
        try {
            foreach (RegKey key in sandbox.Root.GetSubKeys(new Regex("^SID-"))) owned.Add(key);
            CollectionAssert.AreEquivalent(new[] { "SID-1", "SID-2" }, owned.Select(k => k.Name).ToArray());
        } finally { foreach (RegKey key in owned) key.Dispose(); }
        Assert.AreEqual(3, sandbox.Root.SubKeyCount);
    }
    [TestMethod] public void DisposedEnumerationReportsCausalWrapperException() {
        RegKey child = sandbox.Root.CreateSubKey("child"); child.Dispose();
        RegKeyException error = Assert.ThrowsException<RegKeyException>(() => child.GetSubKeys().ToList());
        Assert.AreEqual(RegKeyExceptionReason.NativeException, error.Reason); Assert.IsInstanceOfType(error.InnerException, typeof(ObjectDisposedException));
    }
    [TestMethod] public void DeleteLeafDeletesOnlyThatLeafAndOptionallyDisposesIt() {
        RegKey child = sandbox.Root.CreateSubKey("child"); child.Delete(true); child.Dispose();
        Assert.IsNull(sandbox.Root.OpenSubKey("child")); Assert.AreEqual(0, sandbox.Root.SubKeyCount);
        sandbox.Root.SetValue("alive", 1, RegistryValueKind.DWord);
    }
    [TestMethod] public void DeleteRefusesToRemoveAKeyContainingSubkeys() {
        using RegKey child = sandbox.Root.CreateSubKey("child"); using RegKey grandchild = child.CreateSubKey("grandchild");
        RegKeyException error = Assert.ThrowsException<RegKeyException>(() => child.Delete(false));
        Assert.AreEqual(RegKeyExceptionReason.NativeException, error.Reason);
        using RegKey reopened = sandbox.Root.OpenSubKey(@"child\grandchild")!; Assert.IsNotNull(reopened);
    }
    [TestMethod] public void ParentOwnershipTransferDisposesOwnedParentOnly() {
        RegKey parent = new(sandbox.FullPath);
        using RegKey child = RegKey.CreateKey(parent, sandbox.FullPath + @"\child", false, true);
        child.Dispose(); Assert.ThrowsException<ObjectDisposedException>(() => parent.GetValue("v"));
        sandbox.Root.SetValue("independent", 1, RegistryValueKind.DWord);
    }
    [TestMethod] public void LocalMachineNameUsesLocalHandleWithoutRemoteRegistryService() {
        using RegKey local = new(sandbox.FullPath, Environment.MachineName.ToUpperInvariant());
        local.SetValue("local", 9, RegistryValueKind.DWord); Assert.AreEqual(9, sandbox.Root.GetValue("local"));
    }
}
