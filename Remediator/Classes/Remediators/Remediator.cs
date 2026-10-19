using SapphTools.DHA.Stig.Remediator.TestSled;
using System.Diagnostics;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal static class Remediator {
    public static List<RemediationActionResult> Execute(Rule rule, Guid batch, string? computerName, bool whatIf = true, ActionSource source = ActionSource.Catalog, Logger? logger = null) {
        Setting[] settings = [..rule.Settings];
        Func<RemediatorDto, Logger?, RemediationActionResult> dispatch;
        Guid ruleBatch = Guid.NewGuid();
        List<RemediationActionResult> results = [];
        for (int i = 0; i < settings.Length; i++) {
            RemediatorDto dto = new() {
                Setting = settings[i].Clone(),
                RemediationBatch = batch,
                RuleBatch = ruleBatch,
                RuleId = rule.RuleId,
                Description = rule.Description,
                SettingIndex = i,
                Source = source,
                ComputerName = computerName,
                WhatIf = whatIf
            };
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
            results.Add(dispatch(dto, logger));
        }
        return results;
    }
    public static RemediationActionResult ExecuteTestCase(TestCase testCase, Guid sledBatch, string computerName, string ruleId, string description, bool whatIf, Logger logger) {
        Func<RemediatorDto, Logger, RemediationActionResult> dispatch;
        Guid testCaseBatch = Guid.NewGuid();
        RemediatorDto dto = new() {
            Setting = testCase.Setting.Clone(),
            RemediationBatch = sledBatch,
            RuleBatch = testCaseBatch,
            RuleId = ruleId,
            Description = description,
            SettingIndex = 0,
            Source = ActionSource.TestSled,
            ComputerName = computerName,
            WhatIf = whatIf
        };
        dispatch = dto.Setting.Data.Action switch {
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
        Debug.WriteLine($"Dispatching to remediator {dto.Setting.Data.Action} for TestCase {dto.RuleId} on {computerName}");
        return dispatch(dto, logger);
    }
    public static IValue? GetActualState(IValue target, string computerName) {
        Func<IValue, string, IValue?> dispatch = target.Action switch {
            TargetType.RegistryKey => RegistryKeyRemediator.GetActualState,
            TargetType.RegistryValue => RegistryValueRemediator.GetActualState,
            TargetType.RegistryValuePattern => throw new NotImplementedException(),
            TargetType.RegistryAcl => RegistryAclRemediator.GetActualState,
            TargetType.FileSystemAcl => FileAclRemediator.GetActualState,
            TargetType.SeRight => LsaRemediator.GetActualState,
            TargetType.Sam => SamRemediator.GetActualState,
            TargetType.LocalUserAccount => throw new NotImplementedException(),
            TargetType.CertStore => throw new NotImplementedException(),
        };
        return dispatch(target, computerName);
    }
}
internal static class Rollbacker {
    public static RemediationActionResult Execute(Guid batch, RemediationAction logEntry, Logger? logger = null) {
        Func<Guid, Guid, RemediationAction, Logger?, RemediationActionResult> dispatch;
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
        return dispatch(batch, ruleBatch, logEntry, logger);
    }
}