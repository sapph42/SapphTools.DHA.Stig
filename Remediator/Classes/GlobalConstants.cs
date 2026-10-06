using System.Text.Json;

namespace SapphTools.DHA.Stig.Remediator.Classes; 
internal static class GlobalConstants {
    public readonly static JsonSerializerOptions JsonSerializerOptions = new(Constants.JsonSerializerOptions) { };
}
