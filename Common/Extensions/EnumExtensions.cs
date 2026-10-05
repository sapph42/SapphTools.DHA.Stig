namespace SapphTools.DHA.Stig.Common.Extensions; 
public static class EnumEx {
    public static T Max<T>(params T[] values) where T : struct, Enum {
        if (values is null || values.Length == 0) {
            return default;
        }
        T max = values[0];
        for (int i = 1; i < values.Length; i++) {
            if (values[i].CompareTo(max) > 0) {
                max = values[i];
            }
        }
        return max;
    }
}
