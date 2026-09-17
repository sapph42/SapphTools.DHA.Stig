namespace SapphTools.DHA.Stig.Remediator.Editor.Interfaces;
public interface IValueSettings<T> where T : IValue {
    public bool IsValid { get; }
    public T? Value { get; }
}