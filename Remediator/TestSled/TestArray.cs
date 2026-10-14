using SapphTools.DHA.Stig.Remediator.TestSled.SledCatalog;
using SapphTools.SecurityDescriptor;
using System.Collections.Immutable;
using System.Text.Json;

namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestArray {
    private ImmutableArray<Guid> _build = [..CurrentBuilds];
    public static readonly ImmutableArray<Guid> CurrentBuilds = [.. GetBuilds()];
    public int ArrayVersion { get; set; } = 1;
    public string LocalTarget { get; set; } = Environment.MachineName;
    public List<string> RemoteDevTarget { get; set; } = [];
    public List<string> RemoteProdTarget { get; set; } = [];
    public Guid[] Build {
        get => [.._build];
        init => _build = [..value];
    }
    public SortedSet<Test> Tests { get; init; } = new(TestComparer.Ordinal);
    public DateTime SledRunDate {  get; set; } = DateTime.Now;
    public bool Success => Tests.All(t => t.TestRun && t.Pass) && Tests.Count != 0;
    public Test? LastRunTest => Tests.OrderBy(l => l).LastOrDefault(l => l.TestRun);
    public bool BuildsMatchCurrent => Build.SequenceEqual(CurrentBuilds);
    public bool DefinitionEquals(TestArray? other) {
        if (other is null) {
            return false;
        }
        if (ArrayVersion != other.ArrayVersion ||
                !string.Equals(LocalTarget, other.LocalTarget, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        if (!RemoteDevTarget.Order(StringComparer.OrdinalIgnoreCase).SequenceEqual(other.RemoteDevTarget.Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase)) {
            return false;
        }
        if (!RemoteProdTarget.Order(StringComparer.OrdinalIgnoreCase).SequenceEqual(other.RemoteProdTarget.Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase)) {
            return false;
        }
        if (!Build.SequenceEqual(other.Build)) {
            return false;
        }
        if (!Enumerable.SequenceEqual(Tests, other.Tests, TestComparer.Definition)) {
            return false;
        }
        return true;
    }
    public TestArray Clone(bool withoutResults = false, bool updateBuilds = false) {
        if (updateBuilds && !withoutResults) {
            throw new ArgumentException("Updating build IDs also requires discarding execution results.");
        }
        TestArray ta = new() {
            ArrayVersion = ArrayVersion,
            SledRunDate = SledRunDate,
            LocalTarget = LocalTarget,
            RemoteDevTarget = [.. RemoteDevTarget.ToArray()],
            RemoteProdTarget = [.. RemoteProdTarget.ToArray()],
            Build = updateBuilds ? [.. CurrentBuilds] : [.. Build],
        };
        foreach (Test test in Tests) {
            ta.Tests.Add(test.Clone(withoutResults));
        }
        return ta;
    }
    public bool ResultsAreValid(string definitionPath) {
        if (LoadFromJson(definitionPath) is not TestArray definition) {
            return false;
        }
        if (!Success || definition.Tests.Count == 0 ||
                !definition.BuildsMatchCurrent ||
                !DefinitionEquals(definition)) {
            return false;
        }
        if (Path.GetDirectoryName(definitionPath) is not string folder) {
            return false;
        }
        string catalogPath = Path.Combine(folder, "TestSled.cat");
        if (!File.Exists(catalogPath)) {
            return false;
        }
        return CatalogApi.VerifyCatalogFile(catalogPath, definitionPath);
    }
    public void UpdateBuilds() {
        _build = [..CurrentBuilds];
    }
    private static Guid[] GetBuilds() {
        Guid[] build = [
            typeof(Constants).Module.ModuleVersionId,
            typeof(Test).Module.ModuleVersionId,
            typeof(Sddl).Module.ModuleVersionId,
        ];
        return build;
    }
    public static TestArray? LoadFromJson(string? path) {
        if (path is null) {
            return null;
        }
        try {
            if (!File.Exists(path)) {
                return null;
            }
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<TestArray>(json, GlobalConstants.JsonSerializerOptions);
        } catch {
            return null;
        }
    }
}
