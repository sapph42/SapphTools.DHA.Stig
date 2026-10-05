namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal class CertificateRemediator : IRemediator {
    public static RemediationActionResult Remediate(Rule rule, int settingIndex, Guid remBatch, Guid ruleBatch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            return new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not CertificatesValue val) {
            return new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type CertificatesValue, was {data.GetType().Name}");
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = remBatch,
            RuleBatch = ruleBatch,
            SettingBatch = settingBatch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex,
            Source = ActionSource.Catalog
        };
        return ExecuteRule(pre, val, targetHost, whatIf);
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) =>
        RemediationActionResult.GenerateWithoutLog(
            logEntry.ToPreAction(),
            logEntry.TargetType,
            logEntry.Target,
            RollbackCapability.NotApplicable,
            logEntry.Before,
            null,
            ActionResult.ActionFailure
        );
    private static RemediationActionResult ExecuteRule(RemediationPreAction pre, CertificatesValue val, string hostName, bool whatIf) {
        if (!val.CatalogArtifact.Verify()) {
            return Logger.LogError(
                pre,
                TargetType.CertStore,
                val.Target,
                $"Artifact {val.CatalogArtifact.FullPath} failed validation",
                whatIf
            );
        } else {
            if (whatIf) {
                return Logger.LogWhatIf(
                    pre,
                    TargetType.CertStore,
                    val.Target,
                    RollbackCapability.NotApplicable,
                    null,
                    val
                );
            }
            CryptoWinApi.AddCerts(hostName, val.Target, val.CatalogArtifact.FullPath, out int success, out int noAction, out int failed);
            if (success == 0 && failed == 0) {
                return Logger.LogNoAction(
                    pre,
                    TargetType.CertStore,
                    val.Target,
                    val
                );
            }
            if (failed == 0) {
                return Logger.LogSuccess(
                    pre,
                    TargetType.CertStore,
                    val.Target,
                    RollbackCapability.NotApplicable,
                    null,
                    val
                );
            }
            return Logger.LogError(
                pre,
                TargetType.CertStore,
                val.Target,
                $"{success} certificates were added, {failed} certificates failed to add, {noAction} certificates were already present",
                whatIf
            );
        }
    }
}
