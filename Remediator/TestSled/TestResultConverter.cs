using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SapphTools.DHA.Stig.Remediator.TestSled;
public class TestResultConverter : JsonConverter<TestResult> {
    private readonly bool verboseDebug = false;
    public override TestResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        if (verboseDebug) { Debug.WriteLine("Deserializing TestCase"); }
        IValue? actualAfter = null;
        RemediationAction? action = null;
        bool? rollbackPass = null;
        string? error = null;
        string? rollbackError = null;
        while (reader.Read()) {
            if (verboseDebug) { Debug.WriteLine($"  TokenType: {reader.TokenType}"); }
            if (reader.TokenType == JsonTokenType.EndObject) {
                return new() {
                    ActualAfter = actualAfter,
                    RemediationAction = action,
                    RollbackPass = rollbackPass,
                    Error = error,
                    RollbackError = rollbackError
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (verboseDebug) { Debug.WriteLine($"    ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
            if (reader.ValueTextEquals(nameof(TestResult.ActualAfter))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                ValueConverter conv = new();
                if (reader.TokenType == JsonTokenType.Null) {
                    actualAfter = null;
                } else {
                    actualAfter = conv.Read(ref reader, typeof(IValue), options);
                }
            } else if (reader.ValueTextEquals(nameof(TestResult.RemediationAction))) {
                reader.Read();
                action = JsonSerializer.Deserialize<RemediationAction>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(TestResult.RollbackPass))) {
                reader.Read();
                rollbackPass = JsonSerializer.Deserialize<bool?>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(TestResult.Error))) {
                reader.Read();
                error = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(TestResult.RollbackError))) {
                reader.Read();
                rollbackError = reader.GetString();
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, TestResult value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(TestResult.ActualAfter));
        writer.NullWriter(value.ActualAfter, options);
        writer.WritePropertyName(nameof(TestResult.RemediationAction));
        writer.NullWriter(value.RemediationAction, options);
        writer.WritePropertyName(nameof(TestResult.RollbackPass));
        writer.NullWriter(value.RollbackPass, writer.WriteBooleanValue);
        writer.WritePropertyName(nameof(TestResult.Error));
        writer.NullWriter(value.Error, writer.WriteStringValue);
        writer.WritePropertyName(nameof(TestResult.RollbackError));
        writer.NullWriter(value.RollbackError, writer.WriteStringValue);
        writer.WriteEndObject();
    }
}
