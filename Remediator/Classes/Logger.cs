using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace SapphTools.DHA.Stig.Remediator.Classes;
internal static class Logger {
    private const string logDir = @"Remediator Logs";
    private static readonly Dictionary<string, bool> canFileLog = [];
    private static readonly Dictionary<string, string> fileLogPaths = [];
    private static readonly Dictionary<string, bool> canEventLog = [];
    private static readonly JsonSerializerOptions fileOptions = new(GlobalConstants.JsonSerializerOptions) {
        IgnoreReadOnlyFields = false,
        IgnoreReadOnlyProperties = false,
        IncludeFields = true,
        MaxDepth = 10,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict,
        WriteIndented = false
    };
    private static readonly JsonSerializerOptions eventOptions = new(GlobalConstants.JsonSerializerOptions) {
        IgnoreReadOnlyFields = false,
        IgnoreReadOnlyProperties = false,
        IncludeFields = true,
        IndentCharacter = ' ',
        IndentSize = 3,
        MaxDepth = 10,
        NewLine = Environment.NewLine,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict,
        WriteIndented = true
    };
    private static readonly byte[] newLine = [
        (byte)'\r',
        (byte)'\n'
    ];
    public static void Log(RemediationAction action) {
        LogToEventLog(action);
        LogToFile(action);
    }
    public static void LogError(RemediationPreAction preAction, TargetType type, string target, string message, bool whatIf) {
        RemediationAction action = GenerateError(preAction, type, target, message);
        if (whatIf) {
            LogWhatIf(action);
        } else {
            Log(action);
        }
    }
    public static void LogNoAction(RemediationPreAction preAction, TargetType type, string target, IValue current) {
        RemediationAction action = GenerateNoAction(preAction, type, target, current);
        Log(action);
    }
    public static void LogSuccess(
            RemediationPreAction preAction,
            TargetType type,
            string target,
            RollbackCapability rollback,
            IValue? before,
            IValue after) {
        RemediationAction action = GenerateSuccess(preAction, type, target, rollback, before, after);
        Log(action);
    }
    public static void LogWhatIf(
            RemediationPreAction preAction,
            TargetType type,
            string target,
            RollbackCapability rollback,
            IValue? before,
            IValue after) {
        RemediationAction action = GenerateSimulation(preAction, type, target, rollback, before, after);
        LogWhatIf(action);
    }
    private static void LogWhatIf(RemediationAction action) {
        LogToFile(action, globalOnly: true);
    }
    private static RemediationAction GenerateError(RemediationPreAction preAction, TargetType type, string target, string message) {
        return new() {
            ComputerName = preAction.ComputerName,
            RemediationBatch = preAction.RemediationBatch,
            RuleId = preAction.RuleId,
            Description = preAction.Description,
            SettingIndex = preAction.SettingIndex,
            RollbackCapability = RollbackCapability.NotApplicable,
            After = null,
            Before = null,
            TargetType = type,
            Target = target,
            Result = ActionResult.ActionFailure,
            FailureMessage = message,
            RemediationTimestamp = DateTime.Now,
        };
    }
    private static RemediationAction GenerateNoAction(RemediationPreAction preAction, TargetType type, string target, IValue current) {
        return new() {
            ComputerName = preAction.ComputerName,
            RemediationBatch = preAction.RemediationBatch,
            RuleId = preAction.RuleId,
            Description = preAction.Description,
            SettingIndex = preAction.SettingIndex,
            RollbackCapability = RollbackCapability.NotApplicable,
            After = current,
            Before = current,
            TargetType = type,
            Target = target,
            Result = ActionResult.NoActionTaken,
            FailureMessage = null,
            RemediationTimestamp = DateTime.Now,
        };
    }
    private static RemediationAction GenerateSuccess(
            RemediationPreAction preAction,
            TargetType type,
            string target,
            RollbackCapability rollback,
            IValue? before,
            IValue after) {
        return new() {
            ComputerName = preAction.ComputerName,
            RemediationBatch = preAction.RemediationBatch,
            RuleId = preAction.RuleId,
            Description = preAction.Description,
            SettingIndex = preAction.SettingIndex,
            RollbackCapability = rollback,
            After = after,
            Before = before,
            TargetType = type,
            Target = target,
            Result = ActionResult.ActionSuccess,
            FailureMessage = null,
            RemediationTimestamp = DateTime.Now,
        };
    }
    private static RemediationAction GenerateSimulation(
        RemediationPreAction preAction,
        TargetType type,
        string target,
        RollbackCapability rollback,
        IValue? before,
        IValue after) {
        return new() {
            ComputerName = preAction.ComputerName,
            RemediationBatch = preAction.RemediationBatch,
            RuleId = preAction.RuleId,
            Description = preAction.Description,
            SettingIndex = preAction.SettingIndex,
            RollbackCapability = rollback,
            After = after,
            Before = before,
            TargetType = type,
            Target = target,
            Result = ActionResult.WhatIf,
            FailureMessage = null,
            RemediationTimestamp = DateTime.Now,
        };
    }
    private static bool CanFileLog(RemediationAction action, bool temp = false) {
        if (canFileLog.TryGetValue(action.ComputerName, out bool value)) {
            return value;
        }
        string logTarget = GetFilePath(action, temp);
        if (!Directory.Exists(logTarget)) {
            try {
                Directory.CreateDirectory(logTarget);
            } catch {
                if (temp) {
                    canFileLog[action.ComputerName] = false;
                    _ = fileLogPaths.Remove(action.ComputerName);
                    return false;
                }
                return CanFileLog(action, true);
            }
        }
        try {
            File.Create(Path.Combine(logTarget, "test.txt"));
            File.Delete(Path.Combine(logTarget, "test.txt"));
            canFileLog[action.ComputerName] = true;
            fileLogPaths[action.ComputerName] = logTarget;
        } catch {
            if (temp) {
                canFileLog[action.ComputerName] = false;
                _ = fileLogPaths.Remove(action.ComputerName);
                return false;
            }
            return CanFileLog(action, true);
        }
        return canFileLog[action.ComputerName];
    }
    private static bool CanEventLog(string computerName) {
        if (canEventLog.TryGetValue(computerName, out bool value)) {
            return value;
        }
        string target = computerName switch {
            string host when host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) => ".",
            _ => computerName
        };
        if (EventLog.SourceExists("EAMC STIG Remediation", target)) {
            try {
                using EventLog appLog = new("Application", target) {
                    Source = "EAMC STIG Remediation"
                };
                appLog.WriteEntry("Remediator Init", EventLogEntryType.SuccessAudit, 1000);
                canEventLog[computerName] = true;
            } catch {
                canEventLog[computerName] = false;
            }
        } else {
            try {
                EventSourceCreationData source = new("EAMC STIG Remediation", "Application") {
                    MachineName = target
                };
                EventLog.CreateEventSource(source);
                using EventLog appLog = new("Application", target) {
                    Source = "EAMC STIG Remediation"
                };
                appLog.WriteEntry("Remediator Init", EventLogEntryType.SuccessAudit, 1000);
                canEventLog[computerName] = true;
            } catch {
                canEventLog[computerName] = false;
            }
        }
        return canEventLog[computerName];
    }
    private static EventLog GetEventLog(RemediationAction action) {
        string target = action.ComputerName switch {
            string host when host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) => ".",
            _ => action.ComputerName
        };
        return new("Application", target) {
            Source = "EAMC STIG Remediation"
        };
    }
    private static string GetFilePath(RemediationAction action, bool temp) {
        if (fileLogPaths.TryGetValue(action.ComputerName, out string? value)) {
            return value;
        }
        string targetPath = temp ? @"WINDOWS\SystemTemp" : logDir;
        string basePath = action.ComputerName switch {
            string host when host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) => $@"C:\{targetPath}",
            _ => $@"\\{action.ComputerName}\c$\{targetPath}"
        };
        string logFileName = $"{action.RemediationBatch:D}.log";
        return Path.Combine(basePath, logFileName);
    }
    private static void LogToEventLog(RemediationAction action) {
        if (!CanEventLog(action.ComputerName)) {
            return;
        }
        try {
            using EventLog log = GetEventLog(action);
            string json = JsonSerializer.Serialize(action, eventOptions);
            switch (action.Result) {
                case ActionResult.NoActionTaken:
                    log.WriteEntry($"Remediator Took No Action\r\n{json}", EventLogEntryType.Information, 2000);
                    break;
                case ActionResult.ActionSuccess:
                    log.WriteEntry($"Remediator Executed Requested Action\r\n{json}", EventLogEntryType.SuccessAudit, 2001);
                    break;
                case ActionResult.ActionFailure:
                    log.WriteEntry($"Remediator Failed To Execute Requested Action\r\n{json}", EventLogEntryType.Error, 2002);
                    break;
                default:
                    log.WriteEntry($"Unknown Remediation Result\r\n{json}", EventLogEntryType.Warning, 2003);
                    break;
            }
            return;
        } catch {
            canEventLog[action.ComputerName] = false;
            return;
        }
    }
    private static void LogToFile(RemediationAction action, bool globalOnly = false) {
        try {
            string globalPath = Path.GetDirectoryName(Settings.Default.CatalogPath)!;
            string byMachineLog = Path.Combine(
                globalPath, 
                "RemediationLog", 
                "ByMachine",
                action.ComputerName, 
                DateTime.Today.ToString("yyyyMMdd")
            );
            string byBatchLog = Path.Combine(
                globalPath,
                "RemediationLog",
                "ByBatch",
                DateTime.Today.ToString("yyyyMMdd")
            );
            _ = Directory.CreateDirectory(byMachineLog);
            _ = Directory.CreateDirectory(byBatchLog);
            string byMachineLogPath = Path.Combine(byMachineLog, $"{action.RemediationBatch:D}.log");
            string byBatchLogPath = Path.Combine(byBatchLog, $"{action.RemediationBatch:D}.log");
            Debug.WriteLine($"  Log Entry to {byMachineLogPath} for rule {action.RuleId}");
            using FileStream stream = File.OpenWrite(byMachineLogPath);
            stream.Seek(0, SeekOrigin.End);
            JsonSerializer.Serialize(stream, action, fileOptions);
            stream.Write(newLine.AsSpan());
            Debug.WriteLine($"  Log Entry to {byBatchLog} for rule {action.RuleId}");
            using FileStream batchStream = File.OpenWrite(byBatchLogPath);
            batchStream.Seek(0, SeekOrigin.End);
            JsonSerializer.Serialize(batchStream, action, fileOptions);
            batchStream.Write(newLine.AsSpan());
        } catch { }
        if (!globalOnly) {
            if (!CanFileLog(action)) {
                return;
            }
            try {
                using FileStream stream = File.OpenWrite(fileLogPaths[action.ComputerName]);
                stream.Seek(0, SeekOrigin.End);
                JsonSerializer.Serialize(stream, action, fileOptions);
                stream.Write(newLine.AsSpan());
            } catch {
                canFileLog[action.ComputerName] = false;
                _ = fileLogPaths.Remove(action.ComputerName);
            }
        }
    }
}
