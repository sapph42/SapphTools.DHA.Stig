
using SapphTools.SecurityDescriptor.Classes;
using System.Diagnostics;

namespace SapphTools.DHA.Stig.Common.Converters; 
public class SeRightsValueConverter : JsonConverter<SeRightsValue> {
    private readonly bool verboseDebug = false;
    public override SeRightsValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        LsaPrivilege? priv = null;
        List<Trustee>? trustees = null;
        if (verboseDebug) { Debug.WriteLine("Deserializing SeRightsValue"); }
        while (reader.Read()) {
            if (verboseDebug) { Debug.WriteLine($"  TokenType: {reader.TokenType}"); }
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (priv is null || trustees is null) {
                    return null;
                }
                return new() {
                    AccountNames = [.. trustees],
                    Target = priv
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (verboseDebug) { Debug.WriteLine($"    ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
            if (reader.ValueTextEquals(nameof(SeRightsValue.Action))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                TargetType action = JsonSerializer.Deserialize<TargetType>(ref reader, options);
                if (action != TargetType.SeRight) {
                    throw new JsonException($"Expected {TargetType.SeRight}, but got {action}.");
                }
            } else if (reader.ValueTextEquals(nameof(SeRightsValue.Target))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                if (reader.GetString() is string privText) {
                    LsaPrivilege.ValidPrivs.TryGetValue(privText, out priv);
                } else {
                    if (verboseDebug) { Debug.WriteLine(System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())); }
                }
            } else if (reader.ValueTextEquals(nameof(SeRightsValue.AccountNames))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                trustees = JsonSerializer.Deserialize<List<Trustee>>(ref reader, options);
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }
    public override void Write(Utf8JsonWriter writer, SeRightsValue value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(SeRightsValue.Action));
        JsonSerializer.Serialize(writer, value.Action, options);
        writer.WritePropertyName(nameof(SeRightsValue.Target));
        writer.WriteStringValue(value.Target.Value);
        writer.WritePropertyName(nameof(SeRightsValue.Target));
        JsonSerializer.Serialize(writer, value.AccountNames, options);
        writer.WriteEndObject();
    }
}
