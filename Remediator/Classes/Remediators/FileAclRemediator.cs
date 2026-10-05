namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal class FileAclRemediator : IRemediator {
    public static RemediationActionResult Remediate(Rule rule, int settingIndex, Guid remBatch, Guid ruleBatch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            return new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not FileSystemAclValue val) {
            return new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type FileSystemAclValue, was {data.GetType().Name}");
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
        return SetAcl(pre, val, whatIf);
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
