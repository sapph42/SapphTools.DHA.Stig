using System.Text;

namespace UnitTests;
// Active tests for intended contracts that dfad869d violates. Do not "fix" expectations to match data loss.
[TestClass, TestCategory("Pure"), TestCategory("KnownRegression")]
public sealed class KnownRegressionTests {
    [DataTestMethod] [DataRow(RegistryValueKind.DWord)] [DataRow(RegistryValueKind.QWord)]
    [DataRow(RegistryValueKind.Binary)] [DataRow(RegistryValueKind.MultiString)] [DataRow(RegistryValueKind.None)]
    public void AbsentRegistryDataMustSurviveSerialization(RegistryValueKind kind) {
        RegistryValueValue source = TestData.Value(null, kind);
        string json = JsonSerializer.Serialize(source, TestData.Options());
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.AreEqual(JsonValueKind.Null, document.RootElement.GetProperty("Data").ValueKind);
        RegistryValueValue copy = JsonSerializer.Deserialize<RegistryValueValue>(json, TestData.Options())!;
        Assert.IsNull(copy.Data); Assert.AreEqual(kind, copy.Kind);
    }
    [DataTestMethod] [DataRow(RegistryValueKind.DWord)] [DataRow(RegistryValueKind.QWord)]
    [DataRow(RegistryValueKind.Binary)] [DataRow(RegistryValueKind.MultiString)]
    public void AbsentPatternDataMustSurviveSerialization(RegistryValueKind kind) {
        RegistryValuePatternValue source = new() { Target = "HKEY_USERS", Name = "v", Data = null, Kind = kind };
        string json = JsonSerializer.Serialize(source, TestData.Options());
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.AreEqual(JsonValueKind.Null, document.RootElement.GetProperty("Data").ValueKind);
        Assert.IsNull(JsonSerializer.Deserialize<RegistryValuePatternValue>(json, TestData.Options())!.Data);
    }
    [TestMethod] public void LogGroupingMustAcceptCaseVariantsOfTheSameHostAndRule() {
        RemediationAction first = TestData.Action();
        SapphTools.DHA.Stig.Remediator.Classes.Rollback.LogBatchCollection logs = new(first);
        RemediationAction next = TestData.Action(first.ToPreAction());
        next.ComputerName = next.ComputerName.ToUpperInvariant(); next.RuleId = next.RuleId.ToLowerInvariant(); next.ActionNumber++;
        logs.Add(next); Assert.AreEqual(2, logs.Flatten().Count());
    }
    [TestMethod] public void EqualRulesMustHaveEqualHashes() {
        Setting setting = new() { Data = TestData.Value(1) };
        Rule left = new() { RuleId = "V-123", Settings = [setting] }, right = new() { RuleId = "v-123", Settings = [setting] };
        Assert.IsTrue(left.Equals(right)); Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        Assert.AreEqual(1, new HashSet<Rule> { left, right }.Count);
    }
    [TestMethod] public void ByteFormattingMustHandleEmptyInput() => Assert.AreEqual("", ByteMagic.ToHexString([]));
    [TestMethod] public void ByteFormattingMustPreserveEveryByteAndSeparator() {
        byte[] bytes = [0, 1, 127, 128, 255];
        Assert.AreEqual("0x00,0x01,0x7F,0x80,0xFF", ByteMagic.ToHexString(bytes));
    }
    [DataTestMethod] [DataRow("a")] [DataRow("Hello")] [DataRow("日本語")] [DataRow("😀")]
    public void LsaUnicodeLengthsMustCountBytesAndPreserveFullUtf16Payload(string text) {
        using LsaUnicodeStringWrapper wrapper = new(text); bool acquired = false;
        LsaUnicodeString value = wrapper.DangerousGetStruct(ref acquired);
        try {
            byte[] expected = Encoding.Unicode.GetBytes(text);
            Assert.AreEqual((ushort)expected.Length, value.Length);
            Assert.AreEqual((ushort)(expected.Length + 2), value.MaximumLength);
            // Read only after validating bounds; no LSA calls or out-of-bounds memory reads.
            byte[] actual = new byte[value.Length]; System.Runtime.InteropServices.Marshal.Copy(value.Buffer, actual, 0, actual.Length);
            CollectionAssert.AreEqual(expected, actual);
        } finally { wrapper.DangerousReleaseStructBuffer(); }
    }
    [TestMethod] public void InvalidPatternMustBeRejectedInsteadOfBroadeningTargetSelection() {
        string json = """{"Target":"HKEY_USERS","Name":"v","Kind":"DWord","Data":1,"Overwrite":true,"TargetPattern":"["}""";
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<RegistryValuePatternValue>(json, TestData.Options()));
    }
    [TestMethod] public void BinaryCloneMustOwnItsMutableSnapshotData() {
        RegistryValueValue source = TestData.Value(new byte[] { 1, 2 }, RegistryValueKind.Binary); RegistryValueValue clone = source.Clone();
        ((byte[])clone.Data!)[0] = 9; Assert.AreEqual((byte)1, ((byte[])source.Data!)[0]);
    }
    [TestMethod] public void MultiStringCloneMustOwnItsMutableSnapshotData() {
        RegistryValueValue source = TestData.Value(new string[] { "original" }, RegistryValueKind.MultiString); RegistryValueValue clone = source.Clone();
        ((string[])clone.Data!)[0] = "changed"; Assert.AreEqual("original", ((string[])source.Data!)[0]);
    }
    [TestMethod] public void PatternCloneMustPreserveResolvedRollbackTarget() {
        RegistryValuePatternValue source = new() { Target = "HKEY_USERS", Name = "v", Data = 1,
            Kind = RegistryValueKind.DWord, ResolvedTarget = TestData.Value(1) };
        RegistryValuePatternValue clone = source.Clone();
        Assert.IsNotNull(clone.ResolvedTarget); Assert.AreNotSame(source.ResolvedTarget, clone.ResolvedTarget);
        Assert.AreEqual(source.ResolvedTarget.Target, clone.ResolvedTarget.Target);
    }
}
