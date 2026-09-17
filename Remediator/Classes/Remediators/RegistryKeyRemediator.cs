using Microsoft.Win32;
using static SapphTools.DHA.Stig.Remediator.Classes.RegistryCommon;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal class RegistryKeyRemediator : IRemediator {
    private RegistryKeyRemediator() { }
    public static void Remediate(Rule rule, int settingIndex, Guid batch, string? computerName, bool whatIf = true) {
        if (!rule.Settings.Where(s => s.Order == settingIndex).Any()) {
            throw new ArgumentException($"No such setting exists at order index {settingIndex}");
        }
        Setting setting = rule.Settings.Where(s => s.Order == settingIndex).First();
        IValue data = setting.Data;
        if (data is not RegistryKeyValue val) {
            throw new ArgumentException($"Expected {nameof(rule)}.Settings[{nameof(settingIndex)}].Data to be of type RegistryKeyValue, was {data.GetType().Name}");
        }
        string targetHost = computerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = batch,
            RuleId = rule.RuleId,
            Description = rule.Description,
            ComputerName = targetHost,
            SettingIndex = settingIndex
        };
        if (GetHive(val.Target) is not RegistryHive hive) {
            Logger.LogError(pre, TargetType.RegistryKey, val.Target, "Key path was not well-formed.", whatIf);
            return;
        }
        if (TryGetHiveKey(pre, hive, out RegistryKey? hiveKey, whatIf)) {
            _ = RegistryValueRemediator.CreateRegistryKey(hiveKey, val.Target, pre, false, whatIf);
        }
    }
}
