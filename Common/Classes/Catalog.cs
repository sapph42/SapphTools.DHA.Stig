namespace SapphTools.DHA.Stig.Common.Classes; 
public class Catalog {
    public required string Name { get; set; }
    public required int SchemaVersion { get; set; }
    public List<BlacklistedPlugin> Blacklist { get; set; } = [];
    public HashSet<Rule> Rules { get; set; } = [];
    public Catalog Clone() {
        HashSet<Rule> clonedRules = [];
        foreach (Rule rule in Rules) {
            clonedRules.Add(rule.Clone());
        }
        List<BlacklistedPlugin> blacklist = [];
        foreach (BlacklistedPlugin plugin in Blacklist) {
            blacklist.Add(plugin);
        }
        return new() {
            Name = Name,
            SchemaVersion = SchemaVersion,
            Blacklist = blacklist,
            Rules = clonedRules
        };
    }
}
