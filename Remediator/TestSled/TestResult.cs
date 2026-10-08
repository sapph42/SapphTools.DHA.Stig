namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestResult {
    public IValue? ActualAfter { get; set; } = null;
    public IValue? ExpectedAfter { get; init; } = null;
    public required ActionResult ExpectedResult { get; init; }
    public RemediationAction? RemediationAction { get; set; }
    public bool Pass {
        get {
            if (RemediationAction is null) {
                return false;
            }
            if (RemediationAction.Result != ExpectedResult) {
                return false;
            }
            if (ExpectedAfter is null && RemediationAction.After is null && ActualAfter is null) {
                return true;
            }
            if (ExpectedAfter is null || RemediationAction.After is null || ActualAfter is null) {
                return false;
            }
            if (!ExpectedAfter.GetType().Equals(RemediationAction.After.GetType())) {
                return false;
            }
            return ExpectedAfter.Equals(RemediationAction.After) && ExpectedAfter.Equals(ActualAfter);
        }
    }
}
