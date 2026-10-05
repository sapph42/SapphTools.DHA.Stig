using UnitTests.Fixtures;

namespace UnitTests;
[TestClass, TestCategory("WindowsHKCU"), TestCategory("KnownRegression")]
public sealed class RegKeyContractRegressionTests {
    private RegistrySandbox sandbox = null!;
    [TestInitialize] public void Initialize() => sandbox = new();
    [TestCleanup] public void Cleanup() => sandbox?.Dispose();
    [TestMethod] public void KeySnapshotMustUseFullParentPathSoTargetAndNameReconstructTheKey() {
        using RegKey child = sandbox.Root.CreateSubKey("child"); RegistryKeyValue value = child.GetKeyValue();
        Assert.AreEqual(child.FullName, value.Target + "\\" + value.Name);
    }
    [TestMethod] public void EqualWrappersMustHaveEqualHashesAndDeduplicateInHashSet() {
        using RegKey clone = sandbox.Root.Clone(); Assert.IsTrue(sandbox.Root.Equals(clone));
        Assert.AreEqual(sandbox.Root.GetHashCode(), clone.GetHashCode()); Assert.AreEqual(1, new HashSet<RegKey> { sandbox.Root, clone }.Count);
    }
    [TestMethod] public void AbsoluteDescendantOpenMustWorkWithCaseInsensitiveRegistryPaths() {
        using (sandbox.Root.CreateSubKey("child")) { }
        using RegKey child = sandbox.Root.OpenKey(sandbox.FullPath.ToLowerInvariant() + @"\child")!;
        Assert.IsNotNull(child); Assert.AreEqual("child", child.Name);
    }
}
