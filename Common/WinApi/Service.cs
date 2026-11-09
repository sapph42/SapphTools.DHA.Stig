using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static SapphTools.DHA.Stig.Common.WinApi.ScmApi;

namespace SapphTools.DHA.Stig.Common.WinApi;
public class Service : IDisposable {
    private bool disposedValue;
    internal SafeScmHandle? _serviceHandle;
    private ServiceState _originalState;
    private string _name;
    private string? _binPath;
    private ServiceManager _manager;

    public string BinPath {
        get {
            if (_binPath is null) {
                GetBinPath();
            }
            return _binPath ?? "";
        }
    }
    public string Name => _name;

    public Service(ServiceManager manager, string name) {
        _manager = manager;
        _name = name;
        GetBinPath();
    }
    public void SetHandle(SafeScmHandle handle) {
        _serviceHandle = handle;
    }
    public void SetState(ServiceState state) {
        _originalState = state;
    }
    private void GetBinPath() {
        OpenService();
        int bytesNeeded = 0;
        QueryServiceConfig(_serviceHandle, IntPtr.Zero, 0, out bytesNeeded);
        int lastError = Marshal.GetLastPInvokeError();
        if (lastError != 0 && lastError != 122 && lastError != 234) {
            OpenService(true);
            QueryServiceConfig(_serviceHandle, IntPtr.Zero, 0, out bytesNeeded);
            lastError = Marshal.GetLastPInvokeError();
        }
        if (lastError != 122 && lastError != 234) {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
        IntPtr buffer = Marshal.AllocHGlobal(bytesNeeded);
        try {
            if (!QueryServiceConfig(_serviceHandle, buffer, bytesNeeded, out bytesNeeded)) {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }
            int structSize = Marshal.SizeOf<QUERY_SERVICE_CONFIG>();

            QUERY_SERVICE_CONFIG config = Marshal.PtrToStructure<QUERY_SERVICE_CONFIG>(buffer);
            _binPath = config.lpBinaryPathName;
        } catch (Exception ex) {
            Console.WriteLine($"Service.GetBinPath exception :: {ex.Message}");
        } finally {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [MemberNotNull(nameof(_serviceHandle))]
    private void OpenService(bool force = false) {
        if (!force) {
            if (_serviceHandle is not null) {
                if (!_serviceHandle.IsClosed && !_serviceHandle.IsInvalid && _serviceHandle.DangerousGetHandle() != IntPtr.Zero) {
                    return;
                }
                CloseService(false);
            }
        } else {
            CloseService(false);
        }
        _manager.GetServiceHandle(this);
        if (_serviceHandle is null) {
            throw new Exception("Make a better exception later");
        }
    }
    internal void CloseService(bool dispose) {
        if (dispose) {
            Dispose();
            return;
        }
        _serviceHandle?.Dispose();
        _serviceHandle = null;
    }
    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // TODO: dispose managed state (managed objects)
            }
            CloseService(false);
            disposedValue = true;
        }
    }

    ~Service() {
        Dispose(disposing: false);
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
