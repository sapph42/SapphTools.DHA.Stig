using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Remediator.Classes;
/// <summary>
/// Represents the result of an operation, indicating success or failure with an optional exception.
/// This is the non-generic version, typically used for operations that don't return a specific value.
/// </summary>
public sealed class Result : Result<Null> {
    public Result() : base(Null.Instance) { }
    public Result(Exception ex) : base(ex) { }

    /// <summary>
    /// Executes an action and converts either completion or a thrown exception
    /// into a Result.
    /// </summary>
    public static Result Try(Action action) {
        try {
            action();
            return new Result();
        } catch (Exception exception) {
            return new Result(exception);
        }
    }
}
public sealed class RemediationActionResult : Result<RemediationAction> {
    public override bool IsSuccess => _exception is null && 
        (
            _value is null ||
            _value.Result != ActionResult.ActionFailure
        );
    public string Message {
        get {
            if (IsSuccess) {
                return string.Empty;
            }
            if (HasException) {
                return Exception.Message;
            }
            return _value?.FailureMessage ?? "Unknown failure";
        }
    }
    private RemediationActionResult() : base() { }
    public RemediationActionResult(Exception ex) : base(ex) { }
    public RemediationActionResult(RemediationAction res) : base(res) { }

    public static RemediationActionResult GenerateWithoutLog(
            RemediationPreAction preAction,
            TargetType type,
            string target,
            RollbackCapability rollback,
            IValue? before,
            IValue? after,
            ActionResult result) {
        return new RemediationAction(preAction, target) {
            RollbackCapability = rollback,
            After = after,
            Before = before,
            TargetType = type,
            Result = result,
            FailureMessage = null,
            RemediationTimestamp = DateTime.Now,
        };
    }
    public static RemediationActionResult IntermediateSuccessFactory() => new();

    public static implicit operator RemediationActionResult(Exception ex) {
        return new RemediationActionResult(ex);
    }
    public static implicit operator RemediationActionResult(RemediationAction val) {
        return new RemediationActionResult(val);
    }
}
/// <summary>
/// Represents the result of an operation, indicating success or failure with an optional value or exception.
/// </summary>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
public class Result<T> : IDisposable {
    private bool disposedValue;
    protected T? _value;
    protected Exception? _exception;

    public T? Value {
        get { return _value; }
        set { _value = value; }
    }
    public Exception? Exception {
        get { return _exception; }
        set { _exception = value; }
    }

    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Exception))]
    public virtual bool IsSuccess => _exception is null;

    /// <summary>
    /// Gets whether the result contains a non-null payload.
    ///
    /// This is intentionally separate from IsSuccess because a successful
    /// result may legally contain Null or null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => _value is not null and not Null;

    /// <summary>
    /// Gets whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool HasException => _exception is not null;

    /// <summary>
    /// Creates a successful result. The value will be null unless T is Null.
    /// </summary>
    public Result() {
        if (typeof(T) == typeof(Null)) {
            _value = (T)(object)Null.Instance;
        } else {
            _value = default;
        }
    }

    /// <summary>
    /// Creates a successful result. The value may be null.
    /// </summary>
    public Result(T? value) {
        _value = value;
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public Result(Exception exception) {
        _exception = exception;
        _value = default; // Ensure value is default if there's an exception
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result<T> Success(T? value) {
        return new Result<T>(value);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result<T> Failure(Exception exception) {
        return new Result<T>(exception);
    }

    public static implicit operator Result<T>(Exception ex) {
        return new Result<T>(ex);
    }
    public static implicit operator Result<T>(T val) {
        return new Result<T>(val);
    }

    /// <summary>
    /// Executes a function and wraps its result or any exception in a Result<T> object.
    /// </summary>
    /// <param name="action">The function to execute.</param>
    /// <returns>A Result<T> containing the function's return value or an exception.</returns>
    public static Result<T> Try(Func<T> action) {
        try {
            return new Result<T>(action());
        } catch (Exception ex) {
            return new Result<T>(ex);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                if (_value is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
            _value = default;
            _exception = null;
            disposedValue = true;
        }
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
