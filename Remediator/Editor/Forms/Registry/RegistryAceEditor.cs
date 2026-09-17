using SapphTools.SecurityDescriptor.Classes;
using SapphTools.SecurityDescriptor.Enums;
using System.Security.Principal;
using SddlRight = SapphTools.SecurityDescriptor.Classes.Right;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Registry;
public partial class RegistryAceEditor : Form {
    private const uint QueryValueMask = 0x00000001;
    private const uint SetValueMask = 0x00000002;
    private const uint CreateSubKeyMask = 0x00000004;
    private const uint EnumerateSubKeysMask = 0x00000008;
    private const uint NotifyMask = 0x00000010;
    private const uint CreateLinkMask = 0x00000020;
    private const uint DeleteMask = 0x00010000;
    private const uint ReadControlMask = 0x00020000;
    private const uint WriteDacMask = 0x00040000;
    private const uint WriteOwnerMask = 0x00080000;
    private const uint ReadMask = 0x00020019;
    private const uint WriteMask = 0x00020006;
    private const uint ExecuteMask = 0x00020019;
    private const uint AllMask = 0x000F003F;
    private NTAccount? _account;
    private Trustee? _trustee;
    public Ace? Ace { get; private set; }
    public RegistryAceEditor() {
        InitializeComponent();
        FullControl.Enabled = false;
        QueryValue.Enabled = false;
        SetValue.Enabled = false;
        CreateSubkey.Enabled = false;
        EnumerateSubkeys.Enabled = false;
        Notify.Enabled = false;
        CreateLink.Enabled = false;
        Delete.Enabled = false;
        WriteDac.Enabled = false;
        WriteOwner.Enabled = false;
        ReadControl.Enabled = false;
        OnlyApply.Enabled = false;
        AceAppliesTo.Enabled = false;
        Ok.Enabled = false;
        Clear.Enabled = false;
    }
    public RegistryAceEditor(Ace? ace) {
        InitializeComponent();
        Ace = ace;
        if (ace is not null) {
            _trustee = ace.Trustee;
            _account = new(_trustee.GetDisplay());
            PrincipalName.Text = _account.Value;
            uint rights = ace.Right.GetHexValue();
            if ((rights & AllMask) == AllMask) {
                FullControl.Checked = true;
            } else {
                QueryValue.Checked = (rights & QueryValueMask) == QueryValueMask;
                SetValue.Checked = (rights & SetValueMask) == SetValueMask;
                CreateSubkey.Checked = (rights & CreateSubKeyMask) == CreateSubKeyMask;
                EnumerateSubkeys.Checked = (rights & EnumerateSubKeysMask) == EnumerateSubKeysMask;
                Notify.Checked = (rights & NotifyMask) == NotifyMask;
                CreateLink.Checked = (rights & CreateLinkMask) == CreateLinkMask;
                Delete.Checked = (rights & DeleteMask) == DeleteMask;
                ReadControl.Checked = (rights & ReadControlMask) == ReadControlMask;
                WriteDac.Checked = (rights & WriteDacMask) == WriteDacMask;
                WriteOwner.Checked = (rights & WriteOwnerMask) == WriteOwnerMask;
            }
        }
    }
    private void FullControl_CheckedChanged(object sender, EventArgs e) {
        if (FullControl.Checked) {
            QueryValue.Checked = true;
            SetValue.Checked = true;
            CreateSubkey.Checked = true;
            EnumerateSubkeys.Checked = true;
            Notify.Checked = true;
            CreateLink.Checked = true;
            Delete.Checked = true;
            WriteDac.Checked = true;
            WriteOwner.Checked = true;
            ReadControl.Checked = true;
        }
    }
    private void Clear_Click(object sender, EventArgs e) {
        FullControl.Checked = false;
        QueryValue.Checked = false;
        SetValue.Checked = false;
        CreateSubkey.Checked = false;
        EnumerateSubkeys.Checked = false;
        Notify.Checked = false;
        CreateLink.Checked = false;
        Delete.Checked = false;
        WriteDac.Checked = false;
        WriteOwner.Checked = false;
        ReadControl.Checked = false;
        OnlyApply.Checked = false;
    }
    private void Ok_Click(object sender, EventArgs e) {
        SddlRight right;
        if (FullControl.Checked) {
            right = SddlRight.Construct(SddlRights.SDDL_KEY_ALL);
        } else {
            uint rights = 0;
            if (QueryValue.Checked)
                rights |= QueryValueMask;
            if (SetValue.Checked)
                rights |= SetValueMask;
            if (CreateSubkey.Checked)
                rights |= CreateSubKeyMask;
            if (EnumerateSubkeys.Checked)
                rights |= EnumerateSubKeysMask;
            if (Notify.Checked)
                rights |= NotifyMask;
            if (CreateLink.Checked)
                rights |= CreateLinkMask;
            if (Delete.Checked)
                rights |= DeleteMask;
            if (WriteDac.Checked)
                rights |= WriteDacMask;
            if (WriteOwner.Checked)
                rights |= WriteOwnerMask;
            if (ReadControl.Checked)
                rights |= ReadControlMask;
            right = SddlRight.Construct(rights);
        }
        SddlAceFlags? flags = 0;
        if (OnlyApply.Checked) {
            flags = SddlAceFlags.SDDL_NO_PROPAGATE;
        };
        switch (AceAppliesTo.SelectedItem) {
            case "This key and subkeys":
                flags |= SddlAceFlags.SDDL_CONTAINER_INHERIT;
                break;
            case "Subkeys only":
                flags |= SddlAceFlags.SDDL_CONTAINER_INHERIT | SddlAceFlags.SDDL_OBJECT_INHERIT;
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
            _trustee!
        );
    }

    private void SelectPrincipal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
        PrincipalPicker picker = new();
        if (picker.ShowDialog() == DialogResult.OK && picker.Principal is Trustee principal) {
            _account = new(principal.GetDisplay());
            _trustee = principal;
            PrincipalName.Text = _account.Value;
        }
    }
}
