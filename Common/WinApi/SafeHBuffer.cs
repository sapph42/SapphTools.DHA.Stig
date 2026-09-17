using System.Runtime.InteropServices;

namespace SapphTools.DHA.Stig.Common.WinApi; 
public class SafeHBuffer : SafeBuffer {
    public override bool IsInvalid => IsClosed || base.IsInvalid || DangerousGetHandle() == IntPtr.Zero;
    public SafeHBuffer() : base(true) { }
    public static SafeHBuffer AllocFactory(int length) {
        SafeHBuffer hbuff = new(
            Marshal.AllocHGlobal(length)
        );
        hbuff.Initialize((ulong)length);
        return hbuff;
    }
    internal SafeHBuffer(IntPtr handle) : base(true) {
        SetHandle(handle);
    }
    protected override bool ReleaseHandle() {
        Marshal.FreeHGlobal(handle);
        SetHandle(IntPtr.Zero); 
        return true;
    }
}
