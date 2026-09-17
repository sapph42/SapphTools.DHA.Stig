namespace SapphTools.DHA.Stig.Common.WinApi; 
public class LsaPrivilege {
    public static readonly Dictionary<string, LsaPrivilege> ValidPrivs = [];
    
    public static readonly LsaPrivilege SE_ASSIGNPRIMARYTOKEN_NAME = new("SeAssignPrimaryTokenPrivilege");
    public static readonly LsaPrivilege SE_AUDIT_NAME = new("SeAuditPrivilege");
    public static readonly LsaPrivilege SE_BACKUP_NAME = new("SeBackupPrivilege");
    public static readonly LsaPrivilege SE_CHANGE_NOTIFY_NAME = new("SeChangeNotifyPrivilege");
    public static readonly LsaPrivilege SE_CREATE_GLOBAL_NAME = new("SeCreateGlobalPrivilege");
    public static readonly LsaPrivilege SE_CREATE_PAGEFILE_NAME = new("SeCreatePagefilePrivilege");
    public static readonly LsaPrivilege SE_CREATE_PERMANENT_NAME = new("SeCreatePermanentPrivilege");
    public static readonly LsaPrivilege SE_CREATE_SYMBOLIC_LINK_NAME = new("SeCreateSymbolicLinkPrivilege");
    public static readonly LsaPrivilege SE_CREATE_TOKEN_NAME = new("SeCreateTokenPrivilege");
    public static readonly LsaPrivilege SE_DEBUG_NAME = new("SeDebugPrivilege");
    public static readonly LsaPrivilege SE_DELEGATE_SESSION_USER_IMPERSONATE_NAME = new("SeDelegateSessionUserImpersonatePrivilege");
    public static readonly LsaPrivilege SE_ENABLE_DELEGATION_NAME = new("SeEnableDelegationPrivilege");
    public static readonly LsaPrivilege SE_IMPERSONATE_NAME = new("SeImpersonatePrivilege");
    public static readonly LsaPrivilege SE_INC_BASE_PRIORITY_NAME = new("SeIncreaseBasePriorityPrivilege");
    public static readonly LsaPrivilege SE_INCREASE_QUOTA_NAME = new("SeIncreaseQuotaPrivilege");
    public static readonly LsaPrivilege SE_INC_WORKING_SET_NAME = new("SeIncreaseWorkingSetPrivilege");
    public static readonly LsaPrivilege SE_LOAD_DRIVER_NAME = new("SeLoadDriverPrivilege");
    public static readonly LsaPrivilege SE_LOCK_MEMORY_NAME = new("SeLockMemoryPrivilege");
    public static readonly LsaPrivilege SE_MACHINE_ACCOUNT_NAME = new("SeMachineAccountPrivilege");
    public static readonly LsaPrivilege SE_MANAGE_VOLUME_NAME = new("SeManageVolumePrivilege");
    public static readonly LsaPrivilege SE_PROF_SINGLE_PROCESS_NAME = new("SeProfileSingleProcessPrivilege");
    public static readonly LsaPrivilege SE_RELABEL_NAME = new("SeRelabelPrivilege");
    public static readonly LsaPrivilege SE_REMOTE_SHUTDOWN_NAME = new("SeRemoteShutdownPrivilege");
    public static readonly LsaPrivilege SE_RESTORE_NAME = new("SeRestorePrivilege");
    public static readonly LsaPrivilege SE_SECURITY_NAME = new("SeSecurityPrivilege");
    public static readonly LsaPrivilege SE_SHUTDOWN_NAME = new("SeShutdownPrivilege");
    public static readonly LsaPrivilege SE_SYNC_AGENT_NAME = new("SeSyncAgentPrivilege");
    public static readonly LsaPrivilege SE_SYSTEM_ENVIRONMENT_NAME = new("SeSystemEnvironmentPrivilege");
    public static readonly LsaPrivilege SE_SYSTEM_PROFILE_NAME = new("SeSystemProfilePrivilege");
    public static readonly LsaPrivilege SE_SYSTEMTIME_NAME = new("SeSystemtimePrivilege");
    public static readonly LsaPrivilege SE_TAKE_OWNERSHIP_NAME = new("SeTakeOwnershipPrivilege");
    public static readonly LsaPrivilege SE_TCB_NAME = new("SeTcbPrivilege");
    public static readonly LsaPrivilege SE_TIME_ZONE_NAME = new("SeTimeZonePrivilege");
    public static readonly LsaPrivilege SE_TRUSTED_CREDMAN_ACCESS_NAME = new("SeTrustedCredManAccessPrivilege");
    public static readonly LsaPrivilege SE_UNDOCK_NAME = new("SeUndockPrivilege");
    public static readonly LsaPrivilege SE_UNSOLICITED_INPUT_NAME = new("SeUnsolicitedInputPrivilege");
    public static readonly LsaPrivilege SE_INTERACTIVE_LOGON = new("SeInteractiveLogonRight");
    public static readonly LsaPrivilege SE_REMOTE_INTERACTIVE_LOGON = new("SeRemoteInteractiveLogonRight");
    public static readonly LsaPrivilege SE_NETWORK_LOGON = new("SeNetworkLogonRight");
    public static readonly LsaPrivilege SE_BATCH_LOGON = new("SeBatchLogonRight");
    public static readonly LsaPrivilege SE_SERVICE_LOGON = new("SeServiceLogonRight");

    public static readonly LsaPrivilege SE_DENY_REMOTE_INTERACTIVE_LOGON = new("SeDenyRemoteInteractiveLogonRight");
    public static readonly LsaPrivilege SE_DENY_NETWORK_LOGON = new("SeDenyNetworkLogonRight");
    public static readonly LsaPrivilege SE_DENY_BATCH_LOGON = new("SeDenyBatchLogonRight");
    public static readonly LsaPrivilege SE_DENY_SERVICE_LOGON = new("SeDenyServiceLogonRight");
    public static readonly LsaPrivilege SE_DENY_INTERACTIVE_LOGON = new("SeDenyInteractiveLogonRight");

    public string Value { get; private set; }

    private LsaPrivilege() { 
        throw new NotImplementedException();
    }
    private LsaPrivilege(string value) {
        Value = value;
        ValidPrivs[value] = this;
    }
    public static implicit operator string(LsaPrivilege priv) => priv.Value;
}
