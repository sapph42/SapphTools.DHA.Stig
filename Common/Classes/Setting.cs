namespace SapphTools.DHA.Stig.Common.Classes;
public class Setting : IComparable<Setting> {
    public int Order { get; set; }
    public SettingContext RequiredContext { get; set; } = SettingContext.Administrator;
    public bool Dangerous { get; set; } = false;
    public required IValue Data { get; set; }
    public Setting Clone() {
        return new() {
            Order = Order,
            RequiredContext = RequiredContext,
            Dangerous = Dangerous,
            Data = Data.Clone()
        };
    }
    public int CompareTo(Setting? other) {
        return Order.CompareTo(other?.Order);
    }
    public override int GetHashCode() {
        return Order.GetHashCode();
    }
}
