namespace SapphTools.DHA.Stig.Common.Interfaces;

[JsonConverter(typeof(ArtifactCatalogConverter))]
public interface IArtifact : IEquatable<IArtifact> {
    string RelativePath { get; set; }
    string Hash { get; set; }
    string HashAlgorithm { get; }
    IArtifact Clone();
    bool IEquatable<IArtifact>.Equals(IArtifact? other) {
        if (other is null) {
            return false;
        }
        return RelativePath.Equals(other.RelativePath, StringComparison.OrdinalIgnoreCase) &&
            Hash.Equals(other.Hash, StringComparison.OrdinalIgnoreCase) &&
            HashAlgorithm.Equals(other.HashAlgorithm, StringComparison.OrdinalIgnoreCase);
    }
    bool Verify();
}

