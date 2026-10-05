namespace SapphTools.DHA.Stig.Remediator.Classes;
/// <summary>
/// A placeholder class for scenarios where a generic Result<T> needs a non-null type
/// but no actual value is expected (e.g., for a non-generic success/failure result).
/// </summary>
public sealed class Null {
    public static Null Instance { get; } = new();
    private Null() { }
}