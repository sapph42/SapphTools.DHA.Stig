namespace SapphTools.DHA.Stig.Common.Extensions; 
public static class Utf8JsonWriterExtensions {
    public static void NullWriter<T>(this Utf8JsonWriter writer, T? value, Action<T> dispatch) {
        if (value is null) {
            writer.WriteNullValue();
        } else {
            dispatch(value);
        }
    }
    public static void NullWriter<T>(this Utf8JsonWriter writer, Nullable<T> value, Action<T> dispatch) where T : struct {
        if (!value.HasValue) {
            writer.WriteNullValue();
        } else {
            dispatch(value.Value);
        }
    }
    public static void NullWriter<T>(this Utf8JsonWriter writer, T? value, JsonSerializerOptions options) {
        if (value is null) {
            writer.WriteNullValue();
        } else {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
