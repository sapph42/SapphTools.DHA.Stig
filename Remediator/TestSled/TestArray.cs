namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestArray {
    public int ArrayVersion { get; set; } = 1;
    public string LocalTarget { get; set; } = Environment.MachineName;
    public List<string> RemoteDevTarget { get; set; } = [];
    public List<string> RemoteProdTarget { get; set; } = [];
    public List<Test> Tests { get; set; } = [];
    public DateTime SledRunDate {  get; set; } = DateTime.Now;
    public bool Success => Tests.All(t => t.TestRun && t.TestPass.HasValue && t.TestPass.Value) && Tests.Count != 0;
    public Test? LastRunTest => Tests.OrderBy(l => l).LastOrDefault(l => l.TestRun);
}
