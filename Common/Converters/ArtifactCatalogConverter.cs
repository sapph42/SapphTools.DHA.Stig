
using SapphTools.SecurityDescriptor.Classes;

namespace SapphTools.DHA.Stig.Common.Converters; 
public class ArtifactCatalogConverter : JsonConverter<ArtifactCatalog> {
    public override ArtifactCatalog? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected StartObject token.");
        }
        string? relPath = null;
        string? hash = null;
        string? hashAlgo = null;
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (relPath is null || hash is null || hashAlgo is null) {
                    throw new JsonException("Invalid IArtifact object structure");
                }
                return new() {
                    RelativePath = relPath,
                    Hash = hash,
                    HashAlgorithm = hashAlgo
                };
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (reader.ValueTextEquals(nameof(ArtifactCatalog.RelativePath))) {
                reader.Read();
                relPath = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(ArtifactCatalog.Hash))) {
                reader.Read();
                hash = reader.GetString();
            } else if (reader.ValueTextEquals(nameof(ArtifactCatalog.HashAlgorithm))) {
                reader.Read();
                hashAlgo = reader.GetString();
            } else {
                reader.Read();
                reader.Skip();
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }
    public override void Write(Utf8JsonWriter writer, ArtifactCatalog value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(ArtifactCatalog.RelativePath));
        writer.WriteStringValue(value.RelativePath);
        writer.WritePropertyName(nameof(ArtifactCatalog.Hash));
        writer.WriteStringValue(value.Hash);
        writer.WritePropertyName(nameof(ArtifactCatalog.HashAlgorithm));
        writer.WriteStringValue(value.HashAlgorithm);
        writer.WriteEndObject();
    }
}
