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
    public bool DeepEquals(Setting? other) {
        return Order == other?.Order &&
            RequiredContext == other?.RequiredContext &&
            Dangerous == other?.Dangerous &&
            Data.Equals(other?.Data);
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
    public new bool SetEquals(IEnumerable<Setting> other) {
        List<Setting> otherUnsorted = [.. other];
        SettingsSet otherSorted = [.. otherUnsorted];
        if (otherSorted.Count != otherUnsorted.Count) {
            return false;
        }
        if (otherSorted.Count != Count) {
            return false;
        }
        foreach (Setting setting in this) {
            if (otherSorted.TryGetValue(setting, out Setting? potentialMatch)) {
                if (!setting.DeepEquals(potentialMatch)) {
                    return false;
                }
            } else {
                return false;
            }
        }
        return true;
    }
}