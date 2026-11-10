using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SapphTools.DHA.Stig.Remediator.TestSled;
public class TestCaseConverter : JsonConverter<TestCase> {
    private readonly bool verboseDebug = false;
    public override TestCase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        if (verboseDebug) { Debug.WriteLine("Deserializing TestCase"); }
        int order = -1;
        IValue? expectedAfter = null;
        Setting? setting = null;
        Dictionary<string, TestResult?> results = [];
        ActionResult expectedResult = (ActionResult)(-1);
        while (reader.Read()) {
            if (verboseDebug) { Debug.WriteLine($"  TokenType: {reader.TokenType}"); }
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (order == -1) {
                    throw new JsonException("Invalid or missing Order property");
                }
                if (setting is null) {
                    throw new JsonException("Invalid or missing Setting property");
                }
                if ((int)expectedResult == -1) {
                    throw new JsonException("Invalid or missing ExpectedResult property");
                }
                return new() {
                    Order = order,
                    Setting = setting,
                    Results = results,
                    ExpectedAfter = expectedAfter,
                    ExpectedResult = expectedResult
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (verboseDebug) { Debug.WriteLine($"    ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
            if (reader.ValueTextEquals(nameof(TestCase.Order))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                order = reader.GetInt32();
            } else if (reader.ValueTextEquals(nameof(TestCase.Setting))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                SettingConverter conv = new();
                setting = conv.Read(ref reader, typeof(Setting), options);
            } else if (reader.ValueTextEquals(nameof(TestCase.ExpectedAfter))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                ValueConverter conv = new();
                if (reader.TokenType == JsonTokenType.Null) {
                    expectedAfter = null;
                } else {
                    expectedAfter = conv.Read(ref reader, typeof(IValue), options);
                }
            } else if (reader.ValueTextEquals(nameof(TestCase.Results))) {
                reader.Read();
                results = JsonSerializer.Deserialize<Dictionary<string, TestResult?>>(ref reader, options) ?? [];
            } else if (reader.ValueTextEquals(nameof(TestCase.ExpectedResult))) {
                reader.Read();
                expectedResult = JsonSerializer.Deserialize<ActionResult>(ref reader, options);
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, TestCase value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(TestCase.Order));
        writer.WriteNumberValue(value.Order);
        writer.WritePropertyName(nameof(TestCase.Setting));
        JsonSerializer.Serialize(writer, value.Setting, options);
        writer.WritePropertyName(nameof(TestCase.Results));
        JsonSerializer.Serialize(writer, value.Results, options);
        writer.WritePropertyName(nameof(TestCase.ExpectedAfter));
        writer.NullWriter(value.ExpectedAfter, options);
        writer.WritePropertyName(nameof(TestCase.ExpectedResult));
        JsonSerializer.Serialize(writer, value.ExpectedResult, options);
        writer.WriteEndObject();
    }
}
