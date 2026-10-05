namespace UnitTests;
internal static class TestData {
    // Take a private copy: parallel tests must never mutate the application's cached options.
    private static readonly JsonSerializerOptions baselineOptions = new(ConverterOptions.JsonSerializerOptions);
    internal static JsonSerializerOptions Options() => new(baselineOptions);
    internal static RegistryValueValue Value(object? data = null, RegistryValueKind kind = RegistryValueKind.DWord) => new() {
        Target = @"HKEY_CURRENT_USER\Software\Test", Name = "Value", Data = data, Kind = kind, Overwrite = true
    };
    internal static RemediationPreAction Pre() => new() {
        RemediationBatch = Guid.NewGuid(), RuleBatch = Guid.NewGuid(), SettingBatch = Guid.NewGuid(),
        ComputerName = "test-host", RuleId = "V-123", SettingIndex = 3, ActionNumber = 7,
        Description = "test description", Source = ActionSource.Rollback
    };
    internal static RemediationAction Action(RemediationPreAction? pre = null) => new(pre ?? Pre(), "test-target") {
        TargetType = TargetType.RegistryValue, Result = ActionResult.ActionSuccess,
        RollbackCapability = RollbackCapability.Automatic,
        RemediationTimestamp = new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero)
    };
    internal static void SamePre(RemediationPreAction expected, RemediationPreAction actual) {
        Assert.AreEqual(expected.RemediationBatch, actual.RemediationBatch);
        Assert.AreEqual(expected.RuleBatch, actual.RuleBatch);
        Assert.AreEqual(expected.SettingBatch, actual.SettingBatch);
        Assert.AreEqual(expected.ComputerName, actual.ComputerName);
        Assert.AreEqual(expected.RuleId, actual.RuleId);
        Assert.AreEqual(expected.SettingIndex, actual.SettingIndex);
        Assert.AreEqual(expected.ActionNumber, actual.ActionNumber);
        Assert.AreEqual(expected.Description, actual.Description);
        Assert.AreEqual(expected.Source, actual.Source);
    }
    internal static void SameData(object? expected, object? actual) {
        if (expected is byte[] bytes) {
            Assert.IsInstanceOfType(actual, typeof(byte[]));
            CollectionAssert.AreEqual(bytes, (byte[])actual!);
        } else if (expected is string[] strings) {
            Assert.IsInstanceOfType(actual, typeof(string[]));
            CollectionAssert.AreEqual(strings, (string[])actual!);
        } else {
            Assert.AreEqual(expected, actual);
            if (expected is not null) Assert.AreEqual(expected.GetType(), actual?.GetType());
        }
    }
    internal static IEnumerable<object[]> RegistryData() {
        yield return [RegistryValueKind.DWord, 0];
        yield return [RegistryValueKind.DWord, int.MinValue];
        yield return [RegistryValueKind.DWord, int.MaxValue];
        yield return [RegistryValueKind.QWord, 0L];
        yield return [RegistryValueKind.QWord, long.MinValue];
        yield return [RegistryValueKind.QWord, long.MaxValue];
        yield return [RegistryValueKind.String, ""];
        yield return [RegistryValueKind.String, "quotes \" slash \\ newline\n日本語 😀"];
        yield return [RegistryValueKind.ExpandString, @"%SystemRoot%\Test"];
        yield return [RegistryValueKind.Binary, Array.Empty<byte>()];
        yield return [RegistryValueKind.Binary, new byte[] { 0, 1, 127, 128, 255 }];
        yield return [RegistryValueKind.MultiString, Array.Empty<string>()];
        yield return [RegistryValueKind.MultiString, new string[] { "first", "日本語", "last" }];
    }
}
