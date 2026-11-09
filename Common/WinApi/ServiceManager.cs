using SapphTools.IDisposableEnumerable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static SapphTools.DHA.Stig.Common.WinApi.ScmApi;

namespace SapphTools.DHA.Stig.Common.WinApi;
public partial class ServiceManager : IDisposable {
    private readonly object _lock = new();
    private readonly DisposableDictionary<string, Service> Services = [];
    private readonly string? _computerName;
    private bool disposedValue;
    private SafeScmHandle? _scmHandle;

    public ServiceManager(string? computerName) {
        _computerName = computerName;
        OpenServiceManager();
    }
    public void FetchServiceData(string? serviceName = null) {
        OpenServiceManager();
        int resumeHandle = 0, bytesNeeded = 0, servicesReturned = 0;
        EnumServicesStatusEx(_scmHandle, 0, SERVICE_WIN32, SERVICE_STATE_ALL,
            IntPtr.Zero, 0, out bytesNeeded, out servicesReturned, ref resumeHandle, null);
        int lastError = Marshal.GetLastPInvokeError();
        if (lastError != 0 && lastError != 122 && lastError != 234) {
            OpenServiceManager(true);
            EnumServicesStatusEx(_scmHandle, 0, SERVICE_WIN32, SERVICE_STATE_ALL,
                IntPtr.Zero, 0, out bytesNeeded, out servicesReturned, ref resumeHandle, null);
            lastError = Marshal.GetLastPInvokeError();
        }
        if (lastError != 122 && lastError != 234) {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
        IntPtr buffer = Marshal.AllocHGlobal(bytesNeeded);
        try {
            if (!EnumServicesStatusEx(_scmHandle, 0, SERVICE_WIN32, SERVICE_STATE_ALL, buffer,
                    bytesNeeded, out bytesNeeded, out servicesReturned, ref resumeHandle, null)) {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }
            IntPtr current = buffer;
            int structSize = Marshal.SizeOf<ENUM_SERVICE_STATUS_PROCESS>();
            for (int i = 0; i < servicesReturned; i++, current += structSize) {
                ENUM_SERVICE_STATUS_PROCESS ssp = Marshal.PtrToStructure<ENUM_SERVICE_STATUS_PROCESS>(current);
                if (serviceName is null || serviceName.Equals(ssp.lpServiceName, StringComparison.OrdinalIgnoreCase)) {
                    Service service = new(this, ssp.lpServiceName);
                    service.SetState((ServiceState)ssp.ServiceStatusProcess.dwCurrentState);
                    Services.Add(service.Name, service);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"ServiceManager.FetchServiceData exception :: {ex.Message}");
        } finally {
            Marshal.FreeHGlobal(buffer);
        }
    }
    public IEnumerable<ServiceValue> GetServiceValues() {
        List<ServiceValue> values = [];
        foreach (Service service in Services.Values) {
            values.Add(service.ToServiceValue());
        }
        return values;
    }
    public Dictionary<string, T> ServiceDispatch<T>(Func<Service, T> dispatcher) {
        Dictionary<string, T> ret = [];
        foreach (Service service in Services.Values) {
            ret.Add(service.Name, dispatcher(service));
        }
        return ret;
    }
    public T? ServiceDispatch<T>(Func<Service, T> dispatcher, string serviceName) {
        foreach (Service service in Services.Values) {
            if (service.Name != serviceName) {
                continue;
            }
            return dispatcher(service);
        }
        return default;
    }
    private void CloseServiceManager(bool dispose) {
        if (dispose) {
            Dispose();
            return;
        }
        _scmHandle?.Dispose();
        _scmHandle = null;
    }
    [MemberNotNull(nameof(_scmHandle))]
    private void OpenServiceManager(bool force = false) {
        if (!force) {
            if (_scmHandle is not null) {
                if (!_scmHandle.IsClosed && !_scmHandle.IsInvalid && _scmHandle.DangerousGetHandle() != IntPtr.Zero) {
                    return;
                }
                CloseServiceManager(false);
            }
        } else {
            CloseServiceManager(false);
        }
        IntPtr? localHandle = OpenSCManager(_computerName, null, SCM_GENERIC_EXECUTE);
        if (localHandle is null || localHandle == IntPtr.Zero) {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
        _scmHandle = new(localHandle.Value, true);
    }
    internal void GetServiceHandle(Service service) {
        OpenServiceManager(false);
        service.CloseService(false);
        IntPtr? localHandle = OpenService(_scmHandle, service.Name, SERVICE_REMEDIATE_CONFIG);
        if (localHandle is null || localHandle == IntPtr.Zero) {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
        SafeScmHandle handle = new(localHandle.Value, true);
        service.SetHandle(handle);
    }
    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {

            }
            lock (_lock) {
                Services.Dispose();
                _scmHandle?.Dispose();
                _scmHandle = null;
                disposedValue = true;
            }
        }
    }
    ~ServiceManager() {
        Dispose(disposing: false);
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}