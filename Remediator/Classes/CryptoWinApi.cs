using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SapphTools.DHA.Stig.Remediator.Classes; 
internal static partial class CryptoWinApi {
    private const uint DEFAULT_ENCODING = 0;
    private const int CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;
    private const int CERT_SYSTEM_STORE_LOCAL_MACHINE_ID = 2;
    private static readonly uint CERT_SYSTEM_STORE_LOCAL_MACHINE = (CERT_SYSTEM_STORE_LOCAL_MACHINE_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT);
    private const uint CERT_STORE_OPEN_EXISTING = 0x00004000;
    private const uint CERT_STORE_READONLY = 0x00008000;
    private const uint CERT_STORE_ADD_NEW = 1;
    private const uint CERT_STORE_ADD_USE_EXISTING = 2;
    private const uint CERT_STORE_ADD_REPLACE_EXISTING = 3;
    private const uint CERT_SHA1_HASH_PROP_ID = 3;
    private static readonly IntPtr CERT_STORE_PROV_FILENAME_W = new(8);
    private static readonly IntPtr CERT_STORE_PROV_SYSTEM_W = new(10);

    public static void AddCerts(string computerName, string store, string artifactPath, out int success, out int noAction, out int failed) {
        using SafeCertStoreHandle artifactStore = OpenSerializedStore(artifactPath);
        using SafeCertStoreHandle remoteStore = OpenRemoteMachineStore(computerName, store);
        HashSet<string> existing = GetThumbprints(remoteStore);
        int successL = 0;
        int noActionL = 0;
        int failedL = 0;
        Enumerate(artifactStore, context => {
            string thumbprint = Convert.ToHexString(GetThumbprint(context));
            if (existing.Contains(thumbprint)) {
                noActionL++;
                return;
            }
            try {
                AddCert(remoteStore, context);
                existing.Add(thumbprint);
                successL++;
            } catch {
                failedL++;
            }
        });
        success = successL;
        noAction = noActionL;
        failed = failedL;
    }
    internal static void AddCert(SafeCertStoreHandle target, IntPtr context) {
        if (!CertAddCertificateContextToStore(
                target,
                context,
                CERT_STORE_ADD_NEW,
                IntPtr.Zero)) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
    internal static void Enumerate(SafeCertStoreHandle store, Action<IntPtr> action) {
        IntPtr current = IntPtr.Zero;
        try {
            while (true) {
                current = CertEnumCertificatesInStore(store, current);
                if (current == IntPtr.Zero) {
                    break;
                }
                action(current);
            }
        } finally {
            if (current != IntPtr.Zero) {
                CertFreeCertificateContext(current);
            }
        }
    }
    internal static byte[] GetThumbprint(IntPtr context) {
        byte[] hash = new byte[20];
        uint size = (uint)hash.Length;
        unsafe {
            fixed (byte* pResult = hash) {
                if (!CertGetCertificateContextProperty(context, CERT_SHA1_HASH_PROP_ID, pResult, ref size)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }
        return hash!;
    }
    internal static HashSet<string> GetThumbprints(SafeCertStoreHandle store) {
        HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);
        Enumerate(store, context => {
            byte[] hash = GetThumbprint(context);
            result.Add(Convert.ToHexString(hash));
        });
        return result;
    }
    internal static SafeCertStoreHandle OpenRemoteMachineStore(string computerName, string store) {
        string target = $@"\\{computerName}\{store}";
        SafeCertStoreHandle handle = CertOpenStore(
            CERT_STORE_PROV_SYSTEM_W,
            DEFAULT_ENCODING,
            IntPtr.Zero,
            CERT_STORE_OPEN_EXISTING | CERT_STORE_OPEN_EXISTING,
            target
        );
        if (handle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Unable to open serialized certificate store at {target}");
        }
        return handle;
    }
    internal static SafeCertStoreHandle OpenSerializedStore(string path) {
        SafeCertStoreHandle handle = CertOpenStore(
            CERT_STORE_PROV_FILENAME_W,
            DEFAULT_ENCODING,
            IntPtr.Zero,
            CERT_STORE_OPEN_EXISTING | CERT_STORE_READONLY,
            path
        );
        if (handle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Unable to open serialized certificate store at {path}");
        }
        return handle;
    }

    [LibraryImport("crypt32.dll", 
        SetLastError = true, 
        StringMarshalling = StringMarshalling.Utf16
    )]
    private static partial SafeCertStoreHandle CertOpenStore(
        IntPtr lpszProvider, 
        uint dwEncoding, 
        IntPtr hCryptProv, 
        uint dwFlags,
        [MarshalAs(UnmanagedType.LPWStr)] string pvPara
    );

    [LibraryImport("crypt32.dll")]
    private static partial IntPtr CertEnumCertificatesInStore(SafeCertStoreHandle hCertStore, IntPtr pPrevCertContext);

    [LibraryImport("crypt32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool CertGetCertificateContextProperty(
        IntPtr pCertContext,
        uint dwPropId,
        void* pvData,
        ref uint pcbData
    );

    [LibraryImport("crypt32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CertAddCertificateContextToStore(
        SafeCertStoreHandle hCertStore, 
        IntPtr pCertContext, 
        uint dwDispo, 
        IntPtr ppStoreContext
    );

    [LibraryImport("crypt32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CertFreeCertificateContext(IntPtr pCertContext);

    [LibraryImport("crypt32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CertCloseStore(IntPtr hCertStore, uint dwFlags);
}
internal sealed class SafeCertStoreHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public SafeCertStoreHandle() : base(true) { }
    protected override bool ReleaseHandle() => CryptoWinApi.CertCloseStore(handle, 0);
}