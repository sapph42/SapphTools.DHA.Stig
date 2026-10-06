namespace SapphTools.DHA.Stig.Common.Classes;
public class Setting : IComparable<Setting>, IEquatable<Setting> {
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
    public override bool Equals(object? obj) => Equals(obj as Setting);
    public bool Equals(Setting? other) {
        return Order == other?.Order;
    }
    public override int GetHashCode() {
        return Order.GetHashCode();
    }
}
public class SettingComparer : IComparer<Setting> {
    public int Compare(Setting? x, Setting? y) {
        if (x is null && y is null) {
            return 0;
        }
        if (x is null) {
            return -1;
        }
        if (y is null) {
            return 1;
        }
        return x.Order.CompareTo(y.Order);
    }
}
public class SettingsSet : SortedSet<Setting> {
    public static readonly SettingComparer SettingComparer = new();
    public SettingsSet() : base(SettingComparer) { }
    public SettingsSet(IEnumerable<Setting> settings) : base(settings, SettingComparer) { }
}