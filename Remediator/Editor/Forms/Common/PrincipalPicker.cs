using SapphTools.SecurityDescriptor.Classes;
using System.Data;
using System.Security.Principal;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common; 
public partial class PrincipalPicker : Form {
    private bool _dirty = false;
    private string _cleanText = string.Empty;
    private SecurityIdentifier? _resolved;
    public Trustee? Principal { get; internal set; }
    public PrincipalPicker() {
        InitializeComponent();
    }
    private void Check_Click(object sender, EventArgs e) {
        ResolveName();
    }
    private void Cancel_Click(object sender, EventArgs e) {
        Principal = null;
        Close();
    }
    private void Ok_Click(object sender, EventArgs e) {
        if (_dirty) {
            ResolveName();
        }
        if (_dirty || _resolved is null) {
            return;
        }
        Principal = Trustee.Construct(_resolved);
        Close();
    }
    private void Name_TextChanged(object sender, EventArgs e) {
        if (!PrincipalName.Text.Equals(_cleanText)) {
            PrincipalName.Font = new Font("Segoe UI", 9F);
            _dirty = true;
        }
    }
    private void ResolveName() {
        try {
            _resolved = new(PrincipalName.Text);
            PrincipalName.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            _dirty = false;
        } catch { }
        try {
            NTAccount account = new(PrincipalName.Text);
            _resolved = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            account = (NTAccount)_resolved.Translate(typeof(NTAccount));
            _cleanText = account.Value;
            PrincipalName.Text = _cleanText;
            PrincipalName.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            _dirty = false;
        } catch {
            try {
                _resolved = new(PrincipalName.Text);
                try {
                    NTAccount account = (NTAccount)_resolved.Translate(typeof(NTAccount));
                    _cleanText = account.Value;
                    PrincipalName.Text = _cleanText;
                    PrincipalName.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
                    _dirty = false;
                } catch {
                    _cleanText = _resolved.Value;
                    PrincipalName.Text = _cleanText;
                    PrincipalName.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
                    _dirty = false;
                }
            } catch {
                ResolveNameHarder();
            }
        }
    }
    private void ResolveNameHarder() {
        IEnumerable<string> localUsers = NetUserWinApi
            .EnumerateLocalUsers()
            .Where(u => u.Contains(PrincipalName.Text, StringComparison.OrdinalIgnoreCase));
        IEnumerable<string> localGroups = NetUserWinApi
            .EnumerateLocalGroups()
            .Where(u => u.Contains(PrincipalName.Text, StringComparison.OrdinalIgnoreCase));
        IEnumerable<string> wellKnown = Trustee.
            EnumerateWellKnownPrincipals()
            .Where(u => u.Contains(PrincipalName.Text, StringComparison.OrdinalIgnoreCase));
        IEnumerable<string> candidates = localUsers.Union(localGroups).Union(wellKnown);
        if (candidates.Count() == 1) {
            Trustee oneHit = Trustee.Construct(candidates.First());
            _resolved = new SecurityIdentifier(oneHit.Sid);
            PrincipalName.Text = oneHit.DisplayString;
            _dirty = false;
            PrincipalName.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            return;
        }
        Disambiguation disam = new(
            candidates,
            PrincipalName.Text
        );
        if (disam.ShowDialog() == DialogResult.OK && disam.Selection is Trustee principal) {
            _resolved = new SecurityIdentifier(principal.Sid);
            PrincipalName.Text = principal.DisplayString;
            _dirty = false;
            PrincipalName.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
        }
    }
}
