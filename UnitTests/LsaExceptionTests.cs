namespace UnitTests;

[TestClass, TestCategory("Pure")]
public sealed class LsaExceptionTests {
    public static IEnumerable<object[]> Reasons() => Enum.GetValues<LsaExceptionReason>().Select(r => new object[] { r });
    [DataTestMethod, DynamicData(nameof(Reasons), DynamicDataSourceType.Method)]
    public void ReasonConstructorsPreserveCauseAndProduceStableMessages(LsaExceptionReason reason) {
        Exception cause = new InvalidOperationException("native failure");
        LsaException value = new("account", reason, cause);
        Assert.AreEqual(reason, value.Reason); Assert.AreSame(cause, value.InnerException);
        Assert.IsTrue(value.Message.Contains("account", StringComparison.Ordinal));
        Assert.AreEqual(value.Message, value.Message);
        Assert.IsFalse(string.IsNullOrWhiteSpace(new LsaException(reason).Message));
        Assert.AreSame(cause, new LsaException(reason, cause).InnerException);
        Assert.IsFalse(new LsaException(null, reason).Message.Contains("Parameter name:", StringComparison.Ordinal));
    }

    [DataTestMethod, TestCategory("KnownRegression"), DataRow(false), DataRow(true)]
    public void ExplicitMessageMustNotBeDiscarded(bool withCause) {
        Exception cause = new InvalidOperationException("native failure");
        LsaException value = withCause ? new("account", "specific failure", LsaExceptionReason.OpenPolicy, cause) :
            new("account", "specific failure", LsaExceptionReason.OpenPolicy);
        Assert.IsTrue(value.Message.Contains("specific failure", StringComparison.Ordinal));
        Assert.IsTrue(value.Message.Contains("account", StringComparison.Ordinal));
        Assert.AreEqual(LsaExceptionReason.OpenPolicy, value.Reason);
        Assert.AreSame(withCause ? cause : null, value.InnerException);
    }
}
