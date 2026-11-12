using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Common.Converters; 
public class RegistryValueSetValueConverter : JsonConverter<RegistryValueSetValue> {
    public static bool VerboseDebug { get; set; } = false;
    public override RegistryValueSetValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        int invocationId = RandomNumberGenerator.GetInt32(int.MaxValue);
        DebugDump(ref reader, invocationId);
        string? target = null;
        string? subpath = null;
        string? name = null;
        bool? overwrite = null;
        RegistryValueKind? kind = null;
        JsonElement? rawData = null;
        Regex? targetPattern = null;
        Regex? pathPattern = null;
        RegistryValueValue? resolved = null;
        List<RegistryValueValue> values = [];
        while (reader.Read()) {
            DebugDump(ref reader, invocationId);
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
                        Overwrite = overwrite.Value,
                        Values = values
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
                    Overwrite = overwrite.Value,
                    Values = values
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (reader.ValueTextEquals(nameof(RegistryValueSetValue.Action))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                TargetType action = JsonSerializer.Deserialize<TargetType>(ref reader, options);
                if (action != TargetType.RegistryValuePattern) {
                    throw new JsonException($"Expected {TargetType.RegistryValuePattern}, but got {action}.");
                }
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.Target))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                target = reader.GetString() ?? string.Empty;
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.TargetPattern))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                try {
                    string? pattern = reader.GetString();
                    if (!string.IsNullOrWhiteSpace(pattern)) {
                        targetPattern = new(pattern);
                    }
                } catch (Exception ex) {
                    throw new JsonException("Failed to instantiate regex object from JSON", ex);
                }
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.SubPath))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                subpath = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.PathPattern))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                try {
                    string? pattern = reader.GetString();
                    if (!string.IsNullOrWhiteSpace(pattern)) {
                        pathPattern = new(pattern);
                    }
                } catch (Exception ex) {
                    throw new JsonException("Failed to instantiate regex object from JSON", ex);
                }
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.Name))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                name = reader.GetString() ?? string.Empty;
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.Data))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                rawData = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.Kind))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                kind = JsonSerializer.Deserialize<RegistryValueKind>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.ResolvedTarget))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                resolved = JsonSerializer.Deserialize<RegistryValueValue>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.Overwrite))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                overwrite = reader.GetBoolean();
            } else if (reader.ValueTextEquals(nameof(RegistryValueSetValue.Values))) {
                reader.Read();
                DebugDump(ref reader, invocationId);
                values = JsonSerializer.Deserialize<List<RegistryValueValue>>(ref reader, options) ?? [];
            } else {
                reader.Read();
                DebugDump(ref reader, invocationId);
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }
    public override void Write(Utf8JsonWriter writer, RegistryValueSetValue value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(RegistryValueSetValue.Action));
        JsonSerializer.Serialize(writer, value.Action, options);
        writer.WritePropertyName(nameof(RegistryValueSetValue.Target));
        writer.WriteStringValue(value.Target);
        writer.WritePropertyName(nameof(RegistryValueSetValue.TargetPattern));
        writer.WriteStringValue(value.TargetPattern?.ToString());
        writer.WritePropertyName(nameof(RegistryValueSetValue.SubPath));
        writer.WriteStringValue(value.SubPath);
        writer.WritePropertyName(nameof(RegistryValueSetValue.PathPattern));
        writer.WriteStringValue(value.PathPattern?.ToString());
        writer.WritePropertyName(nameof(RegistryValueSetValue.Name));
        writer.WriteStringValue(value.Name);
        writer.WritePropertyName(nameof(RegistryValueSetValue.Data));
        if (value.Data is null) {
            writer.WriteNullValue();
        } else {
            switch (value.Kind) {
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    writer.WriteStringValue(value.Data.ToString());
                    break;
                case RegistryValueKind.DWord:
                    writer.WriteNumberValue(Convert.ToInt32(value.Data));
                    break;
                case RegistryValueKind.QWord:
                    writer.WriteNumberValue(Convert.ToInt64(value.Data));
                    break;
                case RegistryValueKind.Binary:
                    writer.WriteBase64StringValue((byte[])value.Data);
                    break;
                case RegistryValueKind.MultiString:
                    JsonSerializer.Serialize(writer, (string[])value.Data, options);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported registry value kind: {value.Kind}");
            }
        }
        writer.WritePropertyName(nameof(RegistryValueSetValue.Kind));
        JsonSerializer.Serialize(writer, value.Kind, options);
        writer.WritePropertyName(nameof(RegistryValueSetValue.ResolvedTarget));
        JsonSerializer.Serialize(writer, value.ResolvedTarget, options);
        writer.WritePropertyName(nameof(RegistryValueSetValue.Overwrite));
        writer.WriteBooleanValue(value.Overwrite);
        writer.WritePropertyName(nameof(RegistryValueSetValue.Values));
        JsonSerializer.Serialize(writer, value.Values, options);
        writer.WriteEndObject();
    }
    private static void DebugDump(ref Utf8JsonReader reader, int invocationId) {
        if (!VerboseDebug) {
            return;
        }
        Debug.WriteLine($"Read operation for invocation {invocationId}");
        Debug.WriteLine($"  BytesConsumed    : {reader.BytesConsumed}");
        Debug.WriteLine($"  CurrentDepth     : {reader.CurrentDepth}");
        Debug.WriteLine($"  HasValueSequence : {reader.HasValueSequence}");
        Debug.WriteLine($"  IsFinalBlock     : {reader.IsFinalBlock}");
        Debug.WriteLine($"  Position         : {reader.Position}");
        Debug.WriteLine($"  TokenStartIndex  : {reader.TokenStartIndex}");
        Debug.WriteLine($"  TokenType        : {reader.TokenType}");
        Debug.WriteLine($"  ValueSpan        : {Encoding.UTF8.GetString(reader.ValueSpan)}");
        Debug.WriteLine("");
    }
}
