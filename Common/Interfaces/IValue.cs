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
        return lhs!.Equals(rhs);
    }
}

[JsonConverter(typeof(ValueConverter))]
public interface IValue<T> : IValue {
    public new T Clone();
}