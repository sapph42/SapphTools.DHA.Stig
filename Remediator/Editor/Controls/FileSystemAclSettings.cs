using SapphTools.SecurityDescriptor;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class FileSystemAclSettings : ValueSettings, IValueSettings<FileSystemAclValue> {
    public override bool IsValid =>
        !string.IsNullOrWhiteSpace(Target.Text) &&
        !string.IsNullOrWhiteSpace(Permissions.Text) &&
        IsValidPath() &&
        IsValidSddl(out _);
    public override FileSystemAclValue? Value {
        get {
            if (!IsValid) {
                return null;
            }
            return new() {
                Target = Target.Text,
                Sddl = new(Permissions.Text, SecurityDescriptor.Enums.ObjectType.File)
            };
        }
    }
    public FileSystemAclSettings() {
        InitializeComponent();
    }
    public FileSystemAclSettings(FileSystemAclValue? value) : this() {
        if (value is not null) {
            Target.Text = value.Target;
            Permissions.Text = value.Sddl.SddlString;
        }
    }
    public FileSystemAclSettings(Setting? setting) : this() {
        if (setting is not null && setting.Data is FileSystemAclValue value) {
            Target.Text = value.Target;
            Permissions.Text = value.Sddl.SddlString;
        }
    }
    private void Browse_Click(object sender, EventArgs e) {
        FolderBrowserDialog browser = new() {
            AddToRecent = false,
            AutoUpgradeEnabled = true,
            Description = "Target Folder",
            InitialDirectory = @"C:\",
            RootFolder = Environment.SpecialFolder.MyComputer,
            ShowHiddenFiles = true,
            ShowNewFolderButton = false,
            ShowPinnedPlaces = false,
            UseDescriptionForTitle = true,
        };
        if (browser.ShowDialog() == DialogResult.OK) {
            Target.Text = browser.SelectedPath;
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
        FileAclEditor editor;
        if (IsValidSddl(out Sddl? sddl)) {
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
        if (!Path.IsPathFullyQualified(Target.Text)) {
            return false;
        }
        try {
            _ = Path.GetFullPath(Target.Text);
            if (Path.GetPathRoot(Target.Text) is not string root) {
                return false;
            }
            foreach (DriveInfo d in DriveInfo.GetDrives()) {
                if (root.Equals(d.Name, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        } catch (Exception ex) when (
                ex is ArgumentException or
                NotSupportedException or
                PathTooLongException) {
            return false;
        }
    }
    private bool IsValidSddl([NotNullWhen(true)] out Sddl? sddl) {
        sddl = null;
        try {
            sddl = new(Permissions.Text, SecurityDescriptor.Enums.ObjectType.File);
            return true;
        } catch {
            return false;
        }
    }
}
