using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SapphTools.DHA.Stig.Remediator.TestSled.SledCatalog;

/// <summary>
/// Creates a catalog from a CDF, signs it with the user's code-signing certificate,
/// and verifies a member against the signed catalog. Windows-only.
/// </summary>
public static class CatalogApi {
    private const uint CRYPTCAT_VERSION_2 = 0x200;
    private const uint SIGNER_SUBJECT_FILE = 1;
    private const uint SIGNER_CERT_STORE = 2;
    private const uint SIGNER_CERT_POLICY_CHAIN_NO_ROOT = 8;
    private const uint CALG_SHA_256 = 0x0000800c;
    private const uint WTD_UI_NONE = 2;
    private const uint WTD_CHOICE_CATALOG = 2;
    private const string CodeSigningOid = "1.3.6.1.5.5.7.3.3";
    private static readonly IntPtr InvalidHandleValue = new(-1);
    private static readonly Guid GenericVerifyV2 = new("00AAC56B-CD44-11D0-8CC2-00C04FC295EE");

    /// <summary>
    /// Enumerates valid code-signing certificates associated with private keys in CurrentUser\My.
    /// HasPrivateKey describes the store's key association; a removed smart card may still fail at signing time.
    /// </summary>
    public static X509Certificate2Collection GetCodeSigningCertificates() {
        using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        X509Certificate2Collection result = [];
        DateTime now = DateTime.Now;
        foreach (X509Certificate2 certificate in store.Certificates) {
            if (!certificate.HasPrivateKey || now < certificate.NotBefore || now > certificate.NotAfter) {
                continue;
            }
            bool codeSigning = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>()
                .Any(eku => eku.EnhancedKeyUsages.Cast<Oid>()
                    .Any(usage => usage.Value == CodeSigningOid));
            bool canSign = certificate.Extensions.OfType<X509KeyUsageExtension>()
                .All(usage => (usage.KeyUsages & X509KeyUsageFlags.DigitalSignature) != 0);
            if (codeSigning && canSign) {
                result.Add(certificate);
            }
        }
        return result;
    }

    /// <summary>
    /// Selects automatically if there is one candidate; otherwise uses the standard Windows certificate UI.
    /// Returns null if the user cancels the selection dialog.
    /// </summary>
    public static X509Certificate2? SelectSigningCertificate(IntPtr ownerWindow = default) {
        X509Certificate2Collection candidates = GetCodeSigningCertificates();
        if (candidates.Count == 0) {
            throw new InvalidOperationException(
                "No currently valid Code Signing certificate with an associated private key was found in CurrentUser\\My.");
        }
        if (candidates.Count == 1) {
            return candidates[0];
        }
        if (!Environment.UserInteractive) {
            throw new InvalidOperationException("Certificate selection requires an interactive Windows session.");
        }
        X509Certificate2Collection selected = X509Certificate2UI.SelectFromCollection(
            candidates, "Test Sled catalog signing", "Select a Code Signing certificate",
            X509SelectionFlag.SingleSelection, ownerWindow);
        return selected.Count == 0 ? null : selected[0];
    }

    /// <summary>
    /// Generates the unsigned catalog from a CDF. The CDF controls the catalog's output name/directory.
    /// CatalogVersion=2 and HashAlgorithms=SHA256 are required by the bundled TestSled.cdf.
    /// </summary>
    public static void CreateCatalogFile(string cdfPath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(cdfPath);
        List<string> errors = [];
        CdfParseErrorCallback errorCallback = (area, code, line) =>
            errors.Add($"Area {area}, error {code}: {line}");
        IntPtr cdf = IntPtr.Zero;
        bool closed = false;
        try {
            cdf = CryptCATCDFOpen(Path.GetFullPath(cdfPath), errorCallback);
            if (cdf == IntPtr.Zero) {
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    $"Could not open catalog definition file '{cdfPath}'. {string.Join("; ", errors)}");
            }
            IntPtr previousTag = IntPtr.Zero;
            IntPtr member = IntPtr.Zero;
            int count = 0;
            do {
                // The returned tag and member are owned by the CDF API; neither is ours to free.
                previousTag = CryptCATCDFEnumMembersByCDFTagEx(
                    cdf, previousTag, errorCallback, ref member, false, IntPtr.Zero);
                if (previousTag != IntPtr.Zero) count++;
            } while (previousTag != IntPtr.Zero && errors.Count == 0);
            if (errors.Count != 0) {
                throw new InvalidDataException($"Invalid catalog definition: {string.Join("; ", errors)}");
            }
            if (count == 0) {
                throw new InvalidDataException("The CDF did not produce any catalog members.");
            }
            // Closing persists the catalog, even when its signature has not yet been applied.
            if (!CryptCATCDFClose(cdf)) {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not persist the generated catalog.");
            }
            closed = true;
        } finally {
            if (cdf != IntPtr.Zero && !closed) {
                CryptCATCDFClose(cdf);
            }
            GC.KeepAlive(errorCallback);
        }
    }

    /// <summary>
    /// Prompts for the certificate if needed, generates the catalog, then signs the catalog itself.
    /// The caller supplies the path where the CDF's Name/ResultDir settings write the .cat file.
    /// </summary>
    public static void CreateAndSignCatalogFile(string cdfPath, string catalogPath, IntPtr ownerWindow = default) {
        X509Certificate2 certificate = SelectSigningCertificate(ownerWindow)
            ?? throw new OperationCanceledException("Code Signing certificate selection was cancelled.");
        CreateCatalogFile(cdfPath);
        SignCatalogFile(catalogPath, certificate);
    }

    /// <summary>
    /// Creates an Authenticode signature on the .cat file using the selected certificate's
    /// existing smart-card-backed key association. No PIN is requested or stored by this application.
    /// A real key operation triggers the Windows smart-card provider's PIN UI if authentication is needed.
    /// </summary>
    public static unsafe void SignCatalogFile(string catalogPath, X509Certificate2 certificate) {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogPath);
        ArgumentNullException.ThrowIfNull(certificate);
        if (!certificate.HasPrivateKey) {
            throw new CryptographicException("The selected certificate has no associated private key.");
        }
        string fullPath = Path.GetFullPath(catalogPath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Catalog was not generated.", fullPath);
        if (!Environment.UserInteractive) {
            throw new InvalidOperationException("Smart-card signing requires an interactive Windows session.");
        }

        // Explicit proof of possession: request a real private-key signature. For a CAC,
        // the CSP/KSP handles PIN entry using its own secure UI. Authentication may already
        // be cached, in which case no prompt is necessary. Never accept a PIN as a string.
        byte[] challenge = SHA256.HashData(RandomNumberGenerator.GetBytes(32));
        using (RSA? rsa = certificate.GetRSAPrivateKey()) {
            if (rsa is not null) {
                _ = rsa.SignHash(challenge, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            } else {
                using ECDsa? ecdsa = certificate.GetECDsaPrivateKey();
                if (ecdsa is null)
                    throw new NotSupportedException("The signing key must support RSA or ECDSA.");
                _ = ecdsa.SignHash(challenge);
            }
        }

        IntPtr signerContext = IntPtr.Zero;
        fixed (char* fileName = fullPath) {
            SignerFileInfo file = new() {
                cbSize = (uint)sizeof(SignerFileInfo),
                pwszFileName = fileName
            };
            SignerSubjectInfo subject = new() {
                cbSize = (uint)sizeof(SignerSubjectInfo),
                pdwIndex = null,
                dwSubjectChoice = SIGNER_SUBJECT_FILE,
                pSignerFileInfo = &file
            };
            SignerCertStoreInfo storeInfo = new() {
                cbSize = (uint)sizeof(SignerCertStoreInfo),
                pSigningCert = certificate.Handle,
                dwCertPolicy = SIGNER_CERT_POLICY_CHAIN_NO_ROOT
            };
            SignerCert signer = new() {
                cbSize = (uint)sizeof(SignerCert),
                dwCertChoice = SIGNER_CERT_STORE,
                pCertStoreInfo = &storeInfo
            };
            SignerSignatureInfo signature = new() {
                cbSize = (uint)sizeof(SignerSignatureInfo),
                algidHash = CALG_SHA_256
            };
            try {
                int hr = SignerSignEx(0, &subject, &signer, &signature,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out signerContext);
                if (hr < 0) Marshal.ThrowExceptionForHR(hr);
            } finally {
                if (signerContext != IntPtr.Zero) SignerFreeSignerContext(signerContext);
                GC.KeepAlive(certificate);
            }
        }
    }

    /// <summary>Enumerates catalog reference tags. This does NOT validate the catalog's signature.</summary>
    public static List<string> ReadCatalogFile(string catalogPath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogPath);
        IntPtr catalog = CryptCATOpen(Path.GetFullPath(catalogPath), 0,
            IntPtr.Zero, CRYPTCAT_VERSION_2, 0);
        if (catalog == InvalidHandleValue || catalog == IntPtr.Zero) {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to open catalog.");
        }
        try {
            List<string> tags = [];
            IntPtr memberPtr = IntPtr.Zero;
            while ((memberPtr = CryptCATEnumerateMember(catalog, memberPtr)) != IntPtr.Zero) {
                CryptCatMember member = Marshal.PtrToStructure<CryptCatMember>(memberPtr);
                string? tag = Marshal.PtrToStringUni(member.pwszReferenceTag);
                if (!string.IsNullOrEmpty(tag)) tags.Add(tag);
            }
            return tags;
        } finally {
            CryptCATClose(catalog);
        }
    }

    /// <summary>
    /// Verifies a catalog member's SHA-256 digest and the catalog's Windows trust policy.
    /// Do not replace this check with ReadCatalogFile, which only enumerates untrusted metadata.
    /// </summary>
    public static unsafe void VerifyCatalogMember(string catalogPath, string memberPath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberPath);
        string fullCatalog = Path.GetFullPath(catalogPath);
        string fullMember = Path.GetFullPath(memberPath);
        if (!File.Exists(fullCatalog)) throw new FileNotFoundException("Catalog not found.", fullCatalog);

        if (!CryptCATAdminAcquireContext2(out IntPtr admin, IntPtr.Zero, "SHA256", IntPtr.Zero, 0)) {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to initialize SHA-256 catalog verification.");
        }
        try {
            using FileStream memberFile = File.OpenRead(fullMember);
            byte[] digest = new byte[32];
            uint digestLength = (uint)digest.Length;
            if (!CryptCATAdminCalcHashFromFileHandle2(
                    admin, memberFile.SafeFileHandle.DangerousGetHandle(), ref digestLength, digest, 0)
                    || digestLength != digest.Length) {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to hash the catalog member.");
            }
            string tag = Convert.ToHexString(digest);
            fixed (char* pCatalog = fullCatalog, pMember = fullMember, pTag = tag)
            fixed (byte* pDigest = digest) {
                WintrustCatalogInfo catalogInfo = new() {
                    cbStruct = (uint)sizeof(WintrustCatalogInfo),
                    dwCatalogVersion = 2,
                    pcwszCatalogFilePath = pCatalog,
                    pcwszMemberTag = pTag,
                    pcwszMemberFilePath = pMember,
                    hMemberFile = memberFile.SafeFileHandle.DangerousGetHandle(),
                    pbCalculatedFileHash = pDigest,
                    cbCalculatedFileHash = digestLength,
                    hCatAdmin = admin
                };
                WintrustData trust = new() {
                    cbStruct = (uint)sizeof(WintrustData),
                    dwUIChoice = WTD_UI_NONE,
                    dwUnionChoice = WTD_CHOICE_CATALOG,
                    pCatalog = (IntPtr)(&catalogInfo)
                };
                Guid action = GenericVerifyV2;
                int status = WinVerifyTrust(IntPtr.Zero, ref action, ref trust);
                if (status != 0) {
                    throw new CryptographicException(
                        $"Catalog signature or member verification failed (0x{unchecked((uint)status):X8}).");
                }
            }
        } finally {
            CryptCATAdminReleaseContext(admin, 0);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CryptoBlob {
        public uint cbData;
        public IntPtr pbData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CryptCatMember {
        public uint cbStruct;
        public IntPtr pwszReferenceTag;
        public IntPtr pwszFileName;
        public Guid gSubjectType;
        public uint fdwMemberFlags;
        public IntPtr pIndirectData;
        public uint dwCertVersion;
        public uint dwReserved;
        public IntPtr hReserved;
        public CryptoBlob sEncodedIndirectData;
        public CryptoBlob sEncodedMemberInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct SignerFileInfo {
        public uint cbSize;
        public char* pwszFileName;
        public IntPtr hFile;
    }
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct SignerSubjectInfo {
        public uint cbSize;
        public uint* pdwIndex;
        public uint dwSubjectChoice;
        public SignerFileInfo* pSignerFileInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct SignerCertStoreInfo {
        public uint cbSize;
        public IntPtr pSigningCert;
        public uint dwCertPolicy;
        public IntPtr hCertStore;
    }
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct SignerCert {
        public uint cbSize;
        public uint dwCertChoice;
        public SignerCertStoreInfo* pCertStoreInfo;
        public IntPtr hwnd;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct SignerSignatureInfo {
        public uint cbSize;
        public uint algidHash;
        public uint dwAttrChoice;
        public IntPtr pAttrAuthcode;
        public IntPtr psAuthenticated;
        public IntPtr psUnauthenticated;
    }
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct WintrustCatalogInfo {
        public uint cbStruct;
        public uint dwCatalogVersion;
        public char* pcwszCatalogFilePath;
        public char* pcwszMemberTag;
        public char* pcwszMemberFilePath;
        public IntPtr hMemberFile;
        public byte* pbCalculatedFileHash;
        public uint cbCalculatedFileHash;
        public IntPtr pcCatalogContext;
        public IntPtr hCatAdmin;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct WintrustData {
        public uint cbStruct;
        public IntPtr pPolicyCallbackData;
        public IntPtr pSIPClientData;
        public uint dwUIChoice;
        public uint fdwRevocationChecks;
        public uint dwUnionChoice;
        public IntPtr pCatalog;
        public uint dwStateAction;
        public IntPtr hWVTStateData;
        public IntPtr pwszURLReference;
        public uint dwProvFlags;
        public uint dwUIContext;
        public IntPtr pSignatureSettings;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
    private delegate void CdfParseErrorCallback(uint errorArea, uint localError,
        [MarshalAs(UnmanagedType.LPWStr)] string line);

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptCATCDFClose(IntPtr cdf);

    [DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CryptCATCDFOpen(
        [MarshalAs(UnmanagedType.LPWStr)] string cdfPath, CdfParseErrorCallback callback);

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr CryptCATCDFEnumMembersByCDFTagEx(
        IntPtr cdf, IntPtr previousTag, CdfParseErrorCallback callback, ref IntPtr member,
        [MarshalAs(UnmanagedType.Bool)] bool continueOnError, IntPtr reserved);

    [DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CryptCATOpen(
        [MarshalAs(UnmanagedType.LPWStr)] string path, uint flags,
        IntPtr cryptProvider, uint version, uint encoding);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptCATClose(IntPtr catalog);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    private static extern IntPtr CryptCATEnumerateMember(IntPtr catalog, IntPtr previousMember);

    [DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptCATAdminAcquireContext2(
        out IntPtr catalogAdmin, IntPtr subsystem, string hashAlgorithm, IntPtr strongSignPolicy, uint flags);

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptCATAdminCalcHashFromFileHandle2(
        IntPtr catalogAdmin, IntPtr file, ref uint hashSize, [Out] byte[] hash, uint flags);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptCATAdminReleaseContext(IntPtr catalogAdmin, uint flags);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    private static extern int WinVerifyTrust(IntPtr hwnd, ref Guid action, ref WintrustData data);

    [DllImport("mssign32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    private static extern unsafe int SignerSignEx(uint flags, SignerSubjectInfo* subject,
        SignerCert* certificate, SignerSignatureInfo* signature, IntPtr provider,
        IntPtr timestamp, IntPtr request, IntPtr sipData, out IntPtr signerContext);

    [DllImport("mssign32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    private static extern int SignerFreeSignerContext(IntPtr signerContext);
}
