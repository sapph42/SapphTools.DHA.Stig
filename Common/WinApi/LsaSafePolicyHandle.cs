using Microsoft.Win32.SafeHandles;

namespace SapphTools.DHA.Stig.Common.WinApi;
public class SafeLsaPolicyHandle : SafeHandleZeroOrMinusOneIsInvalid {

    public SafeLsaPolicyHandle() : base(true) { }
    public SafeLsaPolicyHandle(IntPtr preExistingHandle, bool ownsHandle) : base(ownsHandle) {
        SetHandle(preExistingHandle);
    }
    protected override bool ReleaseHandle() {
        return LsaConnection.LsaClose(handle) == 0;
    }
}