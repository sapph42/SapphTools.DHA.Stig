namespace SapphTools.DHA.Stig.Common.Interfaces;

[JsonConverter(typeof(ArtifactCatalogConverter))]
public interface IArtifact {
    string RelativePath { get; set; }
    string Hash { get; set; }
    string HashAlgorithm { get; }
    IArtifact Clone();
    bool Verify();
}

