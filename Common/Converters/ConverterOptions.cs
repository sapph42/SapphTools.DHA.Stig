namespace SapphTools.DHA.Stig.Common.Converters; 
public static class ConverterOptions {
    private static JsonSerializerOptions? opts;
    private readonly static List<JsonConverter> converters = [
        new ArtifactCatalogConverter(),
        new CatalogConverter(),
        new RegistryValuePatternValueConverter(),
        new RegistryValueValueConverter(),
        new SeRightsValueConverter(),
        new SettingConverter(),
        new ValueConverter(),
        new SecurityDescriptor.Converters.TrusteeConverter(),
        new JsonStringEnumConverter()
    ];
    public static JsonSerializerOptions JsonSerializerOptions {
        get {
            if (opts is not null) {
                return opts;
            }
            opts = new();
            foreach (JsonConverter converter in converters) {
                opts.Converters.Add(converter);
            }
            return opts;
        }
    }
}
