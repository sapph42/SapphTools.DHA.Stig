namespace SapphTools.DHA.Stig.Common.Classes; 
public class Rule : IEquatable<Rule> {
    [JsonPropertyName("RuleId")]
    public required string RuleId { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Settings")]
    public required SettingsSet Settings { get; set; }
    public Rule Clone() {
        SettingsSet clonedSettings = [];
        foreach (Setting setting in Settings) {
            clonedSettings.Add(setting.Clone());
        }
        return new() {
            RuleId = RuleId,
            Description = Description,
            Settings = clonedSettings
        };
    }

    public bool Equals(Rule? other) {
        if (other is null) {
            return false;
        }
        if (ReferenceEquals(this, other)) {
            return true;
        }
        if (!RuleId.Equals(other.RuleId, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        if (Description is null && other.Description is not null) {
            return false;
        }
        if (Description is not null && !Description.Equals(other.Description, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        return Settings.SetEquals(other.Settings);
    }
    public override bool Equals(object? obj) => Equals(obj as Rule);
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(RuleId.ToLowerInvariant());
        hc.Add(Description?.ToLowerInvariant());
        foreach (Setting setting in Settings) {
            hc.Add(setting.GetHashCode());
        }
        return hc.ToHashCode();
    }
}
