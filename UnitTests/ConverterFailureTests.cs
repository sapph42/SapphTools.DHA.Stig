using System.Text.Json.Nodes;

namespace UnitTests;

[TestClass, TestCategory("Pure")]
public sealed class ConverterFailureTests {
    public static IEnumerable<object[]> Readers() {
        foreach (Type type in new[] { typeof(FileSystemAclValue), typeof(RegistryAclValue), typeof(SeRightsValue), typeof(Setting) })
            foreach (string json in new[] { "[]", "1", "true", "\"text\"", "{", "{\"Unknown\":[1," })
                yield return [type, json];
    }
    [DataTestMethod, DynamicData(nameof(Readers), DynamicDataSourceType.Method)]
    public void ReadersRejectNonObjectsAndTruncatedDocuments(Type type, string json) {
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize(json, type, TestData.Options()));
    }

    [DataTestMethod, DataRow(false), DataRow(true)]
    public void AclReadersRejectWrongActionsAndIncompleteObjects(bool registry) {
        Type type = registry ? typeof(RegistryAclValue) : typeof(FileSystemAclValue);
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize("""{"Action":"SeRight"}""", type, TestData.Options()));
        foreach (string json in new[] { "{}", "{\"Target\":\"Test\"}", "{\"Target\":null}", "{\"Sddl\":null}" })
            Assert.IsNull(JsonSerializer.Deserialize(json, type, TestData.Options()));
        string action = registry ? "RegistryAcl" : "FileSystemAcl";
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<IValue>($$"""{"Action":"{{action}}","Target":"Test"}""", TestData.Options()));
    }

    [DataTestMethod]
    [DataRow("{}")]
    [DataRow("{\"Target\":\"SeNetworkLogonRight\"}")]
    [DataRow("{\"Target\":null,\"AccountNames\":[]}")]
    [DataRow("{\"Target\":\"SeNetworkLogonRight\",\"AccountNames\":null}")]
    public void IncompleteSeRightsCannotBecomeUsableValues(string json) {
        Assert.IsNull(JsonSerializer.Deserialize<SeRightsValue>(json, TestData.Options()));
    }

    [TestMethod]
    public void SeRightsReaderSkipsNestedUnknownFieldsAndRejectsWrongAction() {
        SeRightsValue value = JsonSerializer.Deserialize<SeRightsValue>(
            """{"Future":{"x":[{}]},"AccountNames":[],"Target":"SeNetworkLogonRight","Action":"SeRight"}""", TestData.Options())!;
        Assert.AreSame(LsaPrivilege.SE_NETWORK_LOGON, value.Target); Assert.AreEqual(0, value.AccountNames.Length);
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<SeRightsValue>("""{"Action":"RegistryAcl"}""", TestData.Options()));
    }

    [TestMethod]
    public void SettingReaderRejectsInvalidContextAndMissingNestedValue() {
        Setting setting = new() { Order = 0, Data = TestData.Value(1) };
        JsonObject json = JsonNode.Parse(JsonSerializer.Serialize(setting, TestData.Options()))!.AsObject();
        json["RequiredContext"] = "Invented";
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<Setting>(json.ToJsonString(), TestData.Options()));
        json["RequiredContext"] = "None"; json["Data"] = new JsonObject { ["Action"] = "RegistryValue" };
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<Setting>(json.ToJsonString(), TestData.Options()));
    }
}
