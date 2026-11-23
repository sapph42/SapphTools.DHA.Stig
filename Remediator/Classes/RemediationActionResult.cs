using SapphTools.ResultWrapper;

namespace SapphTools.DHA.Stig.Remediator.Classes;

public sealed class RemediationActionResult : Result<RemediationAction> {
    public override bool IsSuccess => _exception is null &&
        (
            _value is null ||
            _value.Result != ActionResult.ActionFailure
        );
    public string Message {
        get {
            if (IsSuccess) {
                return string.Empty;
            }
            if (HasException) {
                return Exception.Message;
            }
            return _value?.FailureMessage ?? "Unknown failure";
        }
    }
    private RemediationActionResult() : base() { }
    public RemediationActionResult(Exception ex) : base(ex) { }
    public RemediationActionResult(RemediationAction res) : base(res) { }

    public static RemediationActionResult GenerateWithoutLog(
            RemediationPreAction preAction,
            TargetType type,
            string target,
            RollbackCapability rollback,
            IValue? before,
            IValue? after,
            ActionResult result) {
        return new RemediationAction(preAction, target) {
            RollbackCapability = rollback,
            After = after,
            Before = before,
            TargetType = type,
            Result = result,
            FailureMessage = null,
            RemediationTimestamp = DateTime.Now,
        };
    }
    public static RemediationActionResult IntermediateSuccessFactory() => new();

    public static implicit operator RemediationActionResult(Exception ex) {
        return new RemediationActionResult(ex);
    }
    public static implicit operator RemediationActionResult(RemediationAction val) {
        return new RemediationActionResult(val);
    }
}