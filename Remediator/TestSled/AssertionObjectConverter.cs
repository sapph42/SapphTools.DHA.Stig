using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SapphTools.DHA.Stig.Remediator.TestSled;
public class AssertionObjectConverter : JsonConverter<IAssertionObject> {
    private static readonly Dictionary<string, Type> AssertionTypes =
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(IAssertionObject).IsAssignableFrom(t))
            .ToDictionary(t => t.Name, StringComparer.Ordinal);
    public override IAssertionObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;
        string aotName = nameof(IAssertionObject.AssertionObjectType);
        string polAotName = new(aotName);
        if (options.PropertyNamingPolicy is JsonNamingPolicy pol) {
            polAotName = pol.ConvertName(aotName);
        }
        if (!root.TryGetProperty(aotName, out JsonElement aotElement) &&
                !root.TryGetProperty(polAotName, out aotElement)) {
            throw new JsonException("Missing AssertionObjectType property.");
        }
        if (aotElement.GetString() is not string aot) {
            throw new JsonException("AssertionObjectType property has no value.");
        }
        if (!AssertionTypes.TryGetValue(aot, out Type? type)) {
            throw new JsonException($"No class named {aot} implementing IAssertion object could be found.");
        }
        return (IAssertionObject?)root.Deserialize(type, options);
    }

    public override void Write(Utf8JsonWriter writer, IAssertionObject value, JsonSerializerOptions options) {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
