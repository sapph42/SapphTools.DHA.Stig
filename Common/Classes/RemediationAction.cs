namespace SapphTools.DHA.Stig.Common.Classes; 
public class RemediationAction {
    public Guid RemediationBatch { get; set; }
    public required string RuleId { get; set; }
    public string? Description { get; set; }
    public required string ComputerName { get; set; }
    public required int SettingIndex { get; set; }
    public ActionSource Source { get; set; } = ActionSource.Catalog;
    public TargetType TargetType { get; set; }
    public required string Target { get; set; }
    public RollbackCapability RollbackCapability { get; set; }
    public IValue? Before { get; set; }
    public IValue? After { get; set; }
    public ActionResult Result { get; set; }
    public string? FailureMessage { get; set; }
    public DateTimeOffset RemediationTimestamp { get; set; }
}
public class RemediationPreAction {
    public Guid RemediationBatch { get; set; }
    public required string RuleId { get; set; }
    public string? Description { get; set; }
    public required string ComputerName { get; set; }
    public required int SettingIndex { get; set; }
}