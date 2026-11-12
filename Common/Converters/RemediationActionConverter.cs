using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SapphTools.DHA.Stig.Common.Converters;
public class RemediationActionConverter : JsonConverter<RemediationAction> {
    private readonly bool verboseDebug = false;
    public override RemediationAction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        if (verboseDebug) { Debug.WriteLine("Deserializing TestCase"); }
        Guid? remBatch = null;
        Guid? ruleBatch = null;
        Guid? settingBatch = null;
        int? actionNo = null;
        ActionSource source = (ActionSource)(-1);
        string? ruleId = null;
        string? description = null;
        string? computerName = null;
        int? settingIdx = null;
        TargetType type = (TargetType)(-1);
        string? target = null;
        RollbackCapability rollback = (RollbackCapability)(-1);
        IValue? before = null;
        IValue? after = null;
        ActionResult result = (ActionResult)(-1);
        string? failureMessage = null;
        DateTimeOffset? timestamp = null;
        ValueConverter conv = new();
        while (reader.Read()) {
            if (verboseDebug) { Debug.WriteLine($"  TokenType: {reader.TokenType}"); }
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (remBatch is null) {
                    throw new JsonException("Invalid or missing RemediationBatch property");
                }
                if (ruleBatch is null) {
                    throw new JsonException("Invalid or missing RuleBatch property");
                }
                if (settingBatch is null) {
                    throw new JsonException("Invalid or missing SettingBatch property");
                }
                if (actionNo is null) {
                    throw new JsonException("Invalid or missing ActionNumber property");
                }
                if ((int)source == -1) {
                    throw new JsonException("Invalid or missing Source property");
                }
                if (ruleId is null) {
                    throw new JsonException("Invalid or missing RuleId property");
                }
                if (computerName is null) {
                    throw new JsonException("Invalid or missing ComputerName property");
                }
                if (settingIdx is null) {
                    throw new JsonException("Invalid or missing SettingIndex property");
                }
                if ((int)type == -1) {
                    throw new JsonException("Invalid or missing TargetType property");
                }
                if ((int)rollback == -1) {
                    throw new JsonException("Invalid or missing RollbackCapability property");
                }
                if ((int)result == -1) {
                    throw new JsonException("Invalid or missing Result property");
                }
                if (target is null) {
                    throw new JsonException("Invalid or missing Target property");
                }
                if (timestamp is null) {
                    throw new JsonException("Invalid or missing RemediationTimestamp property");
                }
                return new() {
                    RemediationBatch = remBatch.Value,
                    RuleBatch = ruleBatch.Value,
                    SettingBatch = settingBatch.Value,
                    ActionNumber = actionNo.Value,
                    Source = source,
                    RuleId = ruleId,
                    Description = description,
                    ComputerName = computerName,
                    SettingIndex = settingIdx.Value,
                    TargetType = type,
                    Target = target,
                    RollbackCapability = rollback,
                    Result = result,
                    Before = before,
                    After = after,
                    FailureMessage = failureMessage,
                    RemediationTimestamp = timestamp.Value
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (verboseDebug) { Debug.WriteLine($"    ValueText: {Encoding.UTF8.GetString(reader.ValueSpan.ToArray())}"); }
            if (reader.ValueTextEquals(nameof(RemediationAction.RemediationBatch))) {
                reader.Read();
                remBatch = JsonSerializer.Deserialize<Guid>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RemediationAction.RuleBatch))) {
                reader.Read();
                ruleBatch = JsonSerializer.Deserialize<Guid>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RemediationAction.SettingBatch))) {
                reader.Read();
                settingBatch = JsonSerializer.Deserialize<Guid>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RemediationAction.ActionNumber))) {
                reader.Read();
                actionNo = reader.GetInt32();
            } else if (reader.ValueTextEquals(nameof(RemediationAction.Source))) {
                reader.Read();
                source = JsonSerializer.Deserialize<ActionSource>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RemediationAction.RuleId))) {
                reader.Read();
                ruleId = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(RemediationAction.Description))) {
                reader.Read();
                description = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(RemediationAction.ComputerName))) {
                reader.Read();
                computerName = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(RemediationAction.SettingIndex))) {
                reader.Read();
                settingIdx = reader.GetInt32();
            } else if (reader.ValueTextEquals(nameof(RemediationAction.TargetType))) {
                reader.Read();
                type = JsonSerializer.Deserialize<TargetType>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RemediationAction.Target))) {
                reader.Read();
                target = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(RemediationAction.RollbackCapability))) {
                reader.Read();
                rollback = JsonSerializer.Deserialize<RollbackCapability>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RemediationAction.Before))) {
                reader.Read();
                if (reader.TokenType == JsonTokenType.Null) {
                    before = null;
                } else {
                    before = conv.Read(ref reader, typeof(IValue), options);
                }
            } else if (reader.ValueTextEquals(nameof(RemediationAction.After))) {
                reader.Read();
                if (reader.TokenType == JsonTokenType.Null) {
                    after = null;
                } else {
                    after = conv.Read(ref reader, typeof(IValue), options);
                }
            } else if (reader.ValueTextEquals(nameof(RemediationAction.Result))) {
                reader.Read();
                result = JsonSerializer.Deserialize<ActionResult>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(RemediationAction.FailureMessage))) {
                reader.Read();
                failureMessage = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(RemediationAction.RemediationTimestamp))) {
                reader.Read();
                timestamp = reader.GetDateTimeOffset();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, RemediationAction value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(RemediationAction.RemediationBatch));
        writer.WriteStringValue(value.RemediationBatch.ToString());
        writer.WritePropertyName(nameof(RemediationAction.RuleBatch));
        writer.WriteStringValue(value.RuleBatch.ToString());
        writer.WritePropertyName(nameof(RemediationAction.SettingBatch));
        writer.WriteStringValue(value.SettingBatch.ToString());
        writer.WritePropertyName(nameof(RemediationAction.ActionNumber));
        writer.WriteNumberValue(value.ActionNumber);
        writer.WritePropertyName(nameof(RemediationAction.Source));
        JsonSerializer.Serialize(writer, value.Source, options);
        writer.WritePropertyName(nameof(RemediationAction.RuleId));
        writer.WriteStringValue(value.RuleId);
        writer.WritePropertyName(nameof(RemediationAction.Description));
        writer.NullWriter(value.Description, writer.WriteStringValue);
        writer.WritePropertyName(nameof(RemediationAction.ComputerName));
        writer.WriteStringValue(value.ComputerName);
        writer.WritePropertyName(nameof(RemediationAction.SettingIndex));
        writer.WriteNumberValue(value.SettingIndex);
        writer.WritePropertyName(nameof(RemediationAction.TargetType));
        JsonSerializer.Serialize(writer, value.TargetType, options);
        writer.WritePropertyName(nameof(RemediationAction.Target));
        writer.WriteStringValue(value.Target);
        writer.WritePropertyName(nameof(RemediationAction.RollbackCapability));
        JsonSerializer.Serialize(writer, value.RollbackCapability, options);
        writer.WritePropertyName(nameof(RemediationAction.Before));
        writer.NullWriter(value.Before, options);
        writer.WritePropertyName(nameof(RemediationAction.After));
        writer.NullWriter(value.After, options);
        writer.WritePropertyName(nameof(RemediationAction.Result));
        JsonSerializer.Serialize(writer, value.Result, options);
        writer.WritePropertyName(nameof(RemediationAction.FailureMessage));
        writer.NullWriter(value.FailureMessage, writer.WriteStringValue);
        writer.WritePropertyName(nameof(RemediationAction.RemediationTimestamp));
        writer.WriteStringValue(value.RemediationTimestamp.ToString("O"));
        writer.WriteEndObject();
    }
}
