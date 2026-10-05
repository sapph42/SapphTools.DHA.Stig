namespace UnitTests;
[TestClass, TestCategory("Catalog")]
public sealed class CatalogTests {
    public static IEnumerable<object[]> CatalogSettings() {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "catalog.json")));
        foreach (JsonElement rule in document.RootElement.GetProperty("Rules").EnumerateArray())
            foreach (JsonElement setting in rule.GetProperty("Settings").EnumerateArray())
                yield return [rule.GetProperty("RuleId").GetString()!, setting.GetProperty("Order").GetInt32(), setting.GetRawText()];
    }
    [DataTestMethod, DynamicData(nameof(CatalogSettings), DynamicDataSourceType.Method)]
    public void EveryShippedSettingCanDeserializeAndRoundTrip(string ruleId, int order, string json) {
        using JsonDocument document = JsonDocument.Parse(json);
        string? action = document.RootElement.GetProperty("Data").GetProperty("Action").GetString();
        if (!OperatingSystem.IsWindows() && action is "SeRight" or "FileSystemAcl" or "RegistryAcl")
            Assert.Inconclusive("This catalog entry constructs Windows SecurityIdentifier objects.");
        Setting source = JsonSerializer.Deserialize<Setting>(json, TestData.Options())!;
        Assert.IsNotNull(source, ruleId); Assert.AreEqual(order, source.Order);
        string serialized = JsonSerializer.Serialize(source, TestData.Options());
        Setting copy = JsonSerializer.Deserialize<Setting>(serialized, TestData.Options())!;
        Assert.AreEqual(source.RequiredContext, copy.RequiredContext, ruleId); Assert.AreEqual(source.Dangerous, copy.Dangerous, ruleId);
        Assert.AreEqual(source.Data.Action, copy.Data.Action, ruleId); Assert.AreEqual(source.Data.TargetString, copy.Data.TargetString, ruleId);
        Assert.AreEqual(serialized, JsonSerializer.Serialize(copy, TestData.Options()), ruleId);
        if (source.Data is RegistryValueValue value) TestData.SameData(value.Data, ((RegistryValueValue)copy.Data).Data);
        if (source.Data is RegistryValuePatternValue pattern) TestData.SameData(pattern.Data, ((RegistryValuePatternValue)copy.Data).Data);
    }
    [TestMethod] public void ShippedCatalogDeserializesAsAWholeAndRoundTripsItsRules() {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("Full catalog includes Windows principal/ACL models.");
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "catalog.json");
        Catalog source = JsonSerializer.Deserialize<Catalog>(File.ReadAllText(path), TestData.Options())!;
        Assert.IsTrue(source.Rules.Count > 0); Assert.IsFalse(string.IsNullOrWhiteSpace(source.Name));
        Catalog copy = JsonSerializer.Deserialize<Catalog>(JsonSerializer.Serialize(source, TestData.Options()), TestData.Options())!;
        Assert.AreEqual(source.Name, copy.Name); Assert.AreEqual(source.SchemaVersion, copy.SchemaVersion);
        Assert.AreEqual(source.Rules.Count, copy.Rules.Count); Assert.AreEqual(source.Blacklist.Count, copy.Blacklist.Count);
        Assert.AreEqual(source.Rules.Sum(r => r.Settings.Count), copy.Rules.Sum(r => r.Settings.Count));
    }
}
