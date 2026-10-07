using SapphTools.SecurityDescriptor.Classes;

namespace UnitTests;

[TestClass, TestCategory("Pure")]
public sealed class ValueContractTests {
    public static IEnumerable<object[]> Values() {
        yield return ["registry", TestData.Value(7)];
        yield return ["key", new RegistryKeyValue { Target = "HKCU", Name = "Test" }];
        yield return ["pattern", new RegistryValuePatternValue { Target = "HKCU", Name = "Test", Data = 7, Kind = RegistryValueKind.DWord }];
        yield return ["rights", new SeRightsValue { Target = LsaPrivilege.SE_NETWORK_LOGON, AccountNames = [] }];
        yield return ["sam", new LockoutValue { SamStruct = new(60, 30, 5) }];
        yield return ["certificate", new CertificatesValue { Target = "Root", Artifact = Artifact() }];
    }
    private static IArtifact Artifact() => new ArtifactCatalog { RelativePath = "test.sst", Hash = "ABCD", HashAlgorithm = "SHA256" };

    [DataTestMethod, DynamicData(nameof(Values), DynamicDataSourceType.Method)]
    public void ValueInterfaceEqualityIsReflexiveAndCloneEqual(string name, IValue value) {
        IValue clone = value.Clone();
        Assert.AreNotSame(value, clone, name);
        Assert.IsTrue(value.Equals(value), name);
        Assert.IsTrue(value.Equals(clone), name);
        Assert.IsTrue(clone.Equals(value), name);
        Assert.IsFalse(value.Equals((IValue?)null), name);
        Assert.AreEqual(value.Action, clone.Action);
        Assert.AreEqual(value.TargetString, clone.ToString());
    }

    [TestMethod]
    public void ArtifactDefaultEqualityComparesAllFieldsWithoutReadingFiles() {
        IArtifact first = Artifact(), second = Artifact();
        second.RelativePath = "TEST.SST"; second.Hash = "abcd"; ((ArtifactCatalog)second).HashAlgorithm = "sha256";
        Assert.IsTrue(first.Equals(second)); Assert.IsTrue(second.Equals(first));
        Assert.IsTrue(first.Equals(first.Clone())); Assert.IsFalse(first.Equals(null));
        second.RelativePath = "different.sst"; Assert.IsFalse(first.Equals(second));
        second = Artifact(); second.Hash = "ABCE"; Assert.IsFalse(first.Equals(second));
        second = Artifact(); ((ArtifactCatalog)second).HashAlgorithm = "SHA512"; Assert.IsFalse(first.Equals(second));
    }

    [DataTestMethod, DataRow("store"), DataRow("path"), DataRow("hash"), DataRow("algorithm")]
    public void CertificateEqualityDetectsBehaviorChangingFields(string field) {
        CertificatesValue first = new() { Target = "Root", Artifact = Artifact() };
        CertificatesValue second = first.Clone();
        switch (field) {
            case "store": second.Target = "Disallowed"; break;
            case "path": second.Artifact.RelativePath = "other.sst"; break;
            case "hash": second.Artifact.Hash = "ABCE"; break;
            case "algorithm": ((ArtifactCatalog)second.Artifact).HashAlgorithm = "SHA512"; break;
        }
        Assert.IsFalse(first.Equals(second)); Assert.IsFalse(second.Equals(first));
        Assert.IsFalse(first.Equals(TestData.Value(7)));
    }

    [DataTestMethod, DataRow(61, 30, 5), DataRow(60, 31, 5), DataRow(60, 30, 6)]
    public void LockoutEqualityIncludesEachPolicyField(int duration, int window, int threshold) {
        IValue first = new LockoutValue { SamStruct = new(60, 30, 5) };
        IValue second = new LockoutValue { SamStruct = new(duration, window, threshold) };
        Assert.IsFalse(first.Equals(second)); Assert.IsFalse(second.Equals(first));
        Assert.IsFalse(first.Equals(TestData.Value(7)));
    }

    [TestMethod]
    public void EmptyRightsEqualityIncludesPrivilegeAndType() {
        SeRightsValue first = new() { Target = LsaPrivilege.SE_NETWORK_LOGON, AccountNames = [] };
        SeRightsValue second = new() { Target = LsaPrivilege.SE_DENY_NETWORK_LOGON, AccountNames = [] };
        Assert.IsFalse(first.Equals(second)); Assert.IsFalse(first.Equals(TestData.Value(7)));
    }

    [TestMethod]
    public void SettingOrderIdentityDeduplicatesBothCollectionTypes() {
        Setting first = new() { Order = 1, Data = TestData.Value(1) };
        Setting second = new() { Order = 1, Data = TestData.Value(2), Dangerous = true };
        Assert.IsTrue(first.Equals(second)); Assert.IsTrue(first.Equals((object)second));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode()); Assert.IsFalse(first.DeepEquals(second));
        Assert.AreEqual(1, new HashSet<Setting> { first, second }.Count);
        Assert.AreEqual(1, new SettingsSet([first, second]).Count);
        Assert.IsFalse(first.Equals((Setting?)null)); Assert.IsFalse(first.Equals(new object()));
    }

    [DataTestMethod, DataRow("context"), DataRow("dangerous"), DataRow("order")]
    public void DeepSettingEqualityIncludesSafetyMetadata(string field) {
        Setting first = new() { Order = 1, Data = TestData.Value(1) }, second = first.Clone();
        switch (field) {
            case "context": second.RequiredContext = SettingContext.System; break;
            case "dangerous": second.Dangerous = true; break;
            case "order": second.Order++; break;
        }
        Assert.IsFalse(first.DeepEquals(second)); Assert.IsFalse(second.DeepEquals(first));
        Assert.IsFalse(first.DeepEquals(null));
    }

    [TestMethod]
    public void SettingsSetRejectsDuplicateOrdersAndEnumeratesInputOnce() {
        Setting first = new() { Order = 1, Data = TestData.Value(1) };
        SettingsSet set = new([first]);
        Assert.IsFalse(set.SetEquals([first, first.Clone()]));
        int enumerations = 0;
        IEnumerable<Setting> Once() {
            Assert.AreEqual(1, ++enumerations);
            yield return first.Clone();
        }
        Assert.IsTrue(set.SetEquals(Once()));
        Assert.IsFalse(set.SetEquals([new Setting { Order = 2, Data = TestData.Value(1) }]));
        Assert.IsFalse(set.SetEquals([]));
        Assert.IsTrue(new SettingsSet().SetEquals([]));
    }

    public static IEnumerable<object[]> RegistryCases() => TestData.RegistryData();
    [DataTestMethod, DynamicData(nameof(RegistryCases), DynamicDataSourceType.Method)]
    public void PatternCloneRetainsDataAndSeparatesMutablePayloads(RegistryValueKind kind, object data) {
        RegistryValuePatternValue source = new() {
            Target = "HKCU", Name = "Test", Data = data, Kind = kind, Overwrite = true,
            ResolvedTarget = TestData.Value(1)
        };
        IValue clone = ((IValue)source).Clone();
        Assert.IsTrue(source.Equals(clone));
        RegistryValuePatternValue typed = (RegistryValuePatternValue)clone;
        Assert.AreNotSame(source.ResolvedTarget, typed.ResolvedTarget);
        if (data is byte[] or string[]) Assert.AreNotSame(data, typed.Data);
        typed.ResolvedTarget!.Data = 2;
        Assert.AreEqual(1, source.ResolvedTarget!.Data); Assert.IsFalse(source.Equals(typed));
    }
}
