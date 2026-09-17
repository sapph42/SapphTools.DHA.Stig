namespace SapphTools.DHA.Stig.Common.Classes; 
public readonly struct BlacklistedPlugin {
    public required string RuleId { get; init; }
    public required string Reason { get; init; }
}
