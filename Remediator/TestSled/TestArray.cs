namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestArray {
    public int ArrayVersion { get; set; } = 1;
    public string LocalTarget { get; set; } = Environment.MachineName;
    public List<string> RemoteDevTarget { get; set; } = [];
    public List<string> RemoteProdTarget { get; set; } = [];
    public SortedSet<Test> Tests { get; init; } = new(Test.Comparer);
    public DateTime SledRunDate {  get; set; } = DateTime.Now;
    public bool Success => Tests.All(t => t.TestRun && t.TestPass.HasValue && t.TestPass.Value && t.RecoveryPass.HasValue && t.RecoveryPass.Value) && Tests.Count != 0;
    public Test? LastRunTest => Tests.OrderBy(l => l).LastOrDefault(l => l.TestRun);
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
}
