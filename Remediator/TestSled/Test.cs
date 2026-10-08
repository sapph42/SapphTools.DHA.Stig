using SapphTools.SecurityDescriptor;

namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class Test : IComparable<Test> {
    public static readonly Guid[] CurrentBuilds = GetBuilds();
    public int TestId { get; set; }
    public int Phase { get; set; }
    public int Order { get; set; }
    public required TestConfig Config { get; set; }
    public Guid[] Build { get; init; } = CurrentBuilds;
    public bool TestRun { get; set; } = false; // maybe change to => TestSuite.Any(t => t.Result is not null) ???
    public SortedSet<TestCase> TestSuite { get; init; } = new(TestCase.Comparer);
    public SortedSet<TestCase> RecoverySuite { get; init; } = new(TestCase.Comparer);
    public bool? TestPass => TestSuite.All(r => r is null)
        ? null
        : TestSuite.All(r => r.Pass.HasValue && r.Pass.Value);
    public bool? RecoveryPass => RecoverySuite.Count == 0 ||
        RecoverySuite.All(r => r is null)
            ? null
            : RecoverySuite.All(r => r.Pass.HasValue && r.Pass.Value);
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