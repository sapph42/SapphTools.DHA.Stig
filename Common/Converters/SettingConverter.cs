namespace SapphTools.DHA.Stig.Common.Converters;
public class SettingConverter : JsonConverter<Setting> {
    public override Setting? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        int order = -1;
        IValue? data = null;
        SettingContext context = SettingContext.Administrator;
        bool dangerous = false;
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (order == -1) {
                    throw new JsonException("Invalid or missing Order property");
                }
                if (data is null) {
                    throw new JsonException("Invalid or missing Data property");
                }
                return new() {
                    Order = order,
                    Data = data
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (reader.ValueTextEquals(nameof(Setting.Order))) {
                reader.Read();
                order = reader.GetInt32();
            } else if (reader.ValueTextEquals(nameof(Setting.RequiredContext))) {
                reader.Read();
                context = JsonSerializer.Deserialize<SettingContext>(ref reader, options);
            } else if (reader.ValueTextEquals(nameof(Setting.Dangerous))) {
                reader.Read();
                dangerous = reader.GetBoolean();
            } else if (reader.ValueTextEquals(nameof(Setting.Data))) {
                reader.Read();
                ValueConverter conv = new();
                data = conv.Read(ref reader, typeof(IValue), options);
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, Setting value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(Setting.Order));
        writer.WriteNumberValue(value.Order);
        writer.WritePropertyName(nameof(Setting.RequiredContext));
        JsonSerializer.Serialize(writer, value.RequiredContext, options);
        writer.WritePropertyName(nameof(Setting.Dangerous));
        writer.WriteBooleanValue(value.Dangerous);
        writer.WritePropertyName(nameof(Setting.Data));
        JsonSerializer.Serialize(writer, value.Data, value.Data.GetType(), options);
        writer.WriteEndObject();
    }
}
