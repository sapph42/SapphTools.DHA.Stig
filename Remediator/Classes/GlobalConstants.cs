using SapphTools.DHA.Stig.Remediator.TestSled;
using System.Text.Json;

namespace SapphTools.DHA.Stig.Remediator.Classes; 
internal static class GlobalConstants {
    public readonly static JsonSerializerOptions JsonSerializerOptions = new(Constants.JsonSerializerOptions) { };
    static GlobalConstants() {
        JsonSerializerOptions.Converters.Add(new AssertionObjectConverter());
    }
}
