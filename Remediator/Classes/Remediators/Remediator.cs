using System.Diagnostics;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal static class Remediator {
    public static List<RemediationActionResult> Execute(Rule rule, Guid batch, string? computerName, bool whatIf = true) {
        Setting[] settings = [..rule.Settings];
        Func<Rule, int, Guid, Guid, string?, bool, RemediationActionResult> dispatch;
        Guid ruleBatch = Guid.NewGuid();
        List<RemediationActionResult> results = [];
        for (int i = 0; i < settings.Length; i++) {
            dispatch = settings[i].Data.Action switch {
                TargetType.RegistryValue => RegistryValueRemediator.Remediate,
                TargetType.RegistryValuePattern => RegistryValuePatternRemediator.Remediate,
                TargetType.RegistryAcl => RegistryAclRemediator.Remediate,
                TargetType.FileSystemAcl => FileAclRemediator.Remediate,
                TargetType.SeRight => LsaRemediator.Remediate,
                TargetType.Sam => SamRemediator.Remediate,
                TargetType.CertStore => CertificateRemediator.Remediate,
                TargetType.RegistryKey => RegistryKeyRemediator.Remediate,
                TargetType.LocalUserAccount => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
            Debug.WriteLine($"Dispatching to remediator {settings[i].Data.Action} for rule {rule.RuleId} on {computerName}");
            results.Add(dispatch(rule, i, batch, ruleBatch, computerName, whatIf));
        }
        return results;
    }
}
internal static class Rollbacker {
    public static RemediationActionResult Execute(Guid batch, RemediationAction logEntry) {
        Func<Guid, Guid, RemediationAction, RemediationActionResult> dispatch;
        Guid ruleBatch = Guid.NewGuid();
        dispatch = logEntry.TargetType switch {
            TargetType.RegistryValue => RegistryValueRemediator.Rollback,
            TargetType.RegistryValuePattern => RegistryValuePatternRemediator.Rollback,
            TargetType.RegistryAcl => RegistryAclRemediator.Rollback,
            TargetType.FileSystemAcl => FileAclRemediator.Rollback,
            TargetType.SeRight => LsaRemediator.Rollback,
            TargetType.Sam => SamRemediator.Rollback,
            TargetType.CertStore => CertificateRemediator.Rollback,
            TargetType.RegistryKey => RegistryKeyRemediator.Rollback,
            TargetType.LocalUserAccount => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
        Debug.WriteLine($"Dispatching to remediator {logEntry.TargetType} for rollback of rule {logEntry.RuleId} on {logEntry.ComputerName}");
        return dispatch(batch, ruleBatch, logEntry);
    }
}