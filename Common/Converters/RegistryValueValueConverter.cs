using Microsoft.Win32;

namespace SapphTools.DHA.Stig.Common.Converters;
public class RegistryValueValueConverter : JsonConverter<RegistryValueValue> {
    public override RegistryValueValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        string? target = null;
        string? name = null;
        bool? overwrite = null;
        RegistryValueKind? kind = null;
        JsonElement? rawData = null;
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (target is null || name is null || overwrite is null || kind is null) {
                    return null;
                }
                if (rawData is null) {
                    return new() {
                        Target = target,
                        Name = name,
                        Data = null,
                        Kind = kind.Value,
                        Overwrite = overwrite.Value
                    };
                }
                return new() {
                    Target = target,
                    Name = name,
                    Data = ConvertRegistryData(rawData, kind.Value),
                    Kind = kind.Value,
                    Overwrite = overwrite.Value
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (reader.ValueTextEquals(nameof(RegistryValueValue.Action))) {
                reader.Read();
                TargetType action = JsonSerializer.Deserialize<TargetType>(ref reader, options);
                if (action != TargetType.RegistryValue) {
                    throw new JsonException($"Expected {TargetType.RegistryValue}, but got {action}.");
                }
            } else if (reader.ValueTextEquals(nameof(RegistryValueValue.Target))) {
                reader.Read();
                target = reader.GetString() ?? string.Empty;
            } else if (reader.ValueTextEquals(nameof(RegistryValueValue.Name))) {
                reader.Read();
                name = reader.GetString() ?? string.Empty;
            } else if (reader.ValueTextEquals(nameof(RegistryValueValue.Data))) {
                reader.Read();
                rawData = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RegistryValueValue.Kind))) {
                reader.Read();
                kind = JsonSerializer.Deserialize<RegistryValueKind>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RegistryValueValue.Overwrite))) {
                reader.Read();
                overwrite = reader.GetBoolean();
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }
    public override void Write(Utf8JsonWriter writer, RegistryValueValue value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(RegistryValueValue.Action));
        JsonSerializer.Serialize(writer, value.Action, options);
        writer.WritePropertyName(nameof(RegistryValueValue.Target));
        writer.WriteStringValue(value.Target);
        writer.WritePropertyName(nameof(RegistryValueValue.Name));
        writer.WriteStringValue(value.Name);
        writer.WritePropertyName(nameof(RegistryValueValue.Data));
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
        writer.WritePropertyName(nameof(RegistryValueValue.Kind));
        JsonSerializer.Serialize(writer, value.Kind, options);
        writer.WritePropertyName(nameof(RegistryValueValue.Overwrite));
        writer.WriteBooleanValue(value.Overwrite);
        writer.WriteEndObject();
    }
    internal static object? ConvertRegistryData(
            JsonElement? data,
            RegistryValueKind kind) {
        if (data is null ||
            data.Value.ValueKind == JsonValueKind.Null) {
            return null;
        }
        JsonElement element = data.Value;
        return kind switch {
            RegistryValueKind.String =>
                element.GetString(),
            RegistryValueKind.ExpandString =>
                element.GetString(),
            RegistryValueKind.DWord =>
                element.GetInt32(),
            RegistryValueKind.QWord =>
                element.GetInt64(),
            RegistryValueKind.Binary =>
                element.GetBytesFromBase64(),
            RegistryValueKind.MultiString =>
                element.EnumerateArray()
                    .Select(x => x.GetString() ?? string.Empty)
                    .ToArray(),
            _ => throw new JsonException(
                $"Unsupported registry value kind: {kind}")
        };
    }
}