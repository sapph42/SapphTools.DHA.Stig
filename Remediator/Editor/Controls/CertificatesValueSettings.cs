using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class CertificatesValueSettings : ValueSettings, IValueSettings<CertificatesValue> {
    private readonly List<IArtifact> _artifacts = [];
    public required string RuleId { get; set; }
    public override bool IsValid =>
        !string.IsNullOrWhiteSpace(Target.Text) &&
        ArtifactsList.Items.Count > 0;
    public override CertificatesValue? Value {
        get {
            if (!IsValid) {
                return null;
            }
            return new() {
                Target = Target.Text,
                Artifacts = _artifacts
            };
        }
    }
    public CertificatesValueSettings() {
        InitializeComponent();
    }
    [SetsRequiredMembers]
    public CertificatesValueSettings(CertificatesValue? value, string ruleId) : this() {
        if (value is not null) {
            Target.Text = value.Target;
            _artifacts = value.Artifacts;
            foreach (IArtifact artifact in _artifacts) {
                ArtifactsList.Items.Add(artifact.Hash);
            }
        }
        RuleId = ruleId;
    }
    [SetsRequiredMembers]
    public CertificatesValueSettings(Setting? setting, string ruleId) : this() {
        if (setting is not null && setting.Data is CertificatesValue value) {
            Target.Text = value.Target;
            _artifacts = value.Artifacts;
            foreach (IArtifact artifact in _artifacts) {
                ArtifactsList.Items.Add(artifact.Hash);
            }
        }
        RuleId = ruleId;
    }

    private void Add_Click(object sender, EventArgs e) {
        if (ArtifactsList.SelectedItem is null) {
            return;
        }
        OpenFileDialog ofd = new();
        if (ofd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName)) {
            ArtifactFirstBuild newArtifact = ArtifactFirstBuild.Construct(RuleId, ofd.FileName);
            _artifacts.Add(newArtifact);
            ArtifactsList.Items.Add(newArtifact.Hash);
        }
    }

    private void Remove_Click(object sender, EventArgs e) {
        if (ArtifactsList.SelectedItem is null) {
            return;
        }
        string hash = (string)ArtifactsList.SelectedItem;
        IArtifact old = _artifacts.Where(a => a.Hash.Equals(hash, StringComparison.OrdinalIgnoreCase)).First();
        _artifacts.Remove(old);
        ArtifactsList.Items.Remove(ArtifactsList.SelectedItem);
    }
}
