using SapphTools.SecurityDescriptor;
using System.Diagnostics;

namespace SapphTools.DHA.Stig.Common.Converters;
public class FileSystemAclValueConverter : JsonConverter<FileSystemAclValue> {
    private readonly bool verboseDebug = false;
    public override FileSystemAclValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        if (verboseDebug) { Debug.WriteLine("Deserializing FileSystemAclValue"); }
        string? target = null;
        Sddl? sddl = null;
        while (reader.Read()) {
            if (verboseDebug) { Debug.WriteLine($"  TokenType: {reader.TokenType}"); }
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (target is null || sddl is null) {
                    return null;
                }
                return new() {
                    Target = target,
                    Sddl = sddl,
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (verboseDebug) { Debug.WriteLine($"    ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
            if (reader.ValueTextEquals(nameof(FileSystemAclValue.Action))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                TargetType action = JsonSerializer.Deserialize<TargetType>(ref reader, options);
                if (action != TargetType.FileSystemAcl) {
                    throw new JsonException($"Expected {TargetType.FileSystemAcl}, but got {action}.");
                }
            } else if (reader.ValueTextEquals(nameof(FileSystemAclValue.Target))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                target = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(FileSystemAclValue.Sddl))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                sddl = JsonSerializer.Deserialize<Sddl>(ref reader, options);
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, FileSystemAclValue value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(FileSystemAclValue.Action));
        JsonSerializer.Serialize(writer, value.Action, options);
        writer.WritePropertyName(nameof(FileSystemAclValue.Target));
        writer.WriteStringValue(value.Target);
        writer.WritePropertyName(nameof(FileSystemAclValue.Sddl));
        JsonSerializer.Serialize(writer, value.Sddl, options);
        writer.WriteEndObject();
    }
}
public class RegistryAclValueConverter : JsonConverter<RegistryAclValue> {
    private readonly bool verboseDebug = false;
    public override RegistryAclValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        if (verboseDebug) { Debug.WriteLine("Deserializing FileSystemAclValue"); }
        string? target = null;
        Sddl? sddl = null;
        while (reader.Read()) {
            if (verboseDebug) { Debug.WriteLine($"  TokenType: {reader.TokenType}"); }
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (target is null || sddl is null) {
                    return null;
                }
                return new() {
                    Target = target,
                    Sddl = sddl,
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (verboseDebug) { Debug.WriteLine($"    ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
            if (reader.ValueTextEquals(nameof(RegistryAclValue.Action))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                TargetType action = JsonSerializer.Deserialize<TargetType>(ref reader, options);
                if (action != TargetType.RegistryAcl) {
                    throw new JsonException($"Expected {TargetType.RegistryAcl}, but got {action}.");
                }
            } else if (reader.ValueTextEquals(nameof(RegistryAclValue.Target))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                target = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(RegistryAclValue.Sddl))) {
                reader.Read();
                if (verboseDebug) { Debug.WriteLine($"      TokenType: {reader.TokenType}"); }
                if (verboseDebug) { Debug.WriteLine($"      ValueText: {System.Text.Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
                sddl = JsonSerializer.Deserialize<Sddl>(ref reader, options);
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, RegistryAclValue value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(RegistryAclValue.Action));
        JsonSerializer.Serialize(writer, value.Action, options);
        writer.WritePropertyName(nameof(RegistryAclValue.Target));
        writer.WriteStringValue(value.Target);
        writer.WritePropertyName(nameof(RegistryAclValue.Sddl));
        JsonSerializer.Serialize(writer, value.Sddl, options);
        writer.WriteEndObject();
    }
}