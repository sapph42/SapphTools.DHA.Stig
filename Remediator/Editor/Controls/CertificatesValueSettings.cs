using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class CertificatesValueSettings : ValueSettings, IValueSettings<CertificatesValue> {
    private IArtifact _artifact;
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
                Artifact = _artifact
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
            _artifact = value.Artifact;
            ArtifactPath.Text = _artifact.RelativePath;
        }
        RuleId = ruleId;
    }
    [SetsRequiredMembers]
    public CertificatesValueSettings(Setting? setting, string ruleId) : this() {
        if (setting is not null && setting.Data is CertificatesValue value) {
            Target.Text = value.Target;
            _artifact = value.Artifact;
            ArtifactPath.Text = _artifact.RelativePath;
        }
        RuleId = ruleId;
    }

    private void Browse_Click(object sender, EventArgs e) {
        if (ArtifactsList.SelectedItem is null) {
            return;
        }
        OpenFileDialog ofd = new() {
            InitialDirectory = ArtifactFirstBuild.BasePath,
            Filter = "Serialized Certificate Store (*.sst)|*.sst",
            Multiselect = false
        };
        if (ofd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName)) {
            ArtifactFirstBuild newArtifact = ArtifactFirstBuild.Construct(RuleId, ofd.FileName);
            _artifact = newArtifact;
            ArtifactPath.Text = _artifact.RelativePath;
        }
    }
}
