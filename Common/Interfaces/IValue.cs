namespace SapphTools.DHA.Stig.Common.Interfaces;

public interface IValue {
    [JsonIgnore]
    public TargetType Action { get; }
    public string TargetString { get; }
    public IValue Clone();
    public string ToString() => TargetString;
}

[JsonConverter(typeof(ValueConverter))]
public interface IValue<T> : IValue {
    public new T Clone();
}