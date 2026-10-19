namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal class FileAclRemediator : IRemediator {
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not FileSystemAclValue val) {
            throw new ArgumentException($"Expected {target} to be of type FileSystemAclValue, was {target.GetType().Name}");
        }
        FileSystemInfo info;
        string targetPath;
        if (computerName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)) {
            targetPath = val.Target;
        } else {
            string? root = Path.GetPathRoot(val.Target);
            if (root is null) {
                return null;
            }
            char drive = root[0];
            string leaf = val.Target[2..];
            targetPath = Path.Combine(@"\\" + computerName, drive.ToString() + "$", leaf);
        }
        FileAttributes attr = File.GetAttributes(targetPath);
        if (attr.HasFlag(FileAttributes.Directory)) {
            info = new DirectoryInfo(targetPath);
        } else {
            info = new FileInfo(targetPath);
        }
        if (!info.Exists) {
            return null;
        }
        try {
            return new FileSystemAclValue() {
                Target = val.Target,
                Sddl = info.GetSecurityDescriptorSddl()
            };
        } catch {
            return null;
        }
    }
    public static RemediationActionResult Remediate(RemediatorDto dto) {
        IValue data = dto.Setting.Data.Clone();
        if (data is not FileSystemAclValue val) {
            return new ArgumentException($"Expected {nameof(dto)}.Setting.Data to be of type FileSystemAclValue, was {data.GetType().Name}");
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        return SetAcl(pre, val, dto.WhatIf);
    }
    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry) {
        if (logEntry.Before is null) {
            return new ArgumentException("Cannot rollback a log entry with a null Before property.", nameof(logEntry));
        }
        IValue data = logEntry.Before;
        if (data is not FileSystemAclValue val) {
            return new ArgumentException($"Expected {nameof(logEntry)}.Before to be of type FileSystemAclValue, was {data.GetType().Name}");
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = logEntry.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = remBatch,
            RuleBatch = ruleBatch,
            SettingBatch = settingBatch,
            RuleId = logEntry.RuleId,
            Description = logEntry.Description,
            ComputerName = targetHost,
            SettingIndex = logEntry.SettingIndex,
            Source = ActionSource.Catalog
        };
        return SetAcl(pre, val, whatIf: false);
    }
    private static RemediationActionResult SetAcl(RemediationPreAction preAction, FileSystemAclValue val, bool whatIf = true) {
        FileSystemInfo info;
        string target;
        if (preAction.ComputerName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)) {
            target = val.Target;
        } else {
            string? root = Path.GetPathRoot(val.Target);
            if (root is null) {
                return Logger.LogError(
                    preAction,
                    TargetType.FileSystemAcl,
                    val.Target,
                    "Path not rooted",
                    whatIf
                );
            }
            char drive = root[0];
            string leaf = val.Target[2..];
            target = Path.Combine(@"\\" + preAction.ComputerName, drive.ToString() + "$", leaf);
        }
        FileAttributes attr = File.GetAttributes(target);
        if (attr.HasFlag(FileAttributes.Directory)) {
            info = new DirectoryInfo(target);
        } else {
            info = new FileInfo(target);
        }
        if (!info.Exists) {
            return Logger.LogError(
                preAction,
                TargetType.FileSystemAcl,
                val.Target,
                "Path not found.",
                whatIf
            );
        }
        try {
            FileSystemAclValue before = new() {
                Target = val.Target,
                Sddl = info.GetSecurityDescriptorSddl()
            };
            if (before.Sddl.Equals(val.Sddl, compareActiveOnly:true)) {
                return Logger.LogNoAction(
                    preAction,
                    TargetType.FileSystemAcl,
                    val.Target,
                    before
                );
            }
            if (whatIf) {
                return Logger.LogWhatIf(
                    preAction,
                    TargetType.FileSystemAcl,
                    val.Target,
                    RollbackCapability.NotApplicable,
                    before,
                    val
                );
            }
            info.SetSecurityDescriptorSddlForm(val.Sddl);
            FileSystemAclValue after = new() {
                Target = val.Target,
                Sddl = info.GetSecurityDescriptorSddl()
            };
            return Logger.LogSuccess(
                preAction,
                TargetType.FileSystemAcl,
                val.Target,
                RollbackCapability.Automatic,
                before,
                after
            );
        } catch (Exception ex) {
            return Logger.LogError(
                preAction,
                TargetType.FileSystemAcl,
                val.Target,
                $"An exception occured during ACL operations: {ex.Message}",
                whatIf
            );
        }
    }

}
