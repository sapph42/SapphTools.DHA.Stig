namespace SapphTools.DHA.Stig.Common.Attributes;
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class UiDisplayAttribute(string uiDisplay) : Attribute {
    public string UiDisplay { get; } = uiDisplay;
}
