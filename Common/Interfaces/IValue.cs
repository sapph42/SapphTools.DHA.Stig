namespace SapphTools.DHA.Stig.Common.Interfaces;

public interface IValue : IEquatable<IValue> {
    [JsonIgnore]
    public TargetType Action { get; }
    public string TargetString { get; }
    public IValue Clone();
    public string ToString() => TargetString;
    public static bool Equals(IValue? lhs, IValue? rhs) {
        if (ReferenceEquals(lhs, rhs)) { 
            return true; 
        }
        if (lhs is not null) {
            return lhs.Equals(rhs);
        }
        return rhs!.Equals(lhs);
    }
}

[JsonConverter(typeof(ValueConverter))]
public interface IValue<T> : IValue {
    public new T Clone();
}