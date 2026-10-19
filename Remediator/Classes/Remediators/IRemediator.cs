namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
public interface IRemediator {
    static abstract IValue? GetActualState(IValue target, string computerName);
    static abstract RemediationActionResult Remediate(RemediatorDto dto, Logger? logger);
    static abstract RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger);
}
