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
    public bool? TestPass {
        get {
            if (TestSuite.Count == 0 || TestSuite.All(tc => !tc.Pass.HasValue || tc.Result is null)) {
                return null;
            }
            return TestSuite.All(tc => tc.Pass.HasValue && tc.Pass.Value);
        }
    }
    public bool? RecoveryPass {
        get {
            if (RecoverySuite.Count == 0) {
                return true;
            }
            return RecoverySuite.All(tc => tc.Pass.HasValue && tc.Pass.Value);
        }
    }
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