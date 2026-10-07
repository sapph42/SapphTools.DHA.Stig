using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Common.WinApi; 
public enum LsaExceptionReason {
    Unknown,
    InvalidArgument,
    SidBufferCreation,
    OpenPolicy,
    QueryRight,
    ObjectNameNotFound,
    AddAccountToRight,
    RemoveAccountFromRight
}
public class LsaException : Exception {
    private string? _paramName = null;
    private const string defaultMessage = "An invalid argument was specified.";
    protected string? _message = null;

    internal const int COR_E_ARGUMENT = unchecked((int)0x80070057);

    public LsaExceptionReason Reason;
    public override string Message {
        get {
            return ReasonText +
                (_message is not null ?
                    $"({Environment.NewLine}{_message})" :
                    string.Empty) +
                (_paramName is not null ?
                    $"({Environment.NewLine}Parameter name: {_paramName})" :
                    string.Empty);
        }
    }
    protected string? ReasonText => Reason switch {
        LsaExceptionReason.InvalidArgument => "An invalid argument was provided.",
        LsaExceptionReason.OpenPolicy => "An exception occured while attempting to open the LSA Policy object.",
        LsaExceptionReason.SidBufferCreation => "An exception occured while attempting to create unmanaged buffers for account SIDs",
        LsaExceptionReason.QueryRight => "An exception occured while attempting to query a right from LSA policy.",
        LsaExceptionReason.ObjectNameNotFound => "An object in the LSA policy database was not found.",
        LsaExceptionReason.AddAccountToRight => "An exception occured while attempting to add a SID to an LSA right.",
        LsaExceptionReason.RemoveAccountFromRight => "An exception occured while attempting to remove a SID from an LSA right.",
        _ => "An unknown exception in the LSA API stack occured.",
    };

    public LsaException(string message) : this(null, message, LsaExceptionReason.Unknown) { }
    public LsaException(string message, Exception ex) : this(null, message, LsaExceptionReason.Unknown, ex) { }
    public LsaException(LsaExceptionReason reason, Exception ex) : base(null, ex) {
        Reason = reason;
    }
    public LsaException(LsaExceptionReason reason) : base() {
        Reason = reason;
    }
    public LsaException(string? paramName, LsaExceptionReason reason) : base() {
        Reason = reason;
        _paramName = paramName;
    }
    public LsaException(string? paramName, LsaExceptionReason reason, Exception ex) : base(null, ex) {
        Reason = reason;
        _paramName = paramName;
    }
    public LsaException(string? paramName, string message) : this(message) { 
        _paramName = paramName; 
    }
    public LsaException(string? paramName, string message, Exception ex) : this(message, ex) {
        _paramName = paramName;
    }
    public LsaException(string? paramName, string message, LsaExceptionReason reason) : this(paramName, reason) {
        _message = message;
    }
    public LsaException(string? paramName, string message, LsaExceptionReason reason, Exception ex) : this(paramName, reason, ex) {
        _message = message;
    }
}
