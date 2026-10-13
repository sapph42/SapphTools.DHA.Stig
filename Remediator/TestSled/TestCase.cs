using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public static class TestCaseComparer {
    public class OrdinalComparer : IComparer<TestCase> {
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
    public class TestCaseDefinitionComparer : IEqualityComparer<TestCase> {
        public bool Equals(TestCase? x, TestCase? y) {
            if (ReferenceEquals(x, y)) {
                return true;
            }
            if (x is null || y is null) {
                return false;
            }
            return x.Order == y.Order && x.Rule.Equals(y.Rule);
        }
        public int GetHashCode([DisallowNull] TestCase obj) => obj.GetHashCode();
    }
    public static readonly OrdinalComparer Ordinal = new();
    public static readonly TestCaseDefinitionComparer Definition = new();
}
public class TestCase : IComparable<TestCase>, IEquatable<TestCase> {
    public int Order { get; set; }
    public required Rule Rule { get; init; }
    public TestResult? Result { get; set; }
    public bool? Pass => Result?.Pass;
    public TestCase Clone(bool withoutResults = false) {
        if (withoutResults) {
            return new() {
                Order = Order,
                Rule = Rule.Clone()
            };
        }
        return new() {
            Order = Order,
            Rule = Rule.Clone(),
            Result = Result?.Clone()
        };
    }
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
        return Order == other.Order && Rule.Equals(other?.Rule) && Equals(Pass, other.Pass);
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Order);
        hc.Add(Rule);
        return hc.ToHashCode();
    }
}
