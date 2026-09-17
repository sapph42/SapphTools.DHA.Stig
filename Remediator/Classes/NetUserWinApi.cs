using System.Runtime.InteropServices;

namespace SapphTools.DHA.Stig.Remediator.Classes;
internal static partial class NetUserWinApi {
    private const int NERR_Success = 0;
    private const int ERROR_MORE_DATA = 234;
    private const int MAX_PREFERRED_LENGTH = -1;

    private const int FILTER_NORMAL_ACCOUNT = 0x0002;

    public static IEnumerable<string> EnumerateLocalUsers(string? computerName = null) {
        int resume = 0;
        int result;
        do {
            IntPtr buffer = IntPtr.Zero;
            try {
                result = NetUserEnum(
                    NormalizeServerName(computerName),
                    level: 0,
                    filter: FILTER_NORMAL_ACCOUNT,
                    bufptr: out buffer,
                    prefmaxlen: -1,
                    entriesread: out int entriesRead,
                    totalentries: out _,
                    resume_handle: ref resume
                );
                if (result != 0 && result != 234) {
                    throw new System.ComponentModel.Win32Exception(result);
                }
                int size = Marshal.SizeOf<USER_INFO_0>();
                IntPtr current = buffer;
                for (int i = 0; i < entriesRead; i++) {
                    USER_INFO_0 entry = Marshal.PtrToStructure<USER_INFO_0>(current);
                    if (!string.IsNullOrWhiteSpace(entry.usri0_name)) {
                        yield return entry.usri0_name;
                    }
                    current = IntPtr.Add(current, size);
                }
            } finally {
                if (buffer != IntPtr.Zero) {
                    _ = NetApiBufferFree(buffer);
                }
            }
        } while (result == 234);
    }
    public static IEnumerable<string> EnumerateLocalGroups(string? computerName = null) {
        int resume = 0;
        int result;
        do {
            IntPtr buffer = IntPtr.Zero;
            try {
                result = NetLocalGroupEnum(
                    NormalizeServerName(computerName),
                    level: 0,
                    bufptr: out buffer,
                    prefmaxlen: -1,
                    entriesread: out int entriesRead,
                    totalentries: out _,
                    resume_handle: ref resume
                );
                if (result != 0 && result != 234) {
                    throw new System.ComponentModel.Win32Exception(result);
                }
                int size = Marshal.SizeOf<LOCALGROUP_INFO_0>();
                IntPtr current = buffer;
                for (int i = 0; i < entriesRead; i++) {
                    LOCALGROUP_INFO_0 entry = Marshal.PtrToStructure<LOCALGROUP_INFO_0>(current);
                    if (!string.IsNullOrWhiteSpace(entry.lgrpi0_name)) {
                        yield return entry.lgrpi0_name;
                    }
                    current = IntPtr.Add(current, size);
                }
            } finally {
                if (buffer != IntPtr.Zero) {
                    _ = NetApiBufferFree(buffer);
                }
            }
        } while (result == 234);
    }
    private static string? NormalizeServerName(string? computerName) {
        if (string.IsNullOrWhiteSpace(computerName)) {
            return null;
        }
        return computerName.StartsWith(@"\\", StringComparison.Ordinal) ?
            computerName :
            $@"\\{computerName}";
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct USER_INFO_0 {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string usri0_name;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct LOCALGROUP_INFO_0 {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lgrpi0_name;
    }
    [LibraryImport("netapi32.dll", EntryPoint = "NetUserEnum", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial int NetUserEnum(
        string? servername,
        int level,
        int filter,
        out IntPtr bufptr,
        int prefmaxlen,
        out int entriesread,
        out int totalentries,
        ref int resume_handle
    );
    [LibraryImport("netapi32.dll", EntryPoint = "NetLocalGroupEnum", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial int NetLocalGroupEnum(
        string? servername,
        int level,
        out IntPtr bufptr,
        int prefmaxlen,
        out int entriesread,
        out int totalentries,
        ref int resume_handle
    );
    [LibraryImport("netapi32.dll")]
    private static partial int NetApiBufferFree(IntPtr buffer);
}
