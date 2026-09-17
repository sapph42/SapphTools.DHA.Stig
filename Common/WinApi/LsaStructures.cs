using System.Runtime.InteropServices;

namespace SapphTools.DHA.Stig.Common.WinApi;

[StructLayout(LayoutKind.Sequential)]
public struct LsaObjectAttributes {
    public int Length = 0;
    public IntPtr RootDirectory = IntPtr.Zero;
    public IntPtr ObjectName = IntPtr.Zero;
    public int Attributes = 0;
    public IntPtr SecurityDescriptor = IntPtr.Zero;
    public IntPtr SecurityQualityOfService = IntPtr.Zero;
    public LsaObjectAttributes() { }
}

[StructLayout(LayoutKind.Sequential)]
public struct LsaUnicodeString {
    public ushort Length;
    public ushort MaximumLength;
    public IntPtr Buffer;
}

[StructLayout(LayoutKind.Sequential)]
public struct LsaEnumerationInfo {
    public IntPtr PSid;
}