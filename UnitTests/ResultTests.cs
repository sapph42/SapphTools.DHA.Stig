namespace UnitTests;
[TestClass, TestCategory("Pure")]
public sealed class ResultTests {
    [TestMethod] public void DefaultReferenceResultIsSuccessfulWithoutPayload() {
        using Result<string> result = new();
        Assert.IsTrue(result.IsSuccess); Assert.IsFalse(result.HasValue); Assert.IsFalse(result.HasException);
        Assert.IsNull(result.Value); Assert.IsNull(result.Exception);
    }
    [TestMethod] public void NullSentinelIsNotARealPayload() {
        using Result result = new();
        Assert.IsTrue(result.IsSuccess); Assert.IsFalse(result.HasValue);
        Assert.AreSame(Null.Instance, result.Value);
        using Result<Null> generic = new();
        Assert.AreSame(Null.Instance, generic.Value); Assert.IsFalse(generic.HasValue);
    }
    [TestMethod] public void ZeroIsAValidValueTypePayload() {
        using Result<int> result = Result<int>.Success(0);
        Assert.IsTrue(result.IsSuccess); Assert.IsTrue(result.HasValue); Assert.AreEqual(0, result.Value);
    }
    [TestMethod] public void ExplicitNullCanBeSuccessful() {
        using Result<string> result = Result<string>.Success(null);
        Assert.IsTrue(result.IsSuccess); Assert.IsFalse(result.HasValue);
    }
    [TestMethod] public void FailureRetainsTheExactException() {
        InvalidOperationException error = new("causal detail");
        using Result<string> result = Result<string>.Failure(error);
        Assert.IsFalse(result.IsSuccess); Assert.IsTrue(result.HasException); Assert.IsFalse(result.HasValue);
        Assert.AreSame(error, result.Exception); Assert.IsNull(result.Value);
    }
    [TestMethod] public void ImplicitConversionsPreservePayloadAndException() {
        object value = new(); using Result<object> success = value;
        Exception error = new("failure"); using Result<object> failure = error;
        Assert.AreSame(value, success.Value); Assert.IsTrue(success.IsSuccess);
        Assert.AreSame(error, failure.Exception); Assert.IsFalse(failure.IsSuccess);
    }
    [TestMethod] public void TryExecutesFunctionExactlyOnce() {
        int count = 0; using Result<int> result = Result<int>.Try(() => ++count);
        Assert.AreEqual(1, count); Assert.AreEqual(1, result.Value); Assert.IsTrue(result.IsSuccess);
    }
    [TestMethod] public void TryPreservesThrownException() {
        Exception error = new InvalidOperationException("original");
        using Result<int> result = Result<int>.Try(() => throw error);
        Assert.AreSame(error, result.Exception); Assert.IsFalse(result.IsSuccess);
    }
    [TestMethod] public void NonGenericTryRunsAndCapturesFailures() {
        int count = 0; using Result success = Result.Try(() => count++);
        Exception error = new("original"); using Result failure = Result.Try(() => throw error);
        Assert.AreEqual(1, count); Assert.IsTrue(success.IsSuccess); Assert.IsFalse(success.HasValue);
        Assert.AreSame(error, failure.Exception); Assert.IsFalse(failure.IsSuccess);
    }
    [TestMethod] public void MutableResultRecomputesItsFlags() {
        using Result<string> result = new(); result.Value = "payload"; result.Exception = new("failure");
        Assert.IsTrue(result.HasValue); Assert.IsTrue(result.HasException); Assert.IsFalse(result.IsSuccess);
        result.Exception = null; Assert.IsTrue(result.IsSuccess); result.Value = null; Assert.IsFalse(result.HasValue);
    }
    private sealed class DisposableSpy : IDisposable {
        internal int Count;
        public void Dispose() => Count++;
    }
    [TestMethod] public void DisposeOwnsPayloadAndIsIdempotent() {
        DisposableSpy spy = new(); Result<DisposableSpy> result = new(spy);
        result.Dispose(); result.Dispose(); ((IDisposable)result).Dispose();
        Assert.AreEqual(1, spy.Count); Assert.IsNull(result.Value); Assert.IsNull(result.Exception);
    }
    [TestMethod] public void DisposeClearsFailureAndToleratesEmptyResults() {
        Result<string> result = new(new Exception("failure")); result.Dispose(); result.Dispose();
        Assert.IsNull(result.Exception); Assert.IsNull(result.Value);
        new Result<string>().Dispose();
    }
    [DataTestMethod]
    [DataRow(ActionResult.NoActionTaken, true)] [DataRow(ActionResult.NotApplicable, true)]
    [DataRow(ActionResult.ActionSuccess, true)] [DataRow(ActionResult.WhatIf, true)]
    [DataRow(ActionResult.ActionFailure, false)]
    public void ActionResultSuccessUsesActionStatus(ActionResult status, bool success) {
        RemediationAction action = TestData.Action(); action.Result = status; action.FailureMessage = "failed";
        using RemediationActionResult result = new(action);
        Assert.AreEqual(success, result.IsSuccess); Assert.IsTrue(result.HasValue); Assert.IsFalse(result.HasException);
        Assert.AreEqual(success ? "" : "failed", result.Message);
    }
    [TestMethod] public void ActionFailureHasFallbackMessageAndExceptionTakesPrecedence() {
        RemediationAction action = TestData.Action(); action.Result = ActionResult.ActionFailure;
        using RemediationActionResult result = new(action); Assert.AreEqual("Unknown failure", result.Message);
        Exception error = new("exception detail"); result.Exception = error; Assert.AreEqual(error.Message, result.Message);
    }
    [TestMethod] public void IntermediateSuccessHasNoActionOrMessage() {
        using RemediationActionResult result = RemediationActionResult.IntermediateSuccessFactory();
        Assert.IsTrue(result.IsSuccess); Assert.IsFalse(result.HasValue); Assert.AreEqual("", result.Message);
    }
    [TestMethod] public void GenerateWithoutLogCopiesIdentityAndStateWithoutMutatingPreAction() {
        RemediationPreAction pre = TestData.Pre(); RegistryValueValue before = TestData.Value(1), after = TestData.Value(2);
        DateTimeOffset start = DateTimeOffset.UtcNow;
        using RemediationActionResult result = RemediationActionResult.GenerateWithoutLog(pre, TargetType.RegistryValue,
            "target", RollbackCapability.Automatic, before, after, ActionResult.WhatIf);
        RemediationAction action = result.Value!; TestData.SamePre(pre, action);
        Assert.AreEqual("target", action.Target); Assert.AreEqual(TargetType.RegistryValue, action.TargetType);
        Assert.AreEqual(ActionResult.WhatIf, action.Result); Assert.AreEqual(RollbackCapability.Automatic, action.RollbackCapability);
        Assert.AreSame(before, action.Before); Assert.AreSame(after, action.After); Assert.IsNull(action.FailureMessage);
        Assert.IsTrue(action.RemediationTimestamp >= start && action.RemediationTimestamp <= DateTimeOffset.UtcNow);
        pre.ActionNumber++; Assert.AreEqual(7, action.ActionNumber);
    }
}
