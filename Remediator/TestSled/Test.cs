using SapphTools.SecurityDescriptor;

namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class Test : IComparable<Test> {
    public static readonly Guid[] CurrentBuilds = GetBuilds();
    public int TestId { get; set; }
    public int Phase { get; set; }
    public int Order { get; set; }
    public required TestConfig Config { get; set; }
    public Guid[] Build { get; set; } = CurrentBuilds;
    public bool TestRun { get; set; } = false;
    public Dictionary<Rule, TestResult?> TestSuite { get; init; } = [];
    public Dictionary<Rule, TestResult?> RecoverySuite { get; init; } = [];
    public bool? TestPass => TestSuite.Values.All(r => r is null) ? null : TestSuite.Values.All(r => r is not null && r.Pass) && TestSuite.Values.Count(r => r is not null)==TestSuite.Keys.Count;
    public bool? RecoveryPass => RecoverySuite.Values.All(r => r is null) ? null : RecoverySuite.Values.All(r => r is not null && r.Pass) && RecoverySuite.Values.Count(r => r is not null) == RecoverySuite.Keys.Count;
    public bool BuildsMatchCurrent => Build.SequenceEqual(CurrentBuilds);

    private static Guid[] GetBuilds() {
        Guid[] build = [
            typeof(Constants).Module.ModuleVersionId,
            typeof(Test).Module.ModuleVersionId,
            typeof(Sddl).Module.ModuleVersionId,
        ];
        return build;
    }
    public int CompareTo(Test? other) {
        if (other is null) return 1;
        int phaseComp = Phase.CompareTo(other.Phase);
        if (phaseComp != 0) {
            return phaseComp;
        }
        return Order.CompareTo(other.Order);
    }
}