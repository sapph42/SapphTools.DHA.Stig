namespace SapphTools.DHA.Stig.Common.Enums; 
public enum TargetType {
    [UiDisplay("Registry Key Only (No Value)")]
    RegistryKey,

    [UiDisplay("Registry Value")]
    RegistryValue,

    [UiDisplay("Registry Value Pattern")]
    RegistryValuePattern,

    [UiDisplay("Registry ACL")]
    RegistryAcl,

    [UiDisplay("File System ACL")]
    FileSystemAcl,

    [UiDisplay("Local Security Policy")]
    SeRight,

    [UiDisplay("Local Account Settings")]
    Sam,

    [UiDisplay("Local User Management")]
    LocalUserAccount,

    [UiDisplay("Certificate Store Baseline")]
    CertStore
}
