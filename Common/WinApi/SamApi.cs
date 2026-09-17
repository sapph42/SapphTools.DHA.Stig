using SapphTools.DHA.Stig.Common.Classes;
using System.Runtime.InteropServices;

namespace SapphTools.DHA.Stig.Common.WinApi;
public static partial class SamApi {
    public static bool SetSamData<T, TSub>(string? computerName, SamValue<T, TSub> value)
            where T : struct where TSub : IValue {
        IntPtr structPtr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
        Marshal.StructureToPtr(value.SamStruct, structPtr, false); //Should be false? Warnings about memory leaks, but I don't want to futz with value
        try {
            uint status = NetUserModalsSet(computerName, (int)value.Target, structPtr, out _);
            if (status != 0) {
                return false;
            } else {
                return true;
            }
        } finally {
            Marshal.FreeHGlobal(structPtr);
        }
    }
    public static T GetSamData<T, TSub>(string? computerName, SamValue<T, TSub> value) 
            where T : struct where TSub : IValue  {
        T returnStruct;
        IntPtr structPtr = IntPtr.Zero;
        try {
            uint status = NetUserModalsGet(computerName, (int)value.Target, out structPtr);
            if (status != 0) {
                int retVal;
                unchecked {
                    retVal = (int)status;
                }
                throw new COMException("Failed to get SAM data", retVal);
            } else {
                returnStruct = Marshal.PtrToStructure<T>(structPtr);
                return returnStruct;
            }
        } finally {
            _ = NetApiBufferFree(structPtr);
        }
    }
    [LibraryImport("netapi32.dll", EntryPoint = "NetUserModalsSetW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    private static partial uint NetUserModalsSet(
        string? server,
        int level,
        IntPtr BufPtr,
        out uint ErrPtr);

    [LibraryImport("netapi32.dll", EntryPoint = "NetUserModalsGetW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    private static partial uint NetUserModalsGet(
        string? server,
        int level,
        out IntPtr BufPtr);

    [LibraryImport("Netapi32.dll", SetLastError = true)]
    private static partial int NetApiBufferFree(IntPtr Buffer);
}