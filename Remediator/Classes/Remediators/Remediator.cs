using System.Diagnostics;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal static class Remediator {
    public static void Execute(Rule rule, Guid batch, string? computerName, bool whatIf = true) {
        Setting[] settings = [..rule.Settings];
        Action<Rule, int, Guid, string?, bool> dispatch;
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
            dispatch(rule, i, batch, computerName, whatIf);
        }
    }
}
