using SapphTools.SecurityDescriptor;

namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class Test : IComparable<Test> {
    public static readonly Guid[] CurrentBuilds = GetBuilds();
    public int TestId { get; set; }
    public int Phase { get; set; }
    public int Order { get; set; }
    public required TestConfig Config { get; set; }
    public Guid[] Build { get; set; } = CurrentBuilds;
    public List<Rule> Rules { get; set; } = [];
    public bool TestRun { get; set; } = false;
    public TestResult? Result { get; set; } = null;
    public bool? TestPass => Result?.Pass;
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