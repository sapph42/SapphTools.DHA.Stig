namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
public interface IRemediator {
    static abstract void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true);
}
