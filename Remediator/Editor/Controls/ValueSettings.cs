namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class ValueSettings : UserControl {
    public virtual bool IsValid => false;
    public virtual IValue? Value => null;
    public ValueSettings() {
        InitializeComponent();
    }
}