namespace SapphTools.DHA.Stig.Remediator.TestSled; 
public readonly record struct TestConfig {
    public readonly TestTarget Target { get; init; }
    public readonly bool Elevated { get; init; }
    public readonly bool WhatIf { get; init; }
    public readonly bool AttemptRollback { get; init; }
    public bool Equals(TestConfig? other) {
        if (other is not TestConfig oth) {
            return false;
        }
        return Target==oth.Target && Elevated==oth.Elevated && AttemptRollback==oth.AttemptRollback && WhatIf==oth.WhatIf;
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Target);
        hc.Add(Elevated);
        hc.Add(AttemptRollback);
        hc.Add(WhatIf);
        return hc.ToHashCode();
    }
}
