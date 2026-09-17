using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Common.Converters; 
public class RegistryValuePatternValueConverter : JsonConverter<RegistryValuePatternValue> {
    public override RegistryValuePatternValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        string? target = null;
        string? subpath = null;
        string? name = null;
        bool? overwrite = null;
        RegistryValueKind? kind = null;
        JsonElement? rawData = null;
        Regex? targetPattern = null;
        Regex? pathPattern = null;
        RegistryValueValue? resolved = null;
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (target is null || name is null || overwrite is null || kind is null) {
                    return null;
                }
                if (rawData is null) {
                    return new() {
                        Target = target,
                        TargetPattern = targetPattern,
                        SubPath = subpath,
                        PathPattern = pathPattern,
                        Name = name,
                        Data = null,
                        Kind = kind.Value,
                        ResolvedTarget = resolved,
                        Overwrite = overwrite.Value
                    };
                }
                return new() {
                    Target = target,
                    TargetPattern = targetPattern,
                    SubPath = subpath,
                    PathPattern = pathPattern,
                    Name = name,
                    Data = RegistryValueValueConverter.ConvertRegistryData(rawData, kind.Value),
                    Kind = kind.Value,
                    ResolvedTarget = resolved,
                    Overwrite = overwrite.Value
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.Action))) {
                reader.Read();
                TargetType action = JsonSerializer.Deserialize<TargetType>(ref reader, options);
                if (action != TargetType.RegistryValuePattern) {
                    throw new JsonException($"Expected {TargetType.RegistryValuePattern}, but got {action}.");
                }
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.Target))) {
                reader.Read();
                target = reader.GetString() ?? string.Empty;
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.TargetPattern))) {
                reader.Read();
                try {
                    string? pattern = reader.GetString();
                    if (!string.IsNullOrWhiteSpace(pattern)) {
                        targetPattern = new(pattern);
                    }
                } catch { }
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.SubPath))) {
                reader.Read();
                subpath = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.PathPattern))) {
                reader.Read();
                try {
                    string? pattern = reader.GetString();
                    if (!string.IsNullOrWhiteSpace(pattern)) {
                        pathPattern = new(pattern);
                    }
                } catch { }
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.Name))) {
                reader.Read();
                name = reader.GetString() ?? string.Empty;
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.Data))) {
                reader.Read();
                rawData = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.Kind))) {
                reader.Read();
                kind = JsonSerializer.Deserialize<RegistryValueKind>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.ResolvedTarget))) {
                reader.Read();
                resolved = JsonSerializer.Deserialize<RegistryValueValue>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RegistryValuePatternValue.Overwrite))) {
                reader.Read();
                overwrite = reader.GetBoolean();
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }
    public override void Write(Utf8JsonWriter writer, RegistryValuePatternValue value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(RegistryValuePatternValue.Action));
        JsonSerializer.Serialize(writer, value.Action, options);
        writer.WritePropertyName(nameof(RegistryValuePatternValue.Target));
        writer.WriteStringValue(value.Target);
        writer.WritePropertyName(nameof(RegistryValuePatternValue.TargetPattern));
        writer.WriteStringValue(value.TargetPattern?.ToString());
        writer.WritePropertyName(nameof(RegistryValuePatternValue.SubPath));
        writer.WriteStringValue(value.SubPath);
        writer.WritePropertyName(nameof(RegistryValuePatternValue.PathPattern));
        writer.WriteStringValue(value.PathPattern?.ToString());
        writer.WritePropertyName(nameof(RegistryValuePatternValue.Name));
        writer.WriteStringValue(value.Name);
        writer.WritePropertyName(nameof(RegistryValuePatternValue.Data));
        switch (value.Kind) {
            case RegistryValueKind.String:
            case RegistryValueKind.ExpandString:
                writer.WriteStringValue(value.Data?.ToString());
                break;
            case RegistryValueKind.DWord:
                writer.WriteNumberValue(Convert.ToInt32(value.Data));
                break;
            case RegistryValueKind.QWord:
                writer.WriteNumberValue(Convert.ToInt64(value.Data));
                break;
            case RegistryValueKind.Binary:
                if (value.Data is null) {
                    writer.WriteBase64StringValue((byte[])[]);
                } else {
                    writer.WriteBase64StringValue((byte[])value.Data);
                }
                break;
            case RegistryValueKind.MultiString:
                if (value.Data is null) {
                    JsonSerializer.Serialize(writer, (string[])[], options);
                    writer.WriteBase64StringValue((byte[])[]);
                } else {
                    JsonSerializer.Serialize(writer, (string[])value.Data, options);
                }
                break;
            default:
                throw new NotSupportedException($"Unsupported registry value kind: {value.Kind}");
        }
        writer.WritePropertyName(nameof(RegistryValuePatternValue.Kind));
        JsonSerializer.Serialize(writer, value.Kind, options);
        writer.WritePropertyName(nameof(RegistryValuePatternValue.ResolvedTarget));
        JsonSerializer.Serialize(writer, value.ResolvedTarget, options);
        writer.WritePropertyName(nameof(RegistryValuePatternValue.Overwrite));
        writer.WriteBooleanValue(value.Overwrite);
        writer.WriteEndObject();
    }
}
