using System;
using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Common.Classes; 
public class RemediationAction : RemediationPreAction {
    public TargetType TargetType { get; set; }
    public required string Target { get; set; }
    public RollbackCapability RollbackCapability { get; set; }
    public IValue? Before { get; set; }
    public IValue? After { get; set; }
    public ActionResult Result { get; set; }
    public string? FailureMessage { get; set; }
    public DateTimeOffset RemediationTimestamp { get; set; }
    public RemediationAction() { }

    [SetsRequiredMembers]
    public RemediationAction(RemediationPreAction action, string target) : base(action) { 
        Target = target;
    }
    public RemediationAction Clone() {
        return new(ToPreAction(), Target) {
            TargetType = TargetType,
            RollbackCapability = RollbackCapability,
            Before = Before?.Clone(),
            After = After?.Clone(),
            Result = Result,
            FailureMessage = FailureMessage,
            RemediationTimestamp = RemediationTimestamp
        };
    }
    public RemediationPreAction ToPreAction() {
        return new() {
            RemediationBatch = new(RemediationBatch.ToByteArray()),
            RuleBatch = new(RuleBatch.ToByteArray()),
            SettingBatch = new(SettingBatch.ToByteArray()),
            ActionNumber = ActionNumber,
            Source = Source,
            RuleId = RuleId,
            Description = Description,
            ComputerName = ComputerName,
            SettingIndex = SettingIndex
        };
    }
}
public class RemediationPreAction {
    public Guid RemediationBatch { get; set; }
    public Guid RuleBatch { get; set; }
    public Guid SettingBatch { get; set; }
    public int ActionNumber { get; set; } = 0;
    public ActionSource Source { get; set; } = ActionSource.Catalog;
    public required string RuleId { get; set; }
    public string? Description { get; set; }
    public required string ComputerName { get; set; }
    public required int SettingIndex { get; set; }
    public RemediationPreAction() { }
    [SetsRequiredMembers]
    public RemediationPreAction(RemediationPreAction pre) {
        RemediationBatch = pre.RemediationBatch;
        RuleBatch = pre.RuleBatch;
        SettingBatch = pre.SettingBatch;
        ActionNumber = pre.ActionNumber;
        Source = pre.Source;
        RuleId = pre.RuleId;
        Description = pre.Description;
        ComputerName = pre.ComputerName;
        SettingIndex = pre.SettingIndex;
    }
}