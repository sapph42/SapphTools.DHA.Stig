namespace SapphTools.DHA.Stig.Remediator.TestSled;
[Flags]
public enum TestSledState {
    Valid              = 0x0000,
    NoDefinition       = 0x0001,
    NoResults          = 0x0002,
    DefinitionMismatch = 0x0004,
    VersionMismatch    = 0x0008,
    ResultsFail        = 0x0010,
    NoCatalog          = 0x0020,
    Unsigned           = 0x0040
}
