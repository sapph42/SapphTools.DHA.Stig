using System.Runtime.InteropServices;

namespace SapphTools.DHA.Stig.Common.WinApi; 
internal static partial class ScmApi {
    internal const int ERROR_ACCESS_DENIED = 0x5;
    internal const int ERROR_INVALID_HANDLE = 0x6;

    public const int SERVICE_WIN32 = 0x0030;
    public const int SERVICE_STATE_ALL = 0x00000003;
    public const string SERVICES_ACTIVE_DATABASE = "ServicesActive";

    internal const int READ_CONTROL = 0x00020000;
    internal const int STANDARD_RIGHTS_WRITE = READ_CONTROL;
    internal const int STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
    internal const int STANDARD_RIGHTS_READ = READ_CONTROL;

    internal const int SC_MANAGER_CONNECT = 0x0001;
    internal const int SC_MANAGER_ENUMERATE_SERVICES = 0x0004;
    internal const int SC_MANAGER_LOCK = 0x0008;
    internal const int SCM_GENERIC_EXECUTE = STANDARD_RIGHTS_EXECUTE |
        SC_MANAGER_CONNECT | SC_MANAGER_ENUMERATE_SERVICES | SC_MANAGER_LOCK;

    internal const int SERVICE_CHANGE_CONFIG = 0x0002;
    internal const int SERVICE_START = 0x0010;
    internal const int SERVICE_STOP = 0x0020;
    internal const int SERVICE_PAUSE = 0x0040;
    internal const int SERVICE_USER_DEFINED = 0x0100;
    internal const int SERVICE_GENERIC_EXECUTE = STANDARD_RIGHTS_EXECUTE | SERVICE_START |
        SERVICE_STOP | SERVICE_PAUSE | SERVICE_USER_DEFINED;

    internal const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
    internal const int SC_MODIFY_SERVICE = SERVICE_CHANGE_CONFIG | SERVICE_GENERIC_EXECUTE;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ENUM_SERVICE_STATUS_PROCESS {
        public string lpServiceName;
        public string lpDisplayName;
        public SERVICE_STATUS_PROCESS ServiceStatusProcess;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SERVICE_STATUS_PROCESS {
        public int dwServiceType;
        public int dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
        public int dwProcessId;
        public int dwServiceFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SERVICE_STATUS {
        public int dwServiceType;
        public int dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct QUERY_SERVICE_CONFIG {
        public uint dwServiceType;
        public uint dwStartType;
        public uint dwErrorControl;
        public string lpBinaryPathName;
        public string lpLoadOrderGroup;
        public uint dwTagId;
        public string lpDependencies;
        public string lpServiceStartName;
        public string lpDisplayName;
    }

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseServiceHandle(IntPtr hSCObject);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ControlService(SafeScmHandle hService, int dwControl, ref SERVICE_STATUS lpServiceStatus);

    [LibraryImport("advapi32.dll", EntryPoint = "EnumServicesStatusExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EnumServicesStatusEx(
        SafeScmHandle hSCManager,
        int infoLevel,
        int serviceType,
        int serviceState,
        nint lpServices,
        int cbBufSize,
        out int pcbBytesNeeded,
        out int lpServicesReturned,
        ref int lpResumeHandle,
        string? groupName);

    [LibraryImport("advapi32.dll", EntryPoint = "OpenSCManagerW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint OpenSCManager(string? machineName, string? databaseName, int dwAccess);

    [LibraryImport("advapi32.dll", EntryPoint = "OpenServiceW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint OpenService(SafeScmHandle hSCManager, string lpServiceName, uint dwDesiredAccess);

    [LibraryImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool QueryServiceStatusEx(
        SafeScmHandle hService,
        int infoLevel,
        nint lpBuffer,
        int cbBufSize,
        out int pcbBytesNeeded);

    [LibraryImport("advapi32.dll", EntryPoint = "QueryServiceConfigW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool QueryServiceConfig(
        SafeScmHandle hService,
        nint lpServiceConfig,
        int cbBufSize,
        out int pcbBytesNeeded
    );

    [LibraryImport("advapi32.dll", EntryPoint = "ChangeServiceConfigW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ChangeServiceConfig(
        SafeScmHandle hService,
        uint dwServiceType,
        uint dwErrorControl,
        string? lpBinaryPathName,
        string? lpLoadORderGroup,
        out uint lpdwTagId,
        string? lpDependencies,
        string? lpServiceStartName,
        string? lpPassword,
        string? lpDisplayName
    );

    [LibraryImport("advapi32.dll", EntryPoint = "StartServiceW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool StartService(SafeScmHandle hService, int dwNumServiceArgs, in string[]? lpServiceArgVectors);
}
