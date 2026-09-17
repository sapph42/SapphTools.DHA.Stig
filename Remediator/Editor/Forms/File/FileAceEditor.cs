using SapphTools.DHA.Stig.Remediator.Editor.Forms.Common;
using SapphTools.SecurityDescriptor.Classes;
using SapphTools.SecurityDescriptor.Enums;
using System.Security.Principal;
using SddlRight = SapphTools.SecurityDescriptor.Classes.Right;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.File;
public partial class FileAceEditor : Form {
    private const uint FILE_LIST_DIRECTORY   = 0x00000001;
    private const uint FILE_ADD_FILE         = 0x00000002;
    private const uint FILE_ADD_SUBDIRECTORY = 0x00000004;
    private const uint FILE_READ_EA          = 0x00000008;
    private const uint FILE_WRITE_EA         = 0x00000010;
    private const uint FILE_TRAVERSE         = 0x00000020;
    private const uint FILE_DELETE_CHILD     = 0x00000040;
    private const uint FILE_READ_ATTRIBUTES  = 0x00000080;
    private const uint FILE_WRITE_ATTRIBUTES = 0x00000100;
    private const uint FILE_DELETE           = 0x00010000;
    private const uint FILE_READ_CONTROL     = 0x00020000;
    private const uint FILE_WRITE_DAC        = 0x00040000;
    private const uint FILE_WRITE_OWNER      = 0x00080000;
    private const uint FILE_ALL_ACCESS       = 0x001F01FF;
    private NTAccount? _account;
    public Ace? Ace { get; private set; }
    public FileAceEditor() {
        InitializeComponent();
        FullControl.Enabled = false;
        Traverse.Enabled = false;
        ListFolder.Enabled = false;
        ReadAttributes.Enabled = false;
        ReadExtendedAttributes.Enabled = false;
        CreateFiles.Enabled = false;
        WriteAttributes.Enabled = false;
        WriteExtendedAttributes.Enabled = false;
        CreateFiles.Enabled = false;
        CreateFolders.Enabled = false;
        DeleteSubfolders.Enabled = false;
        Delete.Enabled = false;
        ReadPermissions.Enabled = false;
        ChangePermissions.Enabled = false;
        TakeOwnership.Enabled = false;
        OnlyApply.Enabled = false;
        AceAppliesTo.Enabled = false;
        Ok.Enabled = false;
        Clear.Enabled = false;
    }
    public FileAceEditor(Ace? ace) {
        InitializeComponent();
        Ace = ace;
        if (ace is not null) {
            _account = new(ace.Trustee.GetDisplay());
            PrincipalName.Text = _account.Value;
            uint rights = ace.Right.GetHexValue();
            if ((rights & FILE_ALL_ACCESS) == FILE_ALL_ACCESS) {
                FullControl.Checked = true;
            } else {
                Traverse.Checked = (rights & FILE_TRAVERSE) == FILE_TRAVERSE;
                ListFolder.Checked = (rights & FILE_LIST_DIRECTORY) == FILE_LIST_DIRECTORY;
                ReadAttributes.Checked = (rights & FILE_READ_ATTRIBUTES) == FILE_READ_ATTRIBUTES;
                ReadExtendedAttributes.Checked = (rights & FILE_READ_EA) == FILE_READ_EA;
                CreateFiles.Checked = (rights & FILE_ADD_FILE) == FILE_ADD_FILE;
                CreateFolders.Checked = (rights & FILE_ADD_SUBDIRECTORY) == FILE_ADD_SUBDIRECTORY;
                WriteAttributes.Checked = (rights & FILE_WRITE_ATTRIBUTES) == FILE_WRITE_ATTRIBUTES;
                WriteExtendedAttributes.Checked = (rights & FILE_WRITE_EA) == FILE_WRITE_EA;
                ReadPermissions.Checked = (rights & FILE_READ_CONTROL) == FILE_READ_CONTROL;
                DeleteSubfolders.Checked = (rights & FILE_DELETE_CHILD) == FILE_DELETE_CHILD;
                Delete.Checked = (rights & FILE_DELETE) == FILE_DELETE;
                ChangePermissions.Checked = (rights & FILE_WRITE_DAC) == FILE_WRITE_DAC;
                TakeOwnership.Checked = (rights & FILE_WRITE_OWNER) == FILE_WRITE_OWNER;
            }
        }
    }
    private void FullControl_CheckedChanged(object sender, EventArgs e) {
        if (FullControl.Checked) {
            Traverse.Checked = true;
            ListFolder.Checked = true;
            ReadAttributes.Checked = true;
            ReadExtendedAttributes.Checked = true;
            CreateFiles.Checked = true;
            CreateFolders.Checked = true;
            WriteAttributes.Checked = true;
            WriteExtendedAttributes.Checked = true;
            DeleteSubfolders.Checked = true;
            Delete.Checked = true;
            ReadPermissions.Checked = true;
            ChangePermissions.Checked = true;
            TakeOwnership.Checked = true;
        }
    }
    private void Clear_Click(object sender, EventArgs e) {
        FullControl.Enabled = false;
        Traverse.Enabled = false;
        ListFolder.Enabled = false;
        ReadAttributes.Enabled = false;
        ReadExtendedAttributes.Enabled = false;
        CreateFiles.Enabled = false;
        WriteAttributes.Enabled = false;
        WriteExtendedAttributes.Enabled = false;
        CreateFiles.Enabled = false;
        CreateFolders.Enabled = false;
        DeleteSubfolders.Enabled = false;
        Delete.Enabled = false;
        ReadPermissions.Enabled = false;
        ChangePermissions.Enabled = false;
        TakeOwnership.Enabled = false;
        OnlyApply.Checked = false;
    }
    private void Ok_Click(object sender, EventArgs e) {
        SddlRight right;
        if (FullControl.Checked) {
            right = SddlRight.Construct(SddlRights.SDDL_KEY_ALL);
        } else {
            uint rights = 0;
            if (Traverse.Checked)
                rights |= FILE_TRAVERSE;
            if (ListFolder.Checked)
                rights |= FILE_LIST_DIRECTORY;
            if (ReadAttributes.Checked)
                rights |= FILE_READ_ATTRIBUTES;
            if (ReadExtendedAttributes.Checked)
                rights |= FILE_READ_EA;
            if (CreateFiles.Checked)
                rights |= FILE_ADD_FILE;
            if (CreateFolders.Checked)
                rights |= FILE_ADD_SUBDIRECTORY;
            if (WriteAttributes.Checked)
                rights |= FILE_WRITE_ATTRIBUTES;
            if (WriteExtendedAttributes.Checked)
                rights |= FILE_WRITE_EA;
            if (DeleteSubfolders.Checked)
                rights |= FILE_DELETE_CHILD;
            if (Delete.Checked)
                rights |= FILE_DELETE;
            if (ReadPermissions.Checked)
                rights |= FILE_READ_CONTROL;
            if (ChangePermissions.Checked)
                rights |= FILE_WRITE_DAC;
            if (TakeOwnership.Checked)
                rights |= FILE_WRITE_OWNER;
            right = SddlRight.Construct(rights);
        }
        SddlAceFlags? flags = null;
        if (OnlyApply.Checked) {
            flags = SddlAceFlags.SDDL_NO_PROPAGATE;
        };
        switch (AceAppliesTo.SelectedItem) {
            case "This folder, subfolders and files":
                flags |= SddlAceFlags.SDDL_CONTAINER_INHERIT | SddlAceFlags.SDDL_OBJECT_INHERIT;
                break;
            case "This folder and subfolders":
                flags |= SddlAceFlags.SDDL_CONTAINER_INHERIT;
                break;
            case "This folder and files":
                flags |= SddlAceFlags.SDDL_OBJECT_INHERIT;
                break;
            case "Subfolders and files only":
                flags |= SddlAceFlags.SDDL_CONTAINER_INHERIT | SddlAceFlags.SDDL_OBJECT_INHERIT | SddlAceFlags.SDDL_INHERIT_ONLY;
                break;
            case "Subfolders only":
                flags |= SddlAceFlags.SDDL_CONTAINER_INHERIT | SddlAceFlags.SDDL_INHERIT_ONLY;
                break;
            case "Files only":
                flags |= SddlAceFlags.SDDL_OBJECT_INHERIT | SddlAceFlags.SDDL_INHERIT_ONLY;
                break;
            default:
                break;
        }
        Ace = new(
            AceTypeCombo.SelectedItem switch {
                "Allow" => SddlAceType.SDDL_ACCESS_ALLOWED,
                "Deny" => SddlAceType.SDDL_ACCESS_DENIED,
                _ => throw new InvalidOperationException("Invalid ACE type selected.")
            },
            flags,
            right,
            null,
            null,
            Trustee.Construct((SecurityIdentifier)_account!.Translate(typeof(SecurityIdentifier)))
        );
    }

    private void SelectPrincipal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
        PrincipalPicker picker = new();
        if (picker.ShowDialog() == DialogResult.OK && picker.Principal is Trustee principal) {
            _account = new(principal.GetDisplay());
            PrincipalName.Text = _account.Value;
        }
    }
}
