namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal class CertificateRemediator : IRemediator {
    public static IValue? GetActualState(IValue target, string computerName) {
        throw new NotImplementedException(); // Think about if this is even possible
    }
    public static RemediationActionResult Remediate(RemediatorDto dto, Logger? logger = null) {
        logger ??= new();
        IValue data = dto.Setting.Data.Clone();
        if (data is not CertificatesValue val) {
            return new ArgumentException($"Expected {nameof(dto)}.Setting.Data to be of type CertificatesValue, was {data.GetType().Name}");
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        return ExecuteRule(pre, val, targetHost, dto.WhatIf, logger);
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger = null) =>
        RemediationActionResult.GenerateWithoutLog(
            logEntry.ToPreAction(),
            logEntry.TargetType,
            logEntry.Target,
            RollbackCapability.NotApplicable,
            logEntry.Before,
            null,
            ActionResult.ActionFailure
        );
    private static RemediationActionResult ExecuteRule(RemediationPreAction pre, CertificatesValue val, string hostName, bool whatIf, Logger logger) {
        if (!val.CatalogArtifact.Verify()) {
            return logger.LogError(
                pre,
                TargetType.CertStore,
                val.Target,
                $"Artifact {val.CatalogArtifact.FullPath} failed validation",
                whatIf
            );
        } else {
            if (whatIf) {
                return logger.LogWhatIf(
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
                return logger.LogNoAction(
                    pre,
                    TargetType.CertStore,
                    val.Target,
                    val
                );
            }
            if (failed == 0) {
                return logger.LogSuccess(
                    pre,
                    TargetType.CertStore,
                    val.Target,
                    RollbackCapability.NotApplicable,
                    null,
                    val
                );
            }
            return logger.LogError(
                pre,
                TargetType.CertStore,
                val.Target,
                $"{success} certificates were added, {failed} certificates failed to add, {noAction} certificates were already present",
                whatIf
            );
        }
    }
}
