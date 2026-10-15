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
            return x.Order == y.Order && 
                x.Setting.Equals(y.Setting) && 
                IValue.Equals(x.ExpectedAfter, y.ExpectedAfter) &&
                x.ExpectedResult == y.ExpectedResult;
        }
        public int GetHashCode([DisallowNull] TestCase obj) => obj.GetHashCode();
    }
    public static readonly OrdinalComparer Ordinal = new();
    public static readonly TestCaseDefinitionComparer Definition = new();
}
public class TestCase : IComparable<TestCase>, IEquatable<TestCase> {
    private IValue? _expectedAfter;
    private Setting? _setting;
    public int Order { get; set; }
    public required Setting Setting {
        get => _setting!;
        init {
            ArgumentNullException.ThrowIfNull(value);
            if (_expectedAfter is not null) {
                Validate(value, _expectedAfter);
            }
            _setting = value;
        }
    }
    public TestResult? Result { get; set; }
    public required IValue ExpectedAfter {
        get => _expectedAfter!;
        init {
            ArgumentNullException.ThrowIfNull(value);
            if (_setting is not null) {
                Validate(_setting, value);
            }
            _expectedAfter = value;
        }
    }
    public required ActionResult ExpectedResult { get; init; }
    public bool? Pass {
        get {
            if (Result is null) {
                return null;
            }
            if (Result.RemediationActions.Count == 0) {
                return false;
            }
            if (!Result.RemediationActions.All(a => a.Result==ExpectedResult)) {
                return false;
            }
            if (!IValue.Equals(ExpectedAfter, Result.ActualAfter)) {
                return false;
            }
            return true;
        }
    }
    public TestCase Clone(bool withoutResults = false) {
        if (withoutResults) {
            return new() {
                Order = Order,
                Setting = Setting.Clone(),
                ExpectedAfter = ExpectedAfter.Clone(),
                ExpectedResult = ExpectedResult
            };
        }
        return new() {
            Order = Order,
            Setting = Setting.Clone(),
            ExpectedAfter = ExpectedAfter.Clone(),
            ExpectedResult = ExpectedResult,
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
        return Order == other.Order && Setting.Equals(other?.Setting) && Equals(Pass, other.Pass);
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Order);
        hc.Add(Setting);
        return hc.ToHashCode();
    }
    private static void Validate(Setting setting, IValue after) {
        if (setting.Data.GetType() != after.GetType()) {
            throw new ArgumentException("Setting.Data and ExpectedAfter must have matching types.");
        }
    }
}
