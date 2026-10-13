using SapphTools.DHA.Stig.Remediator.TestSled.SledCatalog;
using System.Text.Json;

namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestArray {
    public int ArrayVersion { get; set; } = 1;
    public string LocalTarget { get; set; } = Environment.MachineName;
    public List<string> RemoteDevTarget { get; set; } = [];
    public List<string> RemoteProdTarget { get; set; } = [];
    public SortedSet<Test> Tests { get; init; } = new(TestComparer.Ordinal);
    public DateTime SledRunDate {  get; set; } = DateTime.Now;
    public bool Success => Tests.All(t => t.TestRun && t.Pass) && Tests.Count != 0;
    public Test? LastRunTest => Tests.OrderBy(l => l).LastOrDefault(l => l.TestRun);
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
        if (!Enumerable.SequenceEqual(Tests, other.Tests, TestComparer.Definition)) {
            return false;
        }
        return true;
    }
    public TestArray Clone() {
        TestArray ta = new() {
            ArrayVersion = ArrayVersion,
            SledRunDate = SledRunDate,
            LocalTarget = LocalTarget,
            RemoteDevTarget = [.. RemoteDevTarget.ToArray()],
            RemoteProdTarget = [.. RemoteProdTarget.ToArray()],
        };
        foreach (Test test in Tests) {
            ta.Tests.Add(test.Clone());
        }
        return ta;
    }
    public bool ResultsAreValid(string definitionPath) {
        if (LoadFromJson(definitionPath) is not TestArray definition) {
            return false;
        }
        if (!Success || !DefinitionEquals(definition)) {
            return false;
        }
        if (Path.GetDirectoryName(definitionPath) is not string folder) {
            return false;
        }
        string catalogPath = Path.Combine(folder, "TestSled.cat");
        if (!File.Exists(catalogPath)) {
            return false;
        }
        try {
            CatalogApi.VerifyCatalogMember(catalogPath, definitionPath);
            return true;
        } catch {
            return false;
        }
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
