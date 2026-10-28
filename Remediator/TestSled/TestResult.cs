namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestResult {
    public IValue? ActualAfter { get; set; } = null;
    public RemediationAction? RemediationAction { get; set; }
    // Null when automatic rollback was not requested; otherwise records its outcome.
    public bool? RollbackPass { get; set; }
    public string? Error { get; set; }
    public string? RollbackError { get; set; }
    public TestResult Clone() {
        return new() {
            ActualAfter = ActualAfter?.Clone(),
            RemediationAction = RemediationAction?.Clone(),
            RollbackPass = RollbackPass,
            Error = Error,
            RollbackError = RollbackError
        };
    }
}
