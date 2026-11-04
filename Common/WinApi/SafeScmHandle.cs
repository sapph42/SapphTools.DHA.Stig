using Microsoft.Win32.SafeHandles;

namespace SapphTools.DHA.Stig.Common.WinApi; 
public class SafeScmHandle : SafeHandleZeroOrMinusOneIsInvalid{
    public SafeScmHandle() : base(true) { }
    public SafeScmHandle(IntPtr preExistingHandle, bool ownsHandle) : base(ownsHandle) { 
        SetHandle(preExistingHandle);
    }
    protected override bool ReleaseHandle() {
        return ScmApi.CloseServiceHandle(handle);
    }
}
