namespace SapphTools.DHA.Stig.Common.Enums; 
public enum SettingContext {
    [UiDisplay("None/Any")]
    None,

    [UiDisplay("Administrator")]
    Administrator,

    [UiDisplay("SYSTEM")]
    System,

    [UiDisplay("TrustedInstaller (noop)")]
    TrustedInstaller
}
