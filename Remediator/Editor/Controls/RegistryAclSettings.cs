using Microsoft.Win32;
using SapphTools.SecurityDescriptor;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class RegistryAclSettings : ValueSettings, IValueSettings<RegistryAclValue> {
    public override bool IsValid =>
        !string.IsNullOrWhiteSpace(Target.Text) &&
        !string.IsNullOrWhiteSpace(Permissions.Text) &&
        IsValidPath() &&
        IsValidSddl(out _);
    public override RegistryAclValue? Value {
        get {
            if (!IsValid) {
                return null;
            }
            return new() {
                Target = Target.Text,
                Sddl = new(Permissions.Text, SecurityDescriptor.Enums.ObjectType.RegistryKey)
            };
        }
    }
    public RegistryAclSettings() {
        InitializeComponent();
    }
    public RegistryAclSettings(RegistryAclValue? value) : this() {
        if (value is not null) {
            Target.Text = value.Target;
            Permissions.Text = value.Sddl.SddlString;
        }
    }
    public RegistryAclSettings(Setting? setting) : this() {
        if (setting is not null && setting.Data is RegistryAclValue value) {
            Target.Text = value.Target;
            Permissions.Text = value.Sddl.SddlString;
        }
    }
    private void Browse_Click(object sender, EventArgs e) {
        RegistryBrowserDialog browser = new(Target.Text);
        if (browser.ShowDialog() == DialogResult.OK) {
            Target.Text = browser.SelectedKey;
        }
    }
    private void Permissions_Validating(object sender, CancelEventArgs e) {
        if (!IsValidSddl(out _)) {
            e.Cancel = true;
            ErrorProvider.SetError(Permissions, "Invalid SDDL String");
        } else {
            ErrorProvider.SetError(Permissions, "");
        }
    }
    private void BuildSddl_Click(object sender, EventArgs e) {
        RegistryAclEditor editor;
        if (!string.IsNullOrWhiteSpace(Permissions.Text) && IsValidSddl(out Sddl? sddl)) {
            editor = new(sddl);
        } else {
            editor = new();
        }
        if (editor.ShowDialog() == DialogResult.OK) {
            Permissions.Text = editor.CommittedSddl.SddlString;
        }
    }
    private void Validate_Click(object sender, EventArgs e) {
        if (IsValidSddl(out Sddl? sddl)) {
            Permissions.Text = sddl.SddlString;
        }
    }
    private bool IsValidPath() {
        if (!HivePattern().IsMatch(Target.Text)) {
            return false;
        }
        string hive = HivePattern().Replace(Target.Text, "$1$4$7$9");
        string path = HivePattern().Replace(Target.Text, "$10");
        path = path.TrimEnd('\\');
        if (path.Any(char.IsControl)) {
            return false;
        }
        RegistryHive? regHive = hive switch {
            "HKCR" => RegistryHive.ClassesRoot,
            "HKLM" => RegistryHive.LocalMachine,
            "HKU" => RegistryHive.Users,
            _ => null
        };
        if (regHive is null) {
            return false;
        }
        hive = hive switch {
            "HKCR" => "HKEY_CLASSES_ROOT",
            "HKLM" => "HKEY_LOCAL_MACHINE",
            "HKU" => "HKEY_USERS",
            _ => throw new UnreachableException()
        };
        RegistryKey? key = RegistryKey.OpenBaseKey(regHive.Value, RegistryView.Default);
        try {
            key = key.OpenSubKey(path);
            if (key is not null) {
                Target.Text = key.Name;
            } else {
                Target.Text = hive + '\\' + path;
            }
        } catch { } finally {
            key?.Dispose();
        }
        return true;
    }
    private bool IsValidSddl([NotNullWhen(true)] out Sddl? sddl) {
        sddl = null;
        try {
            sddl = new(Permissions.Text, SecurityDescriptor.Enums.ObjectType.RegistryKey);
            return true;
        } catch {
            return false;
        }
    }
    [GeneratedRegex(@"(HK)((([A-Z]{1,2})(?:[:\\]+))|((?:EY)((?:_)([A-Z])(?:[^_:\\]+))((?:_)([A-Z])(?:[^_:\\]+))?(?:\\)))(\S*)")]
    private partial Regex HivePattern();
}
