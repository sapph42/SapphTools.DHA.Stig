using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SapphTools.DHA.Stig.Remediator.TestSled.SledCatalog;

[StructLayout(LayoutKind.Sequential)]
public struct CRYPTOAPI_BLOB {
    public uint cbData;
    public IntPtr pbData;
}

[StructLayout(LayoutKind.Sequential)]
public struct GUID {
    int a;
    short b;
    short c;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    byte[] d;
}

[StructLayout(LayoutKind.Sequential)]
public class CRYPTCATMEMBER {
    public uint cbStruct;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string pwszReferenceTag;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string pwszFileName;
    public GUID gSubjectType;
    public uint fdwMemberFlags;
    public IntPtr pIndirectData;
    public uint dwCertVersion;
    public uint dwReserved;
    public IntPtr hReserved;
    public CRYPTOAPI_BLOB sEncodedIndirectData;
    public CRYPTOAPI_BLOB sEncodedMemberInfo;
}

[StructLayout(LayoutKind.Sequential)]
public struct CRYPTCATCDF {
    uint cbStruct;
    IntPtr hFile;
    uint dwCurFilePos;
    uint dwLastMemberOffset;
    bool fEOF;
    [MarshalAs(UnmanagedType.LPWStr)]
    string pwszResultDir;
    IntPtr hCATStore;
}

[StructLayout(LayoutKind.Sequential)]
public struct SIP_INDIRECT_DATA {
    public CRYPT_ATTRIBUTE_TYPE_VALUE Data;
    public CRYPT_ALGORITHM_IDENTIFIER DigestAlgorithm;
    public CRYPTOAPI_BLOB Digest;
}

[StructLayout(LayoutKind.Sequential)]
public struct CRYPT_ATTRIBUTE_TYPE_VALUE {
    [MarshalAs(UnmanagedType.LPStr)]
    public string pszObjId;
    public CRYPTOAPI_BLOB Value;
}

[StructLayout(LayoutKind.Sequential)]
public struct CRYPT_ALGORITHM_IDENTIFIER {
    [MarshalAs(UnmanagedType.LPStr)]
    public string pszObjId;
    public CRYPTOAPI_BLOB Parameters;
}

public static partial class CatalogApi {
    private static readonly IntPtr INVALID_HANDLE_VALUE = IntPtr.Zero;
    private const uint PROV_RSA_FULL = 1;
    private const uint CRYPTCAT_VERSION_1 = 0x100;
    private const long NTE_BAD_KEYSET = 0x80090016L;
    private const uint CRYPT_NEWKEYSET = 0x00000008;

    public static void CreateCatalogFile(string fileName) {
        CRYPTCATMEMBER? ccm = null;
        try {
            PFN_CDF_PARSE_ERROR_CALLBACK pfn = ParseErrorCallback;
            string? s = null;

            IntPtr cdfPtr = CryptCATCDFOpen(fileName, Marshal.GetFunctionPointerForDelegate(pfn));
            if (cdfPtr == IntPtr.Zero) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            if (Marshal.PtrToStructure(cdfPtr, typeof(CRYPTCATCDF)) is not CRYPTCATCDF cdf) {
                throw new Exception("I'm not sure how this happens");
            }
            do {
                ccm = new CRYPTCATMEMBER {
                    pIndirectData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SIP_INDIRECT_DATA)))
                };
                unsafe {
                    IntPtr ptr = Marshal.StringToHGlobalUni(s);
                    s = CryptCATCDFEnumMembersByCDFTagEx(cdfPtr, ptr.ToPointer(), Marshal.GetFunctionPointerForDelegate(pfn), ccm, true, IntPtr.Zero);
                }
            } while (s != null);
            CryptCATCDFClose(cdfPtr);
        } catch {
            throw;
        } finally {
            if (ccm != null) {
                Marshal.FreeHGlobal(ccm.pIndirectData);
            }
        }
    }
    public static List<string> ReadCatalogFile(string fileName) {
        List<string> data = [];
        IntPtr cryptProv = IntPtr.Zero;
        IntPtr hCatalog = IntPtr.Zero;
        try {
            bool status = CryptAcquireContext(out cryptProv, IntPtr.Zero, IntPtr.Zero, PROV_RSA_FULL, 0);
            if (status == false) {
                uint err = (uint)Marshal.GetLastWin32Error();
                if (err == NTE_BAD_KEYSET) {
                    status = CryptAcquireContext(out cryptProv, IntPtr.Zero, IntPtr.Zero, PROV_RSA_FULL, CRYPT_NEWKEYSET);
                    if (status == false) {
                        err = (uint)Marshal.GetLastWin32Error();
                        throw new Exception("Error in CryptAcquireContext: " + err);
                    }
                } else {
                    throw new Exception("Error in CryptAcquireContext: " + err);
                }
            }

            hCatalog = CryptCATOpen(fileName, 0, cryptProv, CRYPTCAT_VERSION_1, 0x00000001);

            if (hCatalog == INVALID_HANDLE_VALUE) {
                CryptReleaseContext(cryptProv, 0);
                throw new Exception("Unable to read catalog file");
            }

            IntPtr pMemberPtr = CryptCATEnumerateMember(hCatalog, IntPtr.Zero);
            while (pMemberPtr != IntPtr.Zero && Marshal.PtrToStructure(pMemberPtr, typeof(CRYPTCATMEMBER)) is CRYPTCATMEMBER cdf) {
                data.Add(cdf.pwszReferenceTag);
                pMemberPtr = CryptCATEnumerateMember(hCatalog, pMemberPtr);
            }
            return data;
        } finally {
            if (hCatalog != IntPtr.Zero) {
                CryptCATClose(hCatalog);
            }
            if (cryptProv != IntPtr.Zero) {
                CryptReleaseContext(cryptProv, 0);
            }
        }

    }
    private static void ParseErrorCallback(uint u1, uint u2, string s) {
        Console.WriteLine(u1 + " " + u2 + " " + s);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void PFN_CDF_PARSE_ERROR_CALLBACK(
        [In] uint dwErrorArea,
        [In] uint dwLocalError,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszLine
    );

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptCATCDFClose([In] IntPtr pCDF);

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CryptCATCDFOpen(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilePath,
        [In, Optional] IntPtr pfnParseError
    );

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.LPWStr)]
    private static extern unsafe string CryptCATCDFEnumMembersByCDFTagEx(
        [In] IntPtr pCDF,
        [In, Out] void* pwszPrevCDFTag,
        [In] IntPtr pfnParseError,
        [In] CRYPTCATMEMBER ppMember,
        [In] bool fContinueOnError,
        [In] IntPtr pvReserved
    );

    [DllImport("kernel32.dll")]
    private static extern uint GetLastError();

    [LibraryImport("Advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CryptAcquireContext(
        out IntPtr hCryptProv,
        IntPtr container,
        IntPtr provider,
        uint providerType,
        uint flags);

    [LibraryImport("Advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CryptReleaseContext(IntPtr hCryptProv, uint flags);

    [LibraryImport("Wintrust.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CryptCATClose(IntPtr hProv);

    [DllImport("Wintrust.dll", SetLastError = true)]
    private static extern IntPtr CryptCATOpen(
    [param: MarshalAs(UnmanagedType.LPTStr)] string pwszFileName,
        uint fdwOpenFlags,
        IntPtr hProv,
        uint dwPublicVersion,
        uint dwEncodingType);

    [LibraryImport("Wintrust.dll")]
    private static unsafe partial IntPtr CryptCATEnumerateMember(IntPtr hCatalog, IntPtr pPrevMember);
}
