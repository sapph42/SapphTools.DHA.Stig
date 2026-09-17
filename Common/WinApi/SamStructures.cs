using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SapphTools.DHA.Stig.Common.WinApi;
//ALL SamStructures should implement IEquitable.  This is unenforcible through code

[StructLayout(LayoutKind.Sequential)]
public struct SamUserModalInfo3(int duration, int window, int threshold) : IEquatable<SamUserModalInfo3> {
    public int Duration = duration;
    public int Window = window;
    public int Threshold = threshold;
    public SamUserModalInfo3() : this(0,0,0) { }

    public readonly bool Equals(SamUserModalInfo3 other) {
        return Duration == other.Duration && Window == other.Window && Threshold == other.Threshold;
    }
    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        return obj is SamUserModalInfo3 other && Equals(other);
    }

    public override readonly int GetHashCode() {
        HashCode code = new();
        code.Add(Duration);
        code.Add(Window);
        code.Add(Threshold);
        return code.ToHashCode();
    }
    public static bool operator ==(SamUserModalInfo3 left, SamUserModalInfo3 right) {
        return left.Equals(right);
    }

    public static bool operator !=(SamUserModalInfo3 left, SamUserModalInfo3 right) {
        return !(left == right);
    }
}