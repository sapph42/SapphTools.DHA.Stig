namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class ServiceRemediator : IRemediator {
    private ServiceRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not ServiceValue val) {
            throw new ArgumentException($"Expected {nameof(target)} to be of type ServiceValue, was {target.GetType().Name}.");
        }
        ServiceSetValue state = new(val);
        try {
            //TODO: The work
            return state;
        } catch {
            return state;
        }
    }
    public static RemediationActionResult Remediate(RemediatorDto dto, Logger? logger = null) {
        throw new NotImplementedException();
    }

    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger) {
        throw new NotImplementedException();
    }
}
