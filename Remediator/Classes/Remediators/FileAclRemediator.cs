namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators;
internal class FileAclRemediator : IRemediator {
    public static void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            throw new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not FileSystemAclValue val) {
            throw new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type FileSystemAclValue, was {data.GetType().Name}");
        }
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = batch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex
        };
        SetAcl(pre, val, whatIf);
    }
    private static void SetAcl(RemediationPreAction preAction, FileSystemAclValue val, bool whatIf = true) {
        FileAttributes attr = File.GetAttributes(val.Target);
        FileSystemInfo info;
        string target;
        if (preAction.ComputerName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)) {
            target = val.Target;
        } else {
            string? root = Path.GetPathRoot(val.Target);
            if (root is null) {
                Logger.LogError(
                    preAction,
                    TargetType.FileSystemAcl,
                    val.Target,
                    "Path not rooted",
                    whatIf
                );
                return;
            }
            char drive = root[0];
            string leaf = val.Target[2..];
            target = Path.Combine(@"\\" + preAction.ComputerName, drive.ToString() + "$", leaf);
        }
        if (attr.HasFlag(FileAttributes.Directory)) {
            info = new DirectoryInfo(target);
        } else {
            info = new FileInfo(target);
        }
        if (!info.Exists) {
            Logger.LogError(
                preAction,
                TargetType.FileSystemAcl,
                val.Target,
                "Path not found.",
                whatIf
            );
            return;
        }
        try {
            FileSystemAclValue before = new() {
                Target = val.Target,
                Sddl = info.GetSecurityDescriptorSddl()
            };
            if (whatIf) {
                Logger.LogWhatIf(
                    preAction,
                    TargetType.FileSystemAcl,
                    val.Target,
                    RollbackCapability.NotApplicable,
                    before,
                    val
                );
                return;
            }
            info.SetSecurityDescriptorSddlForm(val.Sddl);
            Logger.LogSuccess(
                preAction,
                TargetType.FileSystemAcl,
                val.Target,
                RollbackCapability.Automatic,
                before,
                val
            );
        } catch (Exception ex) {
            Logger.LogError(
                preAction,
                TargetType.FileSystemAcl,
                val.Target,
                $"An exception occured during ACL operations: {ex.Message}",
                whatIf
            );
        }
    }

}
