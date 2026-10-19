namespace SapphTools.DHA.Stig.Common.Classes; 
public class RemediatorDto {
    public required Setting Setting { get; init; }
    public Guid RemediationBatch { get; init; }
    public Guid RuleBatch { get; init; }
    public required string RuleId { get; init; }
    public string? Description { get; init; }
    public int SettingIndex { get; init; }
    public ActionSource Source { get; init; }
    public string? ComputerName { get; init; }
    public bool WhatIf { get; init; } = true;
}
