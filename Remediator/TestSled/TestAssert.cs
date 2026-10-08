using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Remediator.TestSled;
public static class TestAssert {
    public static void IsEqual(
            object? left,
            object? right,
            [CallerArgumentExpression(nameof(left))] string valName = "left",
            [CallerArgumentExpression(nameof(right))] string valPattern = "right") {
        if (Equals(left, right)) {
            return;
        }
        ComparisonAssertionObject obj = new(valName, valPattern, left, right);
        throw new TestAssertException(obj, Assertion.IsEqual);
    }
    public static void IsEqual<T>(
            object? left,
            object? right,
            IComparer<T> comparer,
            [CallerArgumentExpression(nameof(left))] string valName = "left",
            [CallerArgumentExpression(nameof(right))] string valPattern = "right") {
        if (left is T lt && right is T rt && comparer.Compare(lt, rt) == 0) {
            return;
        }
        ComparisonAssertionObject obj = new(valName, valPattern, left, right);
        throw new TestAssertException(obj, Assertion.IsEqual);
    }
    public static void IsFalse(bool? value, [CallerArgumentExpression(nameof(value))] string name = "value") {
        if (value is bool and false) {
            return;
        }
        StateAssertionObject obj = new(name, value);
        throw new TestAssertException(obj, Assertion.IsFalse);
    }
    public static void IsNotEqual(
            object? left,
            object? right,
            [CallerArgumentExpression(nameof(left))] string valName = "left",
            [CallerArgumentExpression(nameof(right))] string valPattern = "right") {
        if (!Equals(left, right)) {
            return;
        }
        ComparisonAssertionObject obj = new(valName, valPattern, left, right);
        throw new TestAssertException(obj, Assertion.IsNotEqual);
    }
    public static void IsNotEqual<T>(
            object? left,
            object? right,
            IComparer<T> comparer,
            [CallerArgumentExpression(nameof(left))] string valName = "left",
            [CallerArgumentExpression(nameof(right))] string valPattern = "right") {
        ComparisonAssertionObject obj;
        if (ReferenceEquals(left, right)) {
            obj = new(valName, valPattern, left, right);
            throw new TestAssertException(obj, Assertion.IsNotEqual);
        }
        if (left is not T lt || right is not T rt || comparer.Compare(lt, rt) != 0) {
            return;
        }
        obj = new(valName, valPattern, left, right);
        throw new TestAssertException(obj, Assertion.IsNotEqual);
    }
    public static void IsNotNull(object? value, [CallerArgumentExpression(nameof(value))] string name = "value") {
        if (value is not null) {
            return;
        }
        StateAssertionObject obj = new(name, value);
        throw new TestAssertException(obj, Assertion.IsNotNull);
    }
    public static void IsNull(object? value, [CallerArgumentExpression(nameof(value))] string name = "value") {
        if (value is null) {
            return;
        }
        StateAssertionObject obj = new(name, value);
        throw new TestAssertException(obj, Assertion.IsNull);
    }
    public static void IsTrue(bool? value, [CallerArgumentExpression(nameof(value))] string name = "value") {
        if (value is bool and true) {
            return;
        }
        StateAssertionObject obj = new(name, value);
        throw new TestAssertException(obj, Assertion.IsTrue);
    }
    public static void MatchesRegex(
            string? value, 
            Regex? pattern, 
            [CallerArgumentExpression(nameof(value))] string valName = "value",
            [CallerArgumentExpression(nameof(pattern))] string valPattern = "pattern") {
        if (value is not null && pattern is not null && pattern.Match(value).Success) {
            return;
        }
        DualAssertionObject obj = new(valName, valPattern, value, pattern);
        throw new TestAssertException(obj, Assertion.MatchesRegex);
    }
}
public enum Assertion {
    IsEqual,
    IsFalse,
    IsNotEqual,
    IsNotNull,
    IsNull,
    IsTrue,
    MatchesRegex
}
public interface IAssertionObject {
    Type AssertionObjectType { get; }
}
public struct ValueAssertionObject
        (string name, object? expected = null, object? actual = null) : IAssertionObject {
    [JsonInclude]
    public readonly Type AssertionObjectType => GetType();
    public string Name { get; private set; } = name;
    public object? Expected { get; private set; } = expected;
    public object? Actual { get; private set; } = actual;

    public static explicit operator ValueAssertionObject((string name, object? expected, object? actual) data) =>
        new(data.name, data.expected, data.actual);
}
public struct StateAssertionObject
        (string name, object? value = null) : IAssertionObject {
    [JsonInclude]
    public readonly Type AssertionObjectType => GetType();
    public string Name { get; private set; } = name;
    public object? Value { get; private set; } = value;

    public static explicit operator StateAssertionObject((string name, object? value) data) =>
        new(data.name, data.value);
}
public struct ComparisonAssertionObject
        (string leftName, string rightName, object? leftValue = null, object? rightValue = null) : IAssertionObject {
    [JsonInclude]
    public readonly Type AssertionObjectType => GetType();
    public string LeftName { get; private set; } = leftName;
    public object? LeftValue { get; private set; } = leftValue;
    public string RightName { get; private set; } = rightName;
    public object? RightValue { get; private set; } = rightValue;

    public static explicit operator ComparisonAssertionObject(
            (string leftName, string rightName, object? leftValue, object? rightValue) data) =>
        new(data.leftName, data.rightName, data.leftValue, data.rightValue);
}
public struct DualAssertionObject
        (string valueName, string otherName, object? valueValue = null, object? otherValue = null) : IAssertionObject {
    [JsonInclude]
    public readonly Type AssertionObjectType => GetType();
    public string ValueName { get; private set; } = valueName;
    public object? ValueValue { get; private set; } = valueValue;
    public string OtherName { get; private set; } = otherName;
    public object? OtherValue { get; private set; } = otherValue;

    public static explicit operator DualAssertionObject(
            (string valueName, string otherName, object? valueValue, object? otherValue) data) =>
        new(data.valueName, data.otherName, data.valueValue, data.otherValue);
}
public class TestAssertException : Exception {
    public required IAssertionObject AssertionObject { get; init; }
    public required Assertion Type { get; init; }
    public string? InnerMessage { get; init; }
    [SetsRequiredMembers]
    internal TestAssertException(IAssertionObject assertionObject, Assertion type) : base(BuildMessage(assertionObject, type)) {
        AssertionObject = assertionObject;
        Type = type;
    }
    [SetsRequiredMembers]
    internal TestAssertException(IAssertionObject assertionObject, Assertion type, Exception ex) : 
            base(BuildMessage(assertionObject, type), ex) {
        AssertionObject = assertionObject;
        Type = type;
        InnerMessage = ex.Message;
    }
    private static string BuildMessage(IAssertionObject obj, Assertion type, bool redispatched = false) {
        if (redispatched) {
            throw new NotSupportedException($"{obj.GetType().Name} is not supported ({nameof(obj)}).");
        }
        return BuildMessage((dynamic)obj, type, true);
    }
    private static string BuildMessage(ValueAssertionObject obj, Assertion type, bool dispatched) {
        return $"TestAssert {type} for {obj.Name} failed." +
            $"Expected: <{obj.Expected?.ToString() ?? "null"}>." +
            $"Actual: <{obj.Actual?.ToString() ?? "null"}>.";
    }
    private static string BuildMessage(StateAssertionObject obj, Assertion type, bool dispatched) {
        return $"TestAssert {type} for {obj.Name} failed." +
            $"Value: <{obj.Value?.ToString() ?? "null"}>.";
    }
    private static string BuildMessage(ComparisonAssertionObject obj, Assertion type, bool dispatched) {
        return $"TestAssert {type} failed." +
            $"{obj.LeftName}: <{obj.LeftValue?.ToString() ?? "null"}>." +
            $"{obj.RightName}: <{obj.RightValue?.ToString() ?? "null"}>.";
    }
    private static string BuildMessage(DualAssertionObject obj, Assertion type, bool dispatched) {
        return $"TestAssert {type} for {obj.ValueName} failed." +
            $"Value: <{obj.ValueValue?.ToString() ?? "null"}>." +
            $"{obj.OtherName}: <{obj.OtherValue?.ToString() ?? "null"}>.";
    }
}