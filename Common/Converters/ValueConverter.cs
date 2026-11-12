namespace SapphTools.DHA.Stig.Common.Converters;
public sealed class ValueConverter : JsonConverter<IValue> {
    public override IValue Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        using JsonDocument document =
            JsonDocument.ParseValue(ref reader);

        JsonElement root = document.RootElement;
        string actionName = nameof(IValue.Action);
        string valuesName = nameof(RegistryValueSetValue.Values);
        if (options.PropertyNamingPolicy is JsonNamingPolicy pol) {
            actionName = pol.ConvertName(actionName);
            valuesName = pol.ConvertName(valuesName);
        }
        if (!root.TryGetProperty(nameof(IValue.Action), out JsonElement actionElement) &&
                !root.TryGetProperty(actionName, out actionElement)) {
            throw new JsonException("IValue is missing Action.");
        }
        bool hasValues = root.TryGetProperty(nameof(RegistryValueSetValue.Values), out _) || root.TryGetProperty(valuesName, out _);

        if (!Enum.TryParse(
                actionElement.GetString(),
                ignoreCase: true,
                out TargetType action)) {
            throw new JsonException(
                $"Invalid Action '{actionElement.GetString()}'.");
        }

        string json = root.GetRawText();

        return action switch {
            TargetType.RegistryValuePattern =>
                (hasValues ?
                    JsonSerializer.Deserialize<RegistryValueSetValue>(json, options) :
                    JsonSerializer.Deserialize<RegistryValuePatternValue>(json, options)
                ) ?? throw new JsonException(),

            TargetType.RegistryValue =>
                JsonSerializer.Deserialize<RegistryValueValue>(
                    json, options)
                ?? throw new JsonException(),

            TargetType.RegistryKey =>
                JsonSerializer.Deserialize<RegistryKeyValue>(
                    json, options)
                ?? throw new JsonException(),

            TargetType.RegistryAcl =>
                JsonSerializer.Deserialize<RegistryAclValue>(
                    json, options)
                ?? throw new JsonException(),

            TargetType.FileSystemAcl =>
                JsonSerializer.Deserialize<FileSystemAclValue>(
                    json, options)
                ?? throw new JsonException(),

            TargetType.SeRight =>
                JsonSerializer.Deserialize<SeRightsValue>(
                    json, options)
                ?? throw new JsonException(),

            TargetType.Sam =>
                JsonSerializer.Deserialize<LockoutValue>(
                    json, options)
                ?? throw new JsonException(),

            TargetType.Service =>
                JsonSerializer.Deserialize<ServiceValue>(
                    json, options)
                ?? throw new JsonException(),

            _ => throw new JsonException(
                $"Unsupported ActionType '{action}'.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        IValue value,
        JsonSerializerOptions options) {
        JsonSerializer.Serialize(
            writer,
            value,
            value.GetType(),
            options);
    }
}
