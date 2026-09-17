namespace SapphTools.DHA.Stig.Remediator.Controls; 
internal class DataGridViewPrincipalColumn : DataGridViewColumn {
    public DataGridViewPrincipalColumn() : base(new DataGridViewPrincipalCell()) { }
    public override object Clone() {
        return base.Clone();
    }
}
