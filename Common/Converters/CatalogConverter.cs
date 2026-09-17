using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SapphTools.DHA.Stig.Common.Converters;
public class CatalogConverter : JsonConverter<Catalog> {
    public override Catalog? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        string? name = null;
        int version = -1;
        HashSet<Rule> rules = [];
        List<BlacklistedPlugin> blacklist = [];
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (name is null) {
                    throw new JsonException("Invalid or missing Name property");
                }
                if (version == -1) {
                    throw new JsonException("Invalid or missing SchemaVersion property");
                }
                return new() {
                    Name = name,
                    SchemaVersion = version,
                    Blacklist = blacklist,
                    Rules = rules
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (reader.ValueTextEquals(nameof(Catalog.SchemaVersion))) {
                reader.Read();
                version = reader.GetInt32();
            } else if (reader.ValueTextEquals(nameof(Catalog.Name))) {
                reader.Read();
                name = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(Catalog.Blacklist))) {
                reader.Read();
                blacklist = JsonSerializer.Deserialize<List<BlacklistedPlugin>>(ref reader, options) ?? [];
            } else if (reader.ValueTextEquals(nameof(Catalog.Rules))) {
                reader.Read();
                rules = JsonSerializer.Deserialize<HashSet<Rule>>(ref reader, options) ?? [];
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, Catalog value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(Catalog.Name));
        writer.WriteStringValue(value.Name);
        writer.WritePropertyName(nameof(Catalog.SchemaVersion));
        writer.WriteNumberValue(value.SchemaVersion);
        writer.WritePropertyName(nameof(Catalog.Blacklist));
        JsonSerializer.Serialize(writer, value.Blacklist, options);
        writer.WritePropertyName(nameof(Catalog.Rules));
        JsonSerializer.Serialize(writer, value.Rules, options);
        writer.WriteEndObject();
    }
}
