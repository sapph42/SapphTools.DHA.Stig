using System.Data;
using System.Security.Cryptography;

namespace SapphTools.DHA.Stig.Common.Classes; 
public class ArtifactFirstBuild : IArtifact {
    private string? _algo;
    private static readonly string targetPath = @"\\eamcfs01\dept$\_EAMC_DATA\workgroup\IMD\System Administration\_AdminApps\Stig Remediator\Artifacts";
    public required string RelativePath { get; set; }
    public required string Hash { get; set; }
    public required string HashAlgorithm {
        get => _algo ?? HashAlgorithmName.SHA256.Name!;
        init {
            _algo = value;
            Algo = new(value);
        }
    }
    [JsonIgnore]
    public HashAlgorithmName Algo { get; private set; }

    public static ArtifactFirstBuild Construct(string ruleId, string filePath) => Construct(ruleId, filePath, HashAlgorithmName.SHA256);
    public static ArtifactFirstBuild Construct(string ruleId, string filePath, HashAlgorithmName algo) {
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException();
        }
        if (algo.Name is null) {
            throw new ArgumentException("Provided HashAlgorithmName does not have a valid Name", nameof(algo));
        }
        using IncrementalHash hash = IncrementalHash.CreateHash(algo);
        hash.AppendData(File.ReadAllBytes(filePath));
        byte[] result = hash.GetHashAndReset();
        string hashString = Convert.ToHexString(result);
        string name = hashString[..15];
        string ext = Path.GetExtension(filePath);
        string destFolder = Path.Combine(targetPath, ruleId);
        if (!Directory.Exists(destFolder)) {
            Directory.CreateDirectory(destFolder);
        }
        string fileName = $"{name}{ext}";
        string fileDest = Path.Combine(destFolder, fileName);
        File.Copy(filePath, fileDest, overwrite: true);
        return new() {
            RelativePath = $"{ruleId}\\{fileName}",
            Hash = hashString,
            HashAlgorithm = algo.Name
        };
    }
    public static implicit operator ArtifactCatalog(ArtifactFirstBuild art) => (ArtifactCatalog)art.Clone();
    IArtifact IArtifact.Clone() {
        return new ArtifactCatalog() {
            RelativePath = RelativePath,
            Hash = Hash,
            HashAlgorithm = HashAlgorithm
        };
    }
    public ArtifactCatalog Clone() {
        return new() {
            RelativePath = RelativePath,
            Hash = Hash,
            HashAlgorithm = HashAlgorithm
        };
    }
    public bool Verify() {
        string absPath = Path.Combine(targetPath, RelativePath);
        HashAlgorithmName algo = new (HashAlgorithm);
        using IncrementalHash hash = IncrementalHash.CreateHash(algo);
        hash.AppendData(File.ReadAllBytes(absPath));
        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(Hash), hash.GetHashAndReset());
    }
}
public class ArtifactCatalog : IArtifact {
    private static readonly string localPath = Path.GetDirectoryName(Environment.ProcessPath!)!;
    public required string RelativePath { get; set; }
    public required string Hash { get; set; }
    public required string HashAlgorithm { get; set; }
    public string FullPath => Path.Combine(localPath, RelativePath);
    public static implicit operator ArtifactFirstBuild(ArtifactCatalog art) => (ArtifactFirstBuild)art.Clone();
    IArtifact IArtifact.Clone() {
        return new ArtifactCatalog() {
            RelativePath = RelativePath,
            Hash = Hash,
            HashAlgorithm = HashAlgorithm
        };
    }
    public ArtifactCatalog Clone() {
        return new() {
            RelativePath = RelativePath,
            Hash = Hash,
            HashAlgorithm = HashAlgorithm
        };
    }
    public bool Verify() {
        string absPath = Path.Combine(localPath, RelativePath);
        HashAlgorithmName algo = new (HashAlgorithm);
        using IncrementalHash hash = IncrementalHash.CreateHash(algo);
        hash.AppendData(File.ReadAllBytes(absPath));
        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(Hash), hash.GetHashAndReset());
    }
}