using SapphTools.DHA.Stig.Common.Attributes;
using SapphTools.SecurityDescriptor.Extensions;


namespace SapphTools.DHA.Stig.Common.Extensions; 
public static class UiDisplayExtensions {
    public static string GetUiDisplay(this Enum enumVal) {
        UiDisplayAttribute? display = enumVal.GetAttributeOfType<UiDisplayAttribute>();
        return display is null ? throw new InvalidOperationException("Enum value does not have a MetaAttribute") : display.UiDisplay;
    }
    public static IEnumerable<string> GetAllUiDisplay<T>() where T : Enum {
        foreach (T e in Enum.GetValues(typeof(T))) {
            UiDisplayAttribute? display = e.GetAttributeOfType<UiDisplayAttribute>();
            if (display is null) {
                continue;
            }
            yield return display.UiDisplay;
        }
    }
}
