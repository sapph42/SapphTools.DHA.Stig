namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public readonly record struct TestConfig {
    public readonly TestTarget Target { get; init; }
    public readonly bool Elevated { get; init; }
    public readonly bool WhatIf { get; init; }
    public readonly bool AttemptRollback { get; init; }
}
