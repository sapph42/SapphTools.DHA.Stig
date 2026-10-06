using System.Text.RegularExpressions;

namespace UnitTests;

[TestClass, TestCategory("Pure")]
public sealed class EqualityRegressionTests {
    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void OneMissingResolvedTargetComparesUnequalInBothDirections(bool resolveFirst) {
        RegistryValuePatternValue first = new() {
            Target = "HKCU", Name = "Test", Data = 1, Kind = RegistryValueKind.DWord
        };
        RegistryValuePatternValue second = first.Clone();
        (resolveFirst ? first : second).ResolvedTarget = TestData.Value(1);
        Assert.IsFalse(first.Equals(second));
        Assert.IsFalse(second.Equals(first));
    }

    [TestMethod]
    public void DifferentResolvedTargetsShouldNotCompareEqual() {
        RegistryValuePatternValue value = new() { Target = "HKCU", Name = "Test", Data = 1, Kind = RegistryValueKind.DWord, ResolvedTarget = TestData.Value(1) };
        var changed = value.Clone();
        changed.ResolvedTarget!.Data = 2;
        Assert.IsFalse(value.Equals(changed));
    }
    [TestMethod]
    public void ResolvedPatternCloneShouldCompareEqual() {
        RegistryValuePatternValue value = new() { Target = "HKCU", Name = "Test", Data = 1, Kind = RegistryValueKind.DWord, ResolvedTarget = TestData.Value(1) };
        Assert.IsTrue(value.Equals(value));
        Assert.IsTrue(value.Equals(value.Clone()));
    }

    [TestMethod]
    public void PatternDelimiterCannotCollideWithFieldContents() {
        RegistryValuePatternValue first = new() { Target = "HKCU", Name = "Test", Data = 1, Kind = RegistryValueKind.DWord, TargetPattern = new Regex(@"a|\b") };
        var second = first.Clone();
        second.TargetPattern = new Regex("a");
        second.SubPath = "b|";
        Assert.IsFalse(first.Equals(second));
    }
    [TestMethod]
    public void RegistryEqualityIncludesOverwriteAndAction() {
        RegistryValueValue normal = TestData.Value(1);
        RegistryValueValue differentOverwrite = normal.Clone();
        differentOverwrite.Overwrite = !normal.Overwrite;
        Assert.IsFalse(normal.Equals(differentOverwrite));
        RegistryValuePatternValue pattern = new() { Target = normal.Target, Name = normal.Name, Data = normal.Data, Kind = normal.Kind, Overwrite = normal.Overwrite };
        Assert.IsFalse(normal.Equals(pattern));
        Assert.IsFalse(pattern.Equals(normal));
        Assert.IsTrue(pattern.Equals(pattern.Clone()));
    }

    [TestMethod]
    public void RegistryArraysAndNullCompareByContent() {
        foreach (var item in new (RegistryValueKind Kind, object? Data)[] {
            (RegistryValueKind.Binary, new byte[] { 0, 128, 255 }),
            (RegistryValueKind.MultiString, new string[] { "first", "second" }),
            (RegistryValueKind.String, "test"),
            (RegistryValueKind.QWord, long.MaxValue),
            (RegistryValueKind.None, null)
        }) {
            RegistryValueValue value = new() { Target = "HKCU\\Test", Name = "Test", Data = item.Data, Kind = item.Kind };
            Assert.IsTrue(value.Equals(value.Clone()), item.Kind.ToString());
            var changed = value.Clone();
            changed.Kind = RegistryValueKind.Unknown;
            Assert.IsFalse(value.Equals(changed), item.Kind.ToString());
        }
    }

    [TestMethod]
    public void PatternStagesRemainDistinct() {
        RegistryValuePatternValue first = new() { Target = "HKCU", Name = "Test", Data = 1, Kind = RegistryValueKind.DWord, TargetPattern = new Regex("User") };
        var second = first.Clone();
        second.PathPattern = second.TargetPattern;
        second.TargetPattern = null;
        Assert.IsFalse(first.Equals(second));
    }
    [TestMethod]
    public void RegistryDataShouldEqualItself() {
        RegistryValueValue value = TestData.Value(1);
        Assert.IsTrue(value.DataEquals(value));
    }

    [TestMethod]
    public void BaseInterfaceShouldDispatchContentEquality() {
        IValue value = new RegistryKeyValue { Target = "HKCU\\Software\\Test", Name = "Child" };
        Assert.IsTrue(value.Equals(value.Clone()));
    }

    [TestMethod]
    public void TypedRegistryKeyEqualityShouldWork() {
        RegistryKeyValue value = new() { Target = "HKCU\\Software\\Test", Name = "Child" };
        Assert.IsTrue(value.Equals(value.Clone()));
    }

    [TestMethod]
    public void EqualRuleSetsMustHaveEqualHashesRegardlessOfInsertionOrder() {
        Setting first = new() { Order = 1, Data = TestData.Value(1) };
        Setting second = new() { Order = 2, Data = TestData.Value(2) };
        Rule left = new() { RuleId = "V-123", Settings = [first, second] };
        Rule right = new() { RuleId = "V-123", Settings = [second, first] };
        Assert.IsTrue(left.Equals(right));
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
    }

    [TestMethod]
    public void DifferentSettingDataShouldNotMakeWholeRulesEqual() {
        Rule left = new() { RuleId = "V-123", Settings = [new Setting { Order = 1, Data = TestData.Value(0) }] };
        Rule right = new() { RuleId = "V-123", Settings = [new Setting { Order = 1, Data = TestData.Value(1) }] };
        Assert.IsFalse(left.Equals(right));
    }

    [TestMethod]
    public void RuleCloneShouldRemainContentEqual() {
        Rule rule = new() { RuleId = "V-123", Settings = [new Setting { Order = 1, Data = TestData.Value(1) }] };
        Assert.IsTrue(rule.Equals(rule.Clone()));
    }

    [TestMethod]
    public void SeparatelyConstructedIdenticalSettingDataShouldBeDeepEqual() {
        Setting left = new() { Order = 1, Data = TestData.Value(1) };
        Setting right = new() { Order = 1, Data = TestData.Value(1) };
        Assert.IsTrue(left.DeepEquals(right));
    }

}
