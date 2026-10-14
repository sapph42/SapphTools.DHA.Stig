using SapphTools.SecurityDescriptor;
using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public static class TestComparer {
    public class OrdinalComparer : IComparer<Test> {
        public int Compare(Test? x, Test? y) {
            if (ReferenceEquals(x, y)) {
                return 0;
            }
            if (x is null) {
                return -1;
            }
            if (y is null) {
                return 1;
            }
            return x.CompareTo(y);
        }
    }
    public class TestDefinitionComparer : IEqualityComparer<Test> {
        public bool Equals(Test? x, Test? y) {
            if (ReferenceEquals(x, y)) {
                return true;
            }
            if (x is null || y is null) {
                return false;
            }
            return x.TestId == y.TestId &&
                x.Phase == y.Phase &&
                x.Order == y.Order &&
                x.Config.Equals(y.Config) &&
                x.TestSuite.SequenceEqual(y.TestSuite, TestCaseComparer.Definition) &&
                x.RecoverySuite.SequenceEqual(y.RecoverySuite, TestCaseComparer.Definition);
        }
        public int GetHashCode([DisallowNull] Test obj) {
            HashCode hc = new();
            hc.Add(obj.TestId);
            hc.Add(obj.Phase);
            hc.Add(obj.Order);
            hc.Add(obj.Config);
            foreach (TestCase tc in obj.TestSuite) {
                hc.Add(tc);
            }
            foreach (TestCase tc in obj.RecoverySuite) {
                hc.Add(tc);
            }
            return hc.ToHashCode();
        }
    }
    public static readonly OrdinalComparer Ordinal = new();
    public static readonly TestDefinitionComparer Definition = new();
}
public class Test : IComparable<Test> {
    public int TestId { get; set; }
    public int Phase { get; set; }
    public int Order { get; set; }
    public required TestConfig Config { get; set; }
    public bool TestRun { get; set; } = false; // maybe change to => TestSuite.Any(t => t.Result is not null) ???
    public SortedSet<TestCase> TestSuite { get; init; } = new(TestCaseComparer.Ordinal);
    public SortedSet<TestCase> RecoverySuite { get; init; } = new(TestCaseComparer.Ordinal);
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
    public bool Pass => TestPass.HasValue && TestPass.Value && RecoveryPass.HasValue && RecoveryPass.Value;
    public Test Clone(bool withoutResults = false) {
        Test t = new() {
            TestId = TestId,
            Phase = Phase,
            Order = Order,
            Config = Config,
            TestRun = !withoutResults && TestRun,
        };
        foreach (TestCase tc in TestSuite) {
            t.TestSuite.Add(tc.Clone(withoutResults));
        }
        foreach (TestCase tc in RecoverySuite) {
            t.RecoverySuite.Add(tc.Clone(withoutResults));
        }
        return t;
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