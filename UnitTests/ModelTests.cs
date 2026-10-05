namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class ModelTests {
    [TestMethod] public void PreActionCopyAndRoundTripRetainEveryCorrelationField() {
        RemediationPreAction pre = TestData.Pre(); RemediationPreAction copy = new(pre);
        RemediationAction action = TestData.Action(pre); RemediationPreAction roundtrip = action.ToPreAction();
        TestData.SamePre(pre, copy); TestData.SamePre(pre, action); TestData.SamePre(pre, roundtrip);
        copy.ActionNumber++; roundtrip.ComputerName = "different";
        Assert.AreEqual(7, pre.ActionNumber); Assert.AreEqual("test-host", action.ComputerName);
    }
    [TestMethod] public void SettingCloneRetainsSafetyMetadataAndOwnsValue() {
        RegistryValueValue data = TestData.Value(1);
        Setting setting = new() { Order = 4, RequiredContext = SettingContext.System, Dangerous = true, Data = data };
        Setting clone = setting.Clone();
        Assert.AreEqual(4, clone.Order); Assert.AreEqual(SettingContext.System, clone.RequiredContext); Assert.IsTrue(clone.Dangerous);
        Assert.AreNotSame(setting.Data, clone.Data); ((RegistryValueValue)clone.Data).Name = "changed";
        Assert.AreEqual("Value", data.Name);
    }
    [DataTestMethod] [DataRow(-5, 0, -1)] [DataRow(0, 0, 0)] [DataRow(5, 0, 1)]
    [DataRow(int.MinValue, int.MaxValue, -1)] [DataRow(int.MaxValue, int.MinValue, 1)]
    public void SettingsCompareByOrderWithoutOverflow(int left, int right, int sign) {
        Setting a = new() { Order = left, Data = TestData.Value() }, b = new() { Order = right, Data = TestData.Value() };
        Assert.AreEqual(sign, Math.Sign(a.CompareTo(b))); Assert.IsTrue(a.CompareTo(null) > 0);
    }
    [TestMethod] public void SettingDefaultsRequireAdministratorAndAreNotDangerous() {
        Setting setting = new() { Data = TestData.Value() };
        Assert.AreEqual(SettingContext.Administrator, setting.RequiredContext); Assert.IsFalse(setting.Dangerous);
    }
    [TestMethod] public void CatalogCloneSeparatesNestedRuleSettingAndValueObjects() {
        Catalog source = new() { Name = "test", SchemaVersion = 2,
            Rules = [new Rule { RuleId = "V-123", Description = "desc", Settings = [new Setting { Order = 1, Data = TestData.Value(3) }] }],
            Blacklist = [new BlacklistedPlugin { RuleId = "V-456", Reason = "unsafe" }] };
        Catalog clone = source.Clone();
        Assert.AreEqual(source.Name, clone.Name); Assert.AreEqual(source.SchemaVersion, clone.SchemaVersion);
        Assert.AreNotSame(source.Rules, clone.Rules); Assert.AreNotSame(source.Blacklist, clone.Blacklist);
        Rule rule = clone.Rules.Single(); Assert.AreNotSame(source.Rules.Single(), rule);
        Assert.AreEqual("desc", rule.Description); Assert.AreNotSame(source.Rules.Single().Settings, rule.Settings);
        ((RegistryValueValue)rule.Settings.Single().Data).Data = 99;
        Assert.AreEqual(3, ((RegistryValueValue)source.Rules.Single().Settings.Single().Data).Data);
        clone.Rules.Clear(); clone.Blacklist.Clear(); Assert.AreEqual(1, source.Rules.Count); Assert.AreEqual(1, source.Blacklist.Count);
    }
    [TestMethod] public void RegistryValueCloneRetainsKindOverwriteAndTarget() {
        RegistryValueValue source = TestData.Value(@"%SystemRoot%\Foo", RegistryValueKind.ExpandString);
        RegistryValueValue clone = source.Clone();
        Assert.AreNotSame(source, clone); Assert.AreEqual(source.Kind, clone.Kind); Assert.AreEqual(source.Overwrite, clone.Overwrite);
        Assert.AreEqual(source.Target, clone.Target); Assert.AreEqual(source.Name, clone.Name); TestData.SameData(source.Data, clone.Data);
        Assert.AreEqual(source.Target + "\\" + source.Name, source.TargetString); Assert.AreEqual(source.TargetString, source.ToString());
    }
    [TestMethod] public void KeyCloneAndInterfaceCloneRetainRepresentation() {
        RegistryKeyValue source = new() { Target = @"HKEY_CURRENT_USER\Software", Name = "Test" };
        RegistryKeyValue clone = source.Clone(); IValue boxed = source; IValue interfaceClone = boxed.Clone();
        Assert.AreNotSame(source, clone); Assert.AreNotSame(source, interfaceClone);
        Assert.AreEqual(source.Name, clone.Name); Assert.AreEqual(source.Target, clone.Target); Assert.AreEqual(TargetType.RegistryKey, clone.Action);
    }
    [TestMethod] public void PatternTargetStringIncludesEachOptionalStageInOrder() {
        RegistryValuePatternValue pattern = new() { Target = "HKU", Name = "value", Data = 1,
            TargetPattern = new("^SID$"), SubPath = @"Software\App", PathPattern = new("^Version$") };
        Assert.AreEqual(@"HKU\^SID$\Software\App\^Version$\value", pattern.TargetString);
        RegistryValuePatternValue clone = pattern.Clone(); Assert.AreEqual(pattern.TargetString, clone.TargetString); Assert.AreNotSame(pattern, clone);
        pattern.TargetPattern = null; pattern.SubPath = null; pattern.PathPattern = null;
        Assert.AreEqual(@"HKU\value", pattern.TargetString);
    }
    [TestMethod] public void LockoutStructEqualityIncludesAllFields() {
        SamUserModalInfo3 value = new(60, 30, 5), equal = new(60, 30, 5);
        Assert.IsTrue(value == equal); Assert.IsFalse(value != equal); Assert.AreEqual(value.GetHashCode(), equal.GetHashCode());
        Assert.IsFalse(value.Equals(new(61, 30, 5))); Assert.IsFalse(value.Equals(new(60, 31, 5))); Assert.IsFalse(value.Equals(new(60, 30, 6)));
        Assert.IsFalse(value.Equals(null)); Assert.IsFalse(value.Equals("wrong type"));
        LockoutValue source = new() { SamStruct = value }; LockoutValue clone = source.Clone();
        clone.SamStruct = new(1, 2, 3); Assert.AreEqual(value, source.SamStruct);
    }
    [TestMethod] public void RuleEqualityChecksIdentityDescriptionAndSettings() {
        Setting setting = new() { Data = TestData.Value() };
        Rule a = new() { RuleId = "V-123", Description = "Example", Settings = [setting] };
        Rule b = new() { RuleId = "v-123", Description = "example", Settings = [setting] };
        Assert.IsTrue(a.Equals(a)); Assert.IsTrue(a.Equals(b)); Assert.IsTrue(b.Equals(a));
        Assert.IsFalse(a.Equals(null)); Assert.IsFalse(a.Equals(new object()));
        b.Description = null; Assert.IsFalse(a.Equals(b)); Assert.IsFalse(b.Equals(a));
        b.Description = "Example"; b.Settings = []; Assert.IsFalse(a.Equals(b));
    }
}
