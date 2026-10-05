namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
public interface IRemediator {
    static abstract RemediationActionResult Remediate(Rule rule, int settingIndex, Guid remBatch, Guid ruleBatch, string? computerName, bool whatIf = true);
    static abstract RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry);
}
