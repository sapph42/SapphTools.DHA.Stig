using SapphTools.DHA.Stig.Common.Classes;
using SapphTools.SecurityDescriptor.Classes;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SapphTools.DHA.Stig.Common.WinApi;

public partial class LsaConnection : IDisposable {
    private bool disposedValue;
    private SafeLsaPolicyHandle? _policyHandle;
    private IntendedOperations _ops = IntendedOperations.None;
    private readonly LsaPrivilege _priv;
    private readonly HashSet<SecurityIdentifier> _sids = [];
    private readonly SeRightsValue _rightsValue;
    [Flags]
    public enum IntendedOperations : uint {
        None = 0,
        EnumerateAccounts = PolicyAccessMask.POLICY_LOOKUP_NAMES | PolicyAccessMask.POLICY_VIEW_LOCAL_INFORMATION,
        AddAccountRights = PolicyAccessMask.POLICY_LOOKUP_NAMES | PolicyAccessMask.POLICY_CREATE_ACCOUNT,
        RemoveAccountRights = PolicyAccessMask.POLICY_LOOKUP_NAMES,
        All = EnumerateAccounts | AddAccountRights | RemoveAccountRights
    }
    #region ctor
    public LsaConnection(string? computername, SeRightsValue value) {
        _rightsValue = value;
        _priv = value.Target;
        _sids = [.. value.AccountNames.Select(t => t.NativeSid)];
        try {
            OpenPolicy(computername, IntendedOperations.All);
        } catch (Exception ex) {
            throw new LsaException(
                LsaExceptionReason.OpenPolicy,
                ex
            );
        }
    }
    #endregion ctor
    #region Public Instance Methods
    public void SetAccountRights() {
        HashSet<SecurityIdentifier> activeSids;
        try {
            activeSids = QueryForRight(_priv);
        } catch (Exception ex) {
            throw new LsaException(LsaExceptionReason.QueryRight, ex);
        }
        foreach(SecurityIdentifier sid in activeSids.Except(_sids)) {
            using SafeHBuffer buff = SidToSidBuffer(sid) ?? throw new LsaException(LsaExceptionReason.SidBufferCreation);
            try {
                RemoveAccountRights(buff, _priv);
            } catch (Exception ex) {
                throw new LsaException(LsaExceptionReason.RemoveAccountFromRight, ex);
            }
        }
        foreach (SecurityIdentifier sid in _sids.Except(activeSids)) {
            using SafeHBuffer buff = SidToSidBuffer(sid) ?? throw new LsaException(LsaExceptionReason.SidBufferCreation);
            try {
                AddAccountRights(buff, _priv);
            } catch (Win32Exception wex) when (wex.NativeErrorCode == 0x2) {
                throw new LsaException(LsaExceptionReason.ObjectNameNotFound, wex);
            } catch (Exception ex) {
                throw new LsaException(LsaExceptionReason.AddAccountToRight, ex);
            }
        }
    }
    public SeRightsValue GetBeforeValue() {
        HashSet<SecurityIdentifier> activeSids = QueryForRight(_priv);
        List<Trustee> sids = [];
        foreach(SecurityIdentifier sid in activeSids) {
            sids.Add(Trustee.Construct(sid));
        }
        return new() {
            AccountNames = [..sids],
            Target = _priv
        };
    }
    public bool RightMatchesList() {
        try {
            HashSet<SecurityIdentifier> activeSids = QueryForRight(_priv);
            return _sids.SetEquals(activeSids);
        } catch (Exception ex) {
            throw new LsaException(LsaExceptionReason.QueryRight, ex);
        }
    }
    #endregion Public Instance Methods
    #region Private Instance Methods
    private void AddAccountRights(SafeHBuffer sid, params LsaPrivilege[] privs) {
        if (_policyHandle is null || _policyHandle.IsInvalid) {
            throw new InvalidOperationException("No valid policy handle.");
        }
        Dictionary<LsaUnicodeStringWrapper, bool> privWrappers = [];
        foreach (LsaPrivilege priv in privs) {
            privWrappers.Add(new(priv), false);
        }
        bool policySuccess = false;
        bool sidSuccess = false;
        try {
            _policyHandle.DangerousAddRef(ref policySuccess);
            sid.DangerousAddRef(ref sidSuccess);
            List<LsaUnicodeString> strings = [];
            foreach (LsaUnicodeStringWrapper key in privWrappers.Keys) {
                bool success = false;
                strings.Add(
                    key.DangerousGetStruct(ref success)
                );
                privWrappers[key] = success;
            }
            uint status = LsaAddAccountRights(
                _policyHandle.DangerousGetHandle(),
                sid.DangerousGetHandle(),
                [.. strings],
                (uint)strings.Count
            );
            strings.Clear();
            if (status != 0) {
                throw new Win32Exception(LsaNtStatusToWinError(status));
            }
        } finally {
            if (policySuccess) {
                _policyHandle.DangerousRelease();
            }
            if (sidSuccess) {
                sid.DangerousRelease();
            }
            foreach (LsaUnicodeStringWrapper key in privWrappers.Keys) {
                if (privWrappers[key]) {
                    key.DangerousReleaseStructBuffer();
                }
            }
        }
    }
    private void ClosePolicy(bool dispose) {
        if (dispose) {
            Dispose();
        } else {
            if (_policyHandle is null) {
                return;
            } else {
                bool success = false;
                try {
                    _policyHandle.DangerousAddRef(ref success);
                } catch (ObjectDisposedException) {
                    _policyHandle = null;
                    return;
                }
                if (success) {
                    try {
                        uint status = LsaClose(_policyHandle.DangerousGetHandle());
                        if (status == 0) {
                            _policyHandle.DangerousRelease();
                            _policyHandle.Dispose();
                            _policyHandle = null;
                        } else {
                            throw new System.ComponentModel.Win32Exception(LsaNtStatusToWinError(status));
                        }
                    } catch (ObjectDisposedException) {
                        _policyHandle = null;
                        return;
                    } catch {
                        _policyHandle?.DangerousRelease();
                        throw;
                    }
                }
                // DangerousAddReg should only ever throw or set success = true, this is unreachable code
                return;
            }
        }
    }
    private void OpenPolicy(string? computerName, IntendedOperations ops) {
        if (_policyHandle is not null) {
            if (!_policyHandle.IsClosed && !_policyHandle.IsInvalid && _policyHandle.DangerousGetHandle() != IntPtr.Zero) {
                return;
            }
            ClosePolicy(false);
        }
        LsaObjectAttributes defaultAttributes = new();
        if (computerName is not null) {
            using SafeHBuffer systemBuffer = SafeHBuffer.AllocFactory(computerName.Length + 2);
            systemBuffer.WriteArray(
                0,
                System.Text.Encoding.Unicode.GetBytes(computerName),
                0,
                computerName.Length
            );
            systemBuffer.Write((ulong)computerName.Length, (short)0);
            bool success = false;
            try {
                systemBuffer.DangerousAddRef(ref success);
                if (success) {
                    LsaUnicodeString systemName = new() {
                        Length = (ushort)computerName.Length,
                        MaximumLength = (ushort)(computerName.Length + 2),
                        Buffer = systemBuffer.DangerousGetHandle()
                    };
                    uint remoteResult = LsaOpenPolicy(ref systemName, ref defaultAttributes, (uint)ops, out IntPtr remoteHandle);
                    if (remoteResult != 0) {
                        _policyHandle = null;
                        throw new System.ComponentModel.Win32Exception(LsaNtStatusToWinError(remoteResult));
                    } else {
                        _policyHandle = new(remoteHandle, true);
                        _ops = ops;
                        return;
                    }
                }
            } finally {
                if (success) {
                    systemBuffer.DangerousRelease();
                }
            }
            return;
        }
        uint localResult = LsaOpenPolicyLocal(IntPtr.Zero, ref defaultAttributes, (uint)ops, out IntPtr localHandle);
        if (localResult != 0) {
            _policyHandle = null;
            throw new System.ComponentModel.Win32Exception(LsaNtStatusToWinError(localResult));
        } else {
            _policyHandle = new(localHandle, true);
            _ops = ops;
            return;
        }
    }
    private HashSet<SecurityIdentifier> QueryForRight(LsaPrivilege right) {
        HashSet<SecurityIdentifier> sids = [];
        if (_policyHandle is null || _policyHandle.IsInvalid) {
            throw new InvalidOperationException("No valid policy handle.");
        }
        LsaUnicodeStringWrapper rightWrapper = new(right);
        bool policySuccess = false;
        bool wrapperSuccess = false;
        try {
            _policyHandle.DangerousAddRef(ref policySuccess);
            LsaUnicodeString priv = rightWrapper.DangerousGetStruct(ref wrapperSuccess);
            uint status = LsaEnumerateAccountsWithUserRight(
                _policyHandle.DangerousGetHandle(),
                priv,
                out nint buffer,
                out uint count
            );
            if (status != 0) {
                throw new System.ComponentModel.Win32Exception(LsaNtStatusToWinError(status));
            }
            nint offset = buffer;
            for (int i = 0; i < count; i++) {
                LsaEnumerationInfo lsaInfo = Marshal.PtrToStructure<LsaEnumerationInfo>(offset);
                sids.Add(
                    new(lsaInfo.PSid)
                );
                offset += Marshal.SizeOf(typeof(LsaEnumerationInfo));
            }
        } finally {
            if (policySuccess) {
                _policyHandle.DangerousRelease();
            }
            if (wrapperSuccess) {
                rightWrapper.DangerousReleaseStructBuffer();
            }
        }
        return sids;
    }
    private void RemoveAccountRights(SafeHBuffer sid, params LsaPrivilege[] privs) {
        if (_policyHandle is null || _policyHandle.IsInvalid) {
            throw new InvalidOperationException("No valid policy handle.");
        }
        Dictionary<LsaUnicodeStringWrapper, bool> privWrappers = [];
        foreach (LsaPrivilege priv in privs) {
            privWrappers.Add(new(priv), false);
        }
        bool policySuccess = false;
        bool sidSuccess = false;
        try {
            _policyHandle.DangerousAddRef(ref policySuccess);
            sid.DangerousAddRef(ref sidSuccess);
            List<LsaUnicodeString> strings = [];
            foreach (LsaUnicodeStringWrapper key in privWrappers.Keys) {
                bool success = false;
                strings.Add(
                    key.DangerousGetStruct(ref success)
                );
                privWrappers[key] = success;
            }
            uint status = LsaRemoveAccountRights(
                _policyHandle.DangerousGetHandle(),
                sid.DangerousGetHandle(),
                false,
                [.. strings],
                (uint)strings.Count
            );
            strings.Clear();
            if (status != 0) {
                throw new System.ComponentModel.Win32Exception(LsaNtStatusToWinError(status));
            }
        } finally {
            if (policySuccess) {
                _policyHandle.DangerousRelease();
            }
            if (sidSuccess) {
                sid.DangerousRelease();
            }
            foreach (LsaUnicodeStringWrapper key in privWrappers.Keys) {
                if (privWrappers[key]) {
                    key.DangerousReleaseStructBuffer();
                }
            }
        }
    }
    #endregion Private Instance Methods
    #region Static Methods
    private static SecurityIdentifier? AccountNameToSid(string accountName) {
        try {
            return (SecurityIdentifier)new NTAccount(accountName).Translate(typeof(SecurityIdentifier));
        } catch {
            return null;
        }
    }
    private static SafeHBuffer? SidToSidBuffer(SecurityIdentifier sid) {
        SafeHBuffer sidBuffer = new();
        try {
            byte[] sidBytes = new byte[sid.BinaryLength];
            sidBuffer = SafeHBuffer.AllocFactory(sidBytes.Length);
            sidBuffer.WriteArray(0, sidBytes, 0, sidBytes.Length);
            return sidBuffer;
        } catch {
            if (!sidBuffer.IsInvalid) {
                sidBuffer.Dispose();
            }
            return null;
        }
    }
    public static SafeHBuffer AccountNameToSidPtr(string accountName) {
        SafeHBuffer sidBuffer = new();
        try {
            NTAccount account = new(accountName);
            SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            byte[] sidBytes = new byte[sid.BinaryLength];
            sidBuffer = SafeHBuffer.AllocFactory(sidBytes.Length);
            sidBuffer.WriteArray(0, sidBytes, 0, sidBytes.Length);
            return sidBuffer;
        } catch {
            if (!sidBuffer.IsInvalid) {
                sidBuffer.Dispose();
            }
            return new SafeHBuffer();
        }
    }
    #endregion Static Methods
    #region IDisposable Implementation
    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // dispose managed state (managed objects)
            }
            ClosePolicy(false);
            disposedValue = true;
        }
    }
    ~LsaConnection() {
        Dispose(disposing: false);
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion IDisposable Implementation

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern uint LsaOpenPolicy(
        ref LsaUnicodeString SystemName,
        ref LsaObjectAttributes ObjectAttributes,
        uint DesiredAccess,
        out IntPtr PolicyHandle
    );
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern uint LsaOpenPolicyLocal(
        IntPtr SystemName,
        ref LsaObjectAttributes ObjectAttributes,
        uint DesiredAccess,
        out IntPtr PolicyHandle
    );

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern uint LsaEnumerateAccountsWithUserRight(
        IntPtr PolicyHandle,
        LsaUnicodeString UserRight,
        out IntPtr Enumeration,
        out uint CountReturned
    );

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern uint LsaAddAccountRights(
        IntPtr PolicyHandle,
        IntPtr AccountSid,
        LsaUnicodeString[] UserRights,
        uint CountOfRights
    );


    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern uint LsaRemoveAccountRights(
        IntPtr PolicyHandle,
        IntPtr AccountSid,
        bool AllRights,
        LsaUnicodeString[] UserRights,
        uint CountOfRights
    );

    [LibraryImport("advapi32.dll")]
    internal static partial uint LsaClose(IntPtr PolicyHandle);

    [LibraryImport("advapi32.dll")]
    private static partial uint LsaFreeMemory(IntPtr Buffer);

    [LibraryImport("advapi32.dll")]
    private static partial int LsaNtStatusToWinError(uint status);
}
