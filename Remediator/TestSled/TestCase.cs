namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestCase : IComparable<TestCase>, IEquatable<TestCase> {
    public class TestCaseComparer : IComparer<TestCase> {
        public int Compare(TestCase? x, TestCase? y) {
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
    public static readonly TestCaseComparer Comparer = new();
    public int Order { get; init; }
    public required Rule Rule { get; init; }
    public TestResult? Result { get; set; }
    public bool? Pass => Result?.Pass;
    public int CompareTo(TestCase? other) {
        return Order.CompareTo(other?.Order);
    }
    public override bool Equals(object? obj) {
        return Equals(obj as TestCase);
    }
    public bool Equals(TestCase? other) {
        if (other is null) {
            return false;
        }
        return Order == other.Order && Rule.Equals(other?.Rule) && object.Equals(Pass, other.Pass);
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Order);
        hc.Add(Rule);
        hc.Add(Pass);
        return hc.ToHashCode();
    }
}
