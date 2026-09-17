using SapphTools.DHA.Stig.Remediator.Editor.Forms.Common;
using SapphTools.SecurityDescriptor;
using SapphTools.SecurityDescriptor.Classes;
using SapphTools.SecurityDescriptor.Enums;
using SapphTools.SecurityDescriptor.Extensions;
using System.Data;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Registry;
public partial class RegistryAclEditor : Form {
    internal Sddl ActiveSddl;
    internal Sddl CommittedSddl;
    internal Sddl OriginalSddl;
    private Trustee? _owner;
    public Sddl Acl => CommittedSddl;
    public RegistryAclEditor() {
        InitializeComponent();
        OriginalSddl = new Sddl("O:SYG:SYD:P", ObjectType.RegistryKey);
        ActiveSddl = new(OriginalSddl.SddlString, OriginalSddl.Type);
        CommittedSddl = new(OriginalSddl.SddlString, OriginalSddl.Type);
    }
    public RegistryAclEditor(Sddl sddl) {
        InitializeComponent();
        ActiveSddl = new(sddl.SddlString, sddl.Type);
        CommittedSddl = new(sddl.SddlString, sddl.Type);
        OriginalSddl = sddl;
        _owner = sddl.Owner;
        PrincipalName.Text = _owner!.DisplayString;
        SelectPrincipal.Text = "Change";
        foreach (Ace ace in sddl.DaclAces ?? []) {
            CreateRow(Permissions, ace);
        }
    }
    #region Event Handlers
    private void Permissions_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
        Ace ace = (Ace)Permissions.Rows[e.RowIndex].Tag!;
        Ace? editedAce = EditAce(ace);
        if (editedAce is not null) {
            UpdateRow(Permissions.Rows[e.RowIndex], editedAce);
            ActiveSddl.ReplaceDacl(ace, editedAce);
        }
    }

    private void AddPermissions_Click(object sender, EventArgs e) {
        Ace? newAce = EditAce(null);
        if (newAce is not null) {
            ActiveSddl.AddDacl(newAce);
            CreateRow(Permissions, newAce);
        }
    }

    private void RemovePermissions_Click(object sender, EventArgs e) {
        DataGridViewRow row = Permissions.SelectedRows.Cast<DataGridViewRow>().First();
        Ace ace = (Ace)row.Tag!;
        ActiveSddl.RemoveDacl(ace);
        Permissions.Rows.Remove(row);
    }

    private void EditPermissions_Click(object sender, EventArgs e) {
        DataGridViewRow row = Permissions.SelectedRows.Cast<DataGridViewRow>().First();
        Ace ace = (Ace)row.Tag!;
        Ace? editedAce = EditAce(ace);
        if (editedAce is not null) {
            UpdateRow(row, editedAce);
            ActiveSddl.ReplaceDacl(ace, editedAce);
        }
    }
    #endregion Event Handlers
    private static Ace? EditAce(Ace? ace) {
        RegistryAceEditor aceEditor = new(ace);
        if (aceEditor.ShowDialog() == DialogResult.OK) {
            return aceEditor.Ace!;
        }
        return ace;
    }
    private static void CreateRow(DataGridView view, Ace ace) {
        int index = view.Rows.Add();
        DataGridViewRow row = view.Rows[index];
        row.Tag = ace;
        UpdateRow(row, ace, true);
    }
    private static void UpdateRow(DataGridViewRow row, Ace ace, bool overwrite = false) {
        if (!overwrite && ((Ace)row.Tag!).Equals(ace)) {
            return;
        }
        row.Cells["PrincipalDgv"].Value = ace.Trustee;
        row.Cells["TypeDgv"].Value = ace.Type switch {
            SddlAceType.SDDL_ACCESS_ALLOWED => "Allow",
            SddlAceType.SDDL_ACCESS_DENIED => "Deny",
            _ => null
        };
        if (MetaExtensions.TryGetMetaAbbr(ace.Right.Value, out SddlRights rights)) {
            row.Cells["AccessDgv"].Value = rights.GetDescription();
        } else {
            row.Cells["AccessDgv"].Value = "Special";
        }
        row.Cells["InheritedFromDgv"].Value = "None";
        row.Cells["AppliesToDgv"].Value = ace.Flags switch {
            null => "This key only",
            SddlAceFlags.SDDL_CONTAINER_INHERIT => "This key and subkeys",
            SddlAceFlags.SDDL_CONTAINER_INHERIT | SddlAceFlags.SDDL_OBJECT_INHERIT => "Subkeys only",
            _ => "Unknown"
        };
        row.Tag = ace;
    }

    private void Cancel_Click(object sender, EventArgs e) {
        Close();
    }

    private void Apply_Click(object sender, EventArgs e) {
        CommittedSddl = new(ActiveSddl.SddlString, ActiveSddl.Type);
    }

    private void Ok_Click(object sender, EventArgs e) {
        CommittedSddl = new(ActiveSddl.SddlString, ActiveSddl.Type);
        Close();
    }

    private void SelectPrincipal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
        PrincipalPicker picker = new();
        if (picker.ShowDialog() != DialogResult.OK || picker.Principal is null) {
            return;
        }
        _owner = picker.Principal;
        ActiveSddl.Owner = _owner;
        PrincipalName.Text = _owner!.DisplayString;
        SelectPrincipal.Text = "Change";
    }
}
