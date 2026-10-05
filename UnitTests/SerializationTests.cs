using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class SerializationTests {
    public static IEnumerable<object[]> RegistryCases() => TestData.RegistryData();
    [DataTestMethod, DynamicData(nameof(RegistryCases), DynamicDataSourceType.Method)]
    public void RegistryValueRoundTripsEverySupportedKindAndBoundary(RegistryValueKind kind, object data) {
        RegistryValueValue value = TestData.Value(data, kind);
        RegistryValueValue copy = JsonSerializer.Deserialize<RegistryValueValue>(JsonSerializer.Serialize(value, TestData.Options()), TestData.Options())!;
        Assert.AreEqual(value.Target, copy.Target); Assert.AreEqual(value.Name, copy.Name); Assert.AreEqual(kind, copy.Kind);
        Assert.AreEqual(value.Overwrite, copy.Overwrite); TestData.SameData(data, copy.Data);
    }
    [DataTestMethod, DynamicData(nameof(RegistryCases), DynamicDataSourceType.Method)]
    public void PatternRoundTripsDataRegexAndResolvedTarget(RegistryValueKind kind, object data) {
        RegistryValuePatternValue value = new() { Target = "HKEY_USERS", Name = "value", Data = data, Kind = kind,
            TargetPattern = new(@"^S-1-5-21-\d+$"), PathPattern = new("^Version"), SubPath = @"Software\App",
            Overwrite = true, ResolvedTarget = TestData.Value(12) };
        RegistryValuePatternValue copy = JsonSerializer.Deserialize<RegistryValuePatternValue>(JsonSerializer.Serialize(value, TestData.Options()), TestData.Options())!;
        Assert.AreEqual(value.TargetString, copy.TargetString); Assert.AreEqual(kind, copy.Kind); Assert.IsTrue(copy.Overwrite);
        Assert.IsTrue(copy.TargetPattern!.IsMatch("S-1-5-21-123")); Assert.IsFalse(copy.TargetPattern.IsMatch(".DEFAULT"));
        Assert.AreEqual("^Version", copy.PathPattern!.ToString()); Assert.AreEqual(@"Software\App", copy.SubPath);
        TestData.SameData(data, copy.Data); Assert.AreEqual(12, copy.ResolvedTarget!.Data);
    }
    [DataTestMethod] [DataRow("DWord")] [DataRow("QWord")] [DataRow("String")]
    [DataRow("ExpandString")] [DataRow("Binary")] [DataRow("MultiString")]
    public void ReadingExplicitNullPreservesAbsence(string kind) {
        string json = $$"""{"Action":"RegistryValue","Target":"HKCU","Name":"","Kind":"{{kind}}","Data":null,"Overwrite":false}""";
        RegistryValueValue value = JsonSerializer.Deserialize<RegistryValueValue>(json, TestData.Options())!;
        Assert.IsNull(value.Data); Assert.AreEqual("", value.Name); Assert.IsFalse(value.Overwrite);
    }
    [TestMethod] public void ReadingOmittedDataPreservesAbsenceAndAcceptsAnyPropertyOrder() {
        RegistryValueValue value = JsonSerializer.Deserialize<RegistryValueValue>(
            """{"Kind":"DWord","Overwrite":false,"Name":"v","Target":"HKCU","Action":"RegistryValue"}""", TestData.Options())!;
        Assert.IsNull(value.Data); Assert.AreEqual(RegistryValueKind.DWord, value.Kind);
    }
    [TestMethod] public void UnknownNestedFieldsAreSkippedWithoutEatingFollowingProperties() {
        RegistryValueValue value = JsonSerializer.Deserialize<RegistryValueValue>(
            """{"Future":{"nested":[1,{"x":true}]},"Target":"HKCU","Name":"v","Kind":"DWord","Data":4,"Overwrite":true}""", TestData.Options())!;
        Assert.AreEqual(4, value.Data); Assert.IsTrue(value.Overwrite);
    }
    [DataTestMethod] [DataRow("Target")] [DataRow("Name")] [DataRow("Kind")] [DataRow("Overwrite")]
    public void IncompleteRegistryValuesCannotBecomeUsableActions(string property) {
        JsonObject obj = JsonNode.Parse(JsonSerializer.Serialize(TestData.Value(1), TestData.Options()))!.AsObject(); obj.Remove(property);
        Assert.IsNull(JsonSerializer.Deserialize<RegistryValueValue>(obj.ToJsonString(), TestData.Options()));
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<IValue>(obj.ToJsonString(), TestData.Options()));
    }
    [TestMethod] public void WrongActionDiscriminatorIsRejected() {
        string json = """{"Action":"SeRight","Target":"HKCU","Name":"v","Kind":"DWord","Data":4,"Overwrite":true}""";
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<RegistryValueValue>(json, TestData.Options()));
    }
    [DataTestMethod] [DataRow("{}")] [DataRow("{\"Action\":\"Invented\"}")]
    [DataRow("{\"Action\":\"RegistryKey\"}")] [DataRow("{\"Action\":\"CertStore\"}")]
    public void InterfaceReaderRejectsMissingInvalidOrUnsupportedAction(string json) {
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<IValue>(json, TestData.Options()));
    }
    [TestMethod] public void InterfaceRoundTripSelectsRegistryAndPatternConcreteTypes() {
        IValue value = TestData.Value(5);
        Assert.IsInstanceOfType(JsonSerializer.Deserialize<IValue>(JsonSerializer.Serialize(value, TestData.Options()), TestData.Options()), typeof(RegistryValueValue));
        value = new RegistryValuePatternValue { Target = "HKEY_USERS", Name = "v", Data = 5, Kind = RegistryValueKind.DWord };
        Assert.IsInstanceOfType(JsonSerializer.Deserialize<IValue>(JsonSerializer.Serialize(value, TestData.Options()), TestData.Options()), typeof(RegistryValuePatternValue));
    }
    [TestMethod] public void NumericOverflowIsRejectedRatherThanTruncated() {
        string json = """{"Target":"HKCU","Name":"v","Kind":"DWord","Data":2147483648,"Overwrite":true}""";
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<RegistryValueValue>(json, TestData.Options()));
    }
    [TestMethod] public void BinaryWireRepresentationIsBase64() {
        string json = JsonSerializer.Serialize(TestData.Value(new byte[] { 0, 1, 255 }, RegistryValueKind.Binary), TestData.Options());
        using JsonDocument doc = JsonDocument.Parse(json); Assert.AreEqual("AAH/", doc.RootElement.GetProperty("Data").GetString());
    }
    [TestMethod] public void SettingDefaultsSupportOlderCatalogEntries() {
        string json = """{"Order":2,"Future":[{}],"Data":{"Action":"RegistryValue","Target":"HKCU","Name":"v","Kind":"DWord","Data":4,"Overwrite":true}}""";
        Setting setting = JsonSerializer.Deserialize<Setting>(json, TestData.Options())!;
        Assert.AreEqual(2, setting.Order); Assert.AreEqual(SettingContext.Administrator, setting.RequiredContext); Assert.IsFalse(setting.Dangerous);
    }
    [DataTestMethod] [DataRow(SettingContext.None)] [DataRow(SettingContext.Administrator)]
    [DataRow(SettingContext.System)] [DataRow(SettingContext.TrustedInstaller)]
    public void SettingRoundTripRetainsContextAndDangerFlag(SettingContext context) {
        Setting value = new() { Order = 12, Data = TestData.Value(4), RequiredContext = context, Dangerous = true };
        Setting copy = JsonSerializer.Deserialize<Setting>(JsonSerializer.Serialize(value, TestData.Options()), TestData.Options())!;
        Assert.AreEqual(context, copy.RequiredContext); Assert.IsTrue(copy.Dangerous); Assert.AreEqual(12, copy.Order);
        Assert.AreEqual(4, ((RegistryValueValue)copy.Data).Data);
    }
    [DataTestMethod] [DataRow("{}")] [DataRow("{\"Order\":1}")] [DataRow("{\"Order\":-1,\"Data\":null}")]
    public void IncompleteSettingsAreRejected(string json) {
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<Setting>(json, TestData.Options()));
    }
    [TestMethod] public void CatalogRoundTripRetainsBlacklistAndNestedSafetyMetadata() {
        Catalog source = new() { Name = "test", SchemaVersion = 1,
            Blacklist = [new BlacklistedPlugin { RuleId = "V-2", Reason = "dangerous" }],
            Rules = [new Rule { RuleId = "V-1", Description = "description", Settings = [new Setting {
                Order = 3, RequiredContext = SettingContext.System, Dangerous = true, Data = TestData.Value(1) }] }] };
        Catalog copy = JsonSerializer.Deserialize<Catalog>(JsonSerializer.Serialize(source, TestData.Options()), TestData.Options())!;
        Assert.AreEqual(source.Name, copy.Name); Assert.AreEqual(1, copy.SchemaVersion);
        Assert.AreEqual("V-2", copy.Blacklist.Single().RuleId); Assert.AreEqual("dangerous", copy.Blacklist.Single().Reason);
        Rule rule = copy.Rules.Single(); Assert.AreEqual("V-1", rule.RuleId); Assert.AreEqual("description", rule.Description);
        Setting setting = rule.Settings.Single(); Assert.IsTrue(setting.Dangerous); Assert.AreEqual(SettingContext.System, setting.RequiredContext);
    }
    [TestMethod] public void MinimalCatalogDefaultsCollectionsAndSkipsUnknownNestedMetadata() {
        Catalog value = JsonSerializer.Deserialize<Catalog>("""{"Future":{"x":[1]},"Name":"minimal","SchemaVersion":1}""", TestData.Options())!;
        Assert.AreEqual(0, value.Rules.Count); Assert.AreEqual(0, value.Blacklist.Count);
    }
    [DataTestMethod] [DataRow("{}")] [DataRow("{\"Name\":\"test\"}")] [DataRow("{\"SchemaVersion\":1}")]
    public void IncompleteCatalogHeadersAreRejected(string json) {
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<Catalog>(json, TestData.Options()));
    }
    [TestMethod] public void SeRightRoundTripUsesAccountNamesAndCanonicalPrivilege() {
        SeRightsValue value = new() { Target = LsaPrivilege.SE_NETWORK_LOGON, AccountNames = [] };
        string json = JsonSerializer.Serialize(value, TestData.Options());
        using JsonDocument doc = JsonDocument.Parse(json);
        Assert.AreEqual(JsonValueKind.Array, doc.RootElement.GetProperty("AccountNames").ValueKind);
        SeRightsValue copy = JsonSerializer.Deserialize<SeRightsValue>(json, TestData.Options())!;
        Assert.AreSame(LsaPrivilege.SE_NETWORK_LOGON, copy.Target); Assert.AreEqual(0, copy.AccountNames.Length);
    }
    [TestMethod] public void InvalidPrivilegeCannotDeserializeAsUsableSeRight() {
        string json = """{"Action":"SeRight","Target":"SeInventedPrivilege","AccountNames":[]}""";
        Assert.IsNull(JsonSerializer.Deserialize<SeRightsValue>(json, TestData.Options()));
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<IValue>(json, TestData.Options()));
    }
    [TestMethod] public void ActionLogRoundTripPreservesActualBeforeKindAndOrderingIdentity() {
        RemediationAction source = TestData.Action(); source.Before = TestData.Value("1", RegistryValueKind.String); source.After = TestData.Value(1);
        RemediationAction copy = JsonSerializer.Deserialize<RemediationAction>(JsonSerializer.Serialize(source, TestData.Options()), TestData.Options())!;
        TestData.SamePre(source, copy); Assert.AreEqual(source.RemediationTimestamp, copy.RemediationTimestamp);
        Assert.AreEqual(RegistryValueKind.String, ((RegistryValueValue)copy.Before!).Kind);
        Assert.AreEqual("1", ((RegistryValueValue)copy.Before!).Data); Assert.AreEqual(1, ((RegistryValueValue)copy.After!).Data);
    }
}
