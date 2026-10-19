namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public class TestResult {
    public IValue? ActualAfter { get; set; } = null;
    public RemediationAction? RemediationAction { get; set; }
    public TestResult Clone() {
        return new() {
            ActualAfter = ActualAfter?.Clone(),
            RemediationAction = RemediationAction?.Clone()
        };
    }
}
