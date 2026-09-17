using SapphTools.SecurityDescriptor.Classes;
using System.ComponentModel;
namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class SeRightsSettings : ValueSettings, IValueSettings<SeRightsValue> {
    private readonly HashSet<Trustee> _trustees = [];
    public override bool IsValid =>
        Target.SelectedItem is LsaPrivilege &&
        AreValidPrincipals(AccountNames.Text.Split(','), out _);
    public override SeRightsValue? Value {
        get {
            if (!IsValid) {
                return null;
            }
            return new() {
                Target = (Target.SelectedItem as LsaPrivilege)!,
                AccountNames = [.. _trustees]
            };
        }
    }
    public SeRightsSettings() {
        InitializeComponent();
        foreach (LsaPrivilege priv in LsaPrivilege.ValidPrivs.Values) {
            Target.Items.Add(priv);
        }
        Target.Format += (_, e) => {
            if (e.ListItem is LsaPrivilege priv) {
                e.Value = priv.Value;
            }
        };
    }
    public SeRightsSettings(SeRightsValue? value) : this() {
        if (value is not null) {
            Target.SelectedItem = value.Target;
            AccountNames.Text = string.Join(",", value.AccountNames.Select(t => t.Sid));
        }
    }
    public SeRightsSettings(Setting? setting) : this() {
        if (setting is not null && setting.Data is SeRightsValue value) {
            Target.SelectedItem = value.Target;
            AccountNames.Text = string.Join(",", value.AccountNames.Select(t => t.Sid));
        }
    }
    private void AccountNames_Validating(object sender, CancelEventArgs e) {
        string[] names = AccountNames.Text.Split(',');
        if (!AreValidPrincipals(names, out int index)) {
            e.Cancel = true;
            ErrorProvider.SetError(AccountNames, $"Unresolvable principal at item {index}: {names[index]}");
            return;
        }
        AccountNames.Text = string.Join(",", _trustees.Select(t => t.DisplayString));
        ErrorProvider.SetError(AccountNames, "");
    }
    private bool AreValidPrincipals(string[] names, out int index) {
        index = -1;
        if (names.Length == 0 || names.All(n => n.Length == 0)) {
            index = 0;
            return false;
        }
        for (int i = 0; i < names.Length; i++) {
            try {
                Trustee trustee = Trustee.Construct(names[i].Trim());
                _trustees.Add(trustee);
            } catch {
                index = i;
                return false;
            }
        }
        return true;
    }
}
