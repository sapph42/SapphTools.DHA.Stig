using System.Security.Cryptography;

namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class ArtifactTests {
    private string directory = null!;
    [TestInitialize] public void Initialize() {
        directory = Path.Combine(Path.GetTempPath(), "StigRemediator.UnitTests." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
    }
    [TestCleanup] public void Cleanup() { if (directory is not null) Directory.Delete(directory, true); }
    private ArtifactCatalog Artifact(string algorithm, byte[] bytes) {
        string path = Path.Combine(directory, "artifact.bin"); File.WriteAllBytes(path, bytes);
        using IncrementalHash hash = IncrementalHash.CreateHash(new(algorithm)); hash.AppendData(bytes);
        // A rooted path avoids writing under Environment.ProcessPath (which may be dotnet.exe).
        return new() { RelativePath = path, Hash = Convert.ToHexString(hash.GetHashAndReset()), HashAlgorithm = algorithm };
    }
    [DataTestMethod] [DataRow("SHA256")] [DataRow("SHA384")] [DataRow("SHA512")]
    public void VerifyAcceptsExactBytesAndRejectsTampering(string algorithm) {
        ArtifactCatalog value = Artifact(algorithm, [0, 1, 2, 255]); Assert.IsTrue(value.Verify());
        File.WriteAllBytes(value.FullPath, [0, 1, 3, 255]); Assert.IsFalse(value.Verify());
    }
    [TestMethod] public void EmptyFileHashAndLowercaseHexAreValid() {
        ArtifactCatalog value = Artifact("SHA256", []); value.Hash = value.Hash.ToLowerInvariant(); Assert.IsTrue(value.Verify());
    }
    [TestMethod] public void MissingFileAndMalformedHexDoNotBecomeSuccessfulVerification() {
        ArtifactCatalog value = Artifact("SHA256", [1]); value.Hash = "invalid-hex";
        Assert.ThrowsException<FormatException>(() => value.Verify());
        File.Delete(value.FullPath); Assert.ThrowsException<FileNotFoundException>(() => value.Verify());
    }
    [TestMethod] public void CorrectlyFormattedWrongDigestIsRejected() {
        ArtifactCatalog value = Artifact("SHA256", [1]); value.Hash = new string('0', 64); Assert.IsFalse(value.Verify());
    }
    [TestMethod] public void CatalogCloneAndFirstBuildCloneRetainDigestWithoutAccessingNetworkShare() {
        ArtifactCatalog source = Artifact("SHA256", [1]); ArtifactCatalog copy = source.Clone();
        Assert.AreNotSame(source, copy); Assert.AreEqual(source.RelativePath, copy.RelativePath);
        Assert.AreEqual(source.Hash, copy.Hash); Assert.AreEqual(source.HashAlgorithm, copy.HashAlgorithm);
        ArtifactFirstBuild build = new() { RelativePath = "local.bin", Hash = source.Hash, HashAlgorithm = "SHA256" };
        ArtifactCatalog builtCopy = build.Clone(); Assert.AreEqual(build.Hash, builtCopy.Hash);
        Assert.AreEqual(HashAlgorithmName.SHA256, build.Algo);
        IArtifact boxed = source; IArtifact interfaceCopy = boxed.Clone(); Assert.AreNotSame(source, interfaceCopy);
        copy.Hash = "changed"; Assert.AreNotEqual(source.Hash, copy.Hash);
    }
    [TestMethod] public void ArtifactJsonRoundTripPreservesDigestAndSkipsUnknownProperties() {
        ArtifactCatalog value = Artifact("SHA256", [1]);
        ArtifactCatalog copy = JsonSerializer.Deserialize<ArtifactCatalog>(JsonSerializer.Serialize(value, TestData.Options()), TestData.Options())!;
        Assert.AreEqual(value.RelativePath, copy.RelativePath); Assert.AreEqual(value.Hash, copy.Hash); Assert.AreEqual(value.HashAlgorithm, copy.HashAlgorithm);
        Assert.IsTrue(copy.Verify());
        ArtifactCatalog withUnknown = JsonSerializer.Deserialize<ArtifactCatalog>(
            """{"Future":{"nested":[1]},"RelativePath":"file","Hash":"00","HashAlgorithm":"SHA256"}""", TestData.Options())!;
        Assert.AreEqual("file", withUnknown.RelativePath);
    }
    [DataTestMethod] [DataRow("{}")] [DataRow("{\"RelativePath\":\"file\",\"Hash\":\"00\"}")]
    [DataRow("{\"RelativePath\":null,\"Hash\":\"00\",\"HashAlgorithm\":\"SHA256\"}")]
    public void IncompleteArtifactHeadersAreRejected(string json) {
        Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<ArtifactCatalog>(json, TestData.Options()));
    }
    [TestMethod] public void CertificateValueCloneOwnsArtifactWithoutOpeningAnyStore() {
        CertificatesValue value = new() { Target = "Root", Artifact = Artifact("SHA256", [1]) };
        CertificatesValue copy = value.Clone(); Assert.AreNotSame(value.Artifact, copy.Artifact);
        Assert.AreEqual(TargetType.CertStore, copy.Action); Assert.AreEqual(@"Local Machine\Root", copy.TargetString);
        copy.Artifact.Hash = "changed"; Assert.AreNotEqual(value.Artifact.Hash, copy.Artifact.Hash);
    }
}
