using System.Text.Json;
using System.Text.Json.Serialization;

namespace SapphTools.DHA.Stig.Remediator.Classes; 
internal static class GlobalConstants {
    public readonly static JsonSerializerOptions JsonSerializerOptions = new(ConverterOptions.JsonSerializerOptions) {
        AllowTrailingCommas = true,
        IgnoreReadOnlyFields = false,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        RespectNullableAnnotations = true,
        //RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };
}
