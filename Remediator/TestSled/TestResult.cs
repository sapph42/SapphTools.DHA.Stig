namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestResult {
    public IValue? ActualAfter { get; set; } = null;
    public List<RemediationAction> RemediationActions { get; set; } = [];
    public TestResult Clone() {
        List<RemediationAction> actions = [];
        foreach (RemediationAction action in RemediationActions ?? []) {
            actions.Add(action.Clone());
        }
        return new() {
            ActualAfter = ActualAfter?.Clone(),
            RemediationActions = actions
        };
    }
}
