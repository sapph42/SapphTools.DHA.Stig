namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal class CertificateRemediator : IRemediator {
    public static void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            throw new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not CertificatesValue val) {
            throw new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type CertificatesValue, was {data.GetType().Name}");
        }
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = batch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex
        };
        foreach (ArtifactCatalog artifact in val.Artifacts.Cast<ArtifactCatalog>()) {
            if (!artifact.Verify()) {
                Logger.LogError(
                    pre,
                    TargetType.CertStore,
                    val.Target,
                    $"Artifact {artifact.FullPath} failed validation",
                    whatIf
                );
            } else {
                if (whatIf) {
                    Logger.LogWhatIf(
                        pre,
                        TargetType.CertStore,
                        val.Target,
                        RollbackCapability.NotApplicable,
                        null,
                        val
                    );
                    return;
                }
                CryptoWinApi.AddCerts(targetHost, val.Target, artifact.FullPath, out int success, out int noAction, out int failed);
                if (success == 0 && failed == 0) {
                    Logger.LogNoAction(
                        pre,
                        TargetType.CertStore,
                        val.Target,
                        val
                    );
                    return;
                }
                if (failed == 0) {
                    Logger.LogSuccess(
                        pre,
                        TargetType.CertStore,
                        val.Target,
                        RollbackCapability.NotApplicable,
                        null,
                        val
                    );
                    return;
                }
                Logger.LogError(
                    pre,
                    TargetType.CertStore,
                    val.Target,
                    $"{success} certificates were added, {failed} certificates failed to add, {noAction} certificates were already present",
                    whatIf
                );
            }
        }
        
    }
}
