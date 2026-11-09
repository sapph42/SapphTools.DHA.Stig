using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Remediator.Classes.Remediators; 
internal partial class ServiceRemediator : IRemediator {
    private ServiceRemediator() { }
    public static IValue? GetActualState(IValue target, string computerName) {
        if (target is not ServiceValue val) {
            throw new ArgumentException($"Expected {nameof(target)} to be of type ServiceValue, was {target.GetType().Name}.");
        }
        ServiceSetValue state = new(val);
        try {
            using ServiceManager scm = new(computerName);
            scm.FetchServiceData(val.Name);
            foreach (ServiceValue sVal in scm.GetServiceValues()) {
                state.Values.Add(sVal.Clone());
            }
            return state;
        } catch {
            return state;
        }
    }
    public static RemediationActionResult Remediate(RemediatorDto dto, Logger? logger = null) {
        logger ??= new();
        IValue data = dto.Setting.Data.Clone();
        if (data is not ServiceValue val) {
            return new ArgumentException(
                $"Expected {nameof(dto)}.Setting.Data to be of type " +
                $"ServiceValue, was {data.GetType().Name}"
            );
        }
        Guid settingBatch = Guid.NewGuid();
        string targetHost = dto.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new(dto, settingBatch, targetHost);
        return val.ActionSubtype switch {
            ServiceType.NoAction => RemediateNoAction(pre, logger),
            ServiceType.UnquotedPath => RemediateUnquotedPath(pre, val, logger, dto.WhatIf),
        };
    }

    public static RemediationActionResult Rollback(Guid remBatch, Guid ruleBatch, RemediationAction logEntry, Logger? logger) {
        logger ??= new();
        if (logEntry.After is null) {
            return new ArgumentException("Cannot rollback a log entry with a null After property.");
        }
        if (logEntry.Before is not ServiceValue oldVal) {
            return new ArgumentException(
                $"Expected {nameof(logEntry)}.Before to be of type RegistryValuePatternValue, was {logEntry.Before?.GetType().Name}"
            );
        }
        if (logEntry.After is not ServiceValue newVal) {
            return new ArgumentException(
                $"Expected {nameof(logEntry)}.After to be of type ServiceValue, was {logEntry.After?.GetType().Name}"
            );
        }
        ArgumentNullException.ThrowIfNull(newVal.Name, nameof(newVal.Name));
        ArgumentNullException.ThrowIfNull(newVal.BinPath, nameof(newVal.BinPath));
        Guid settingBatch = Guid.NewGuid();
        string targetHost = logEntry.ComputerName ?? Environment.MachineName;
        RemediationPreAction pre = new() {
            RemediationBatch = remBatch,
            RuleBatch = ruleBatch,
            SettingBatch = settingBatch,
            RuleId = logEntry.RuleId,
            Description = logEntry.Description,
            ComputerName = targetHost,
            SettingIndex = logEntry.SettingIndex,
            Source = ActionSource.Rollback
        };
        using ServiceManager scm = new(targetHost);
        scm.FetchServiceData(newVal.Name);
        string? currentBinPath = scm.ServiceDispatch((service) => {
            return service.BinPath;
        }, newVal.Name);
        if (!newVal.BinPath.Equals(currentBinPath)) {
            return logger.LogError(pre, TargetType.Service, newVal.Name, "Current binPath does not match remediated value", false);
        }
        return SetBinPath(pre, newVal, oldVal, scm, logger);
    }
    private static RemediationActionResult SetBinPath(RemediationPreAction preAction, ServiceValue before, ServiceValue target, ServiceManager scm, Logger logger) {
        ArgumentNullException.ThrowIfNull(target.Name, nameof(target.Name));
        ArgumentNullException.ThrowIfNull(target.BinPath, nameof(target.BinPath));
        try {
            bool changeRes = scm.ServiceDispatch((service) => {
                return service.SetBinPath(target.BinPath);
            }, target.Name);
            if (changeRes) {
                return logger.LogSuccess(preAction, TargetType.Service, target.BinPath, RollbackCapability.Automatic, before, target);
            } else {
                return logger.LogError(preAction, TargetType.Service, target.BinPath, "Unknown failure", false);
            }
        } catch (Exception ex) {
            return logger.LogError(preAction, TargetType.Service, target.BinPath, ex.Message, false);
        }
    }
    private static RemediationActionResult RemediateNoAction(RemediationPreAction preAction, Logger logger) {
        return logger.LogNoAction(preAction, TargetType.Service, preAction.ComputerName, null);
    }
    private static RemediationActionResult RemediateUnquotedPath(RemediationPreAction preAction, ServiceValue data, Logger logger, bool whatIf = true) {
        using ServiceManager scm = new(preAction.ComputerName);
        scm.FetchServiceData(data.Name);
        Dictionary<string, string> serviceBinPaths = scm.ServiceDispatch((service) => {
            return service.BinPath;
        });
        Dictionary<string, string> flaggedServices  = [];
        foreach (string serviceName in serviceBinPaths.Keys) {
            string path = ExtractPath(serviceBinPaths[serviceName]);
            if (FlagPath(path)) {
                int startIndex = serviceBinPaths[serviceName].IndexOf(path);
                string arguments = serviceBinPaths[serviceName][(startIndex + path.Length)..];
                string newPath = $"\"{path}\"{arguments}";
                flaggedServices.Add(serviceName, newPath);
            }
        }
        if (flaggedServices.Count == 0) {
            return logger.LogNoAction(preAction, TargetType.Service, string.Empty, null);
        }
        RemediationActionResult? res = null;
        foreach (string serviceName in flaggedServices.Keys) {
            RemediationActionResult thisRes;
            ServiceValue before = new() {
                ActionSubtype = ServiceType.UnquotedPath,
                Name = serviceName,
                BinPath = serviceBinPaths[serviceName]
            };
            ServiceValue after = new() {
                ActionSubtype = ServiceType.UnquotedPath,
                Name = serviceName,
                BinPath = flaggedServices[serviceName]
            };
            if (whatIf) {
                thisRes = logger.LogWhatIf(preAction, TargetType.Service, serviceName, RollbackCapability.NotApplicable, before, null);
                res ??= thisRes;
                continue;
            }
            if (IsAmbiguous(serviceBinPaths[serviceName])) {
                thisRes = logger.LogError(preAction, TargetType.Service, serviceName, "binPath was sufficiently ambiguous that it could not be automatically resolved.", false);
            } else {
                thisRes = SetBinPath(preAction, before, after, scm, logger);
            }
            if (!thisRes.IsSuccess) {
                res = thisRes;
            }
            res ??= thisRes;
        }
        return res!;
    }
    private static string ExtractPath(string binPath) {
        string path = new(binPath);
        int exeIdx;
        if (path.StartsWith('"')) {
            int secondQuote = path.IndexOf('"', 1);
            path = path[0..(secondQuote + 1)];
            return path;
        } else if (ExtractorStep1().IsMatch(path)) {
            Match match = ExtractorStep1().Match(path);
            path = match.Groups[2].Value;
            if (path.IndexOf('/') > 0) {
                path = path[0..(path.IndexOf('/'))];
            }
            if (path.IndexOf(" -") > 0) {
                path = path[0..(path.IndexOf(" -"))];
            }
            if (ExtractorStep2().IsMatch(path)) {
                match = ExtractorStep2().Match(path);
                path = match.Groups[1].Value;
            }
        }
        
        exeIdx = path.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        return path[..(exeIdx + 4)];
    }
    private static bool IsAmbiguous(string? binPath) {
        if (string.IsNullOrWhiteSpace(binPath) ||
                binPath.IndexOfAny(['\0', '\r', '\n']) >= 0) {
            return true;
        }
        string command = binPath.TrimStart(' ', '\t');
        bool quoted = command[0] == '"';
        string expectedPath;

        if (quoted) {
            int closingQuote = command.IndexOf('"', 1);
            if (closingQuote <= 1) {
                return true;
            }
            int pathEnd = closingQuote + 1;
            if (pathEnd < command.Length && command[pathEnd] is not (' ' or '\t')) {
                return true;
            }
            expectedPath = command[..pathEnd];
        } else {
            MatchCollection matches = AmbiCheck1().Matches(command);
            if (matches.Count != 1) {
                return true;
            }
            Match candidate = matches[0];
            expectedPath = command[..(candidate.Index + candidate.Length)];
            if (!AmbiCheck2().IsMatch(expectedPath)) {
                return true;
            }
        }
        string extracted = ExtractPath(binPath);
        bool expectedFlag = !quoted && expectedPath.Contains(' ');
        return !string.Equals(extracted, expectedPath, StringComparison.Ordinal) ||
            FlagPath(extracted) != expectedFlag;
    }
    private static bool FlagPath(string binPath) {
        return binPath.IndexOf(' ') > 0 &&
            !FlagCheck1().IsMatch(binPath) &&
            !FlagCheck2().IsMatch(binPath);
    }

    [GeneratedRegex("""^(\s+)?("?([A-Za-z]:|\\\\)\\[^:]+).*""")]
    private static partial Regex ExtractorStep1();

    [GeneratedRegex(@"^(([A-Za-z]:|\\\\)\\.*\\[^\.]+\.[^\s]+).*")]
    private static partial Regex ExtractorStep2();

    [GeneratedRegex("""^".*"$""")]
    private static partial Regex FlagCheck1();

    [GeneratedRegex(@"^[^\s]+\s+$")]
    private static partial Regex FlagCheck2();

    [GeneratedRegex(@"\.exe(?=[ \t]|\z)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AmbiCheck1();

    [GeneratedRegex(@"\A[A-Za-z]:\\[^:""<>|?*/\x00-\x1F]+\z")]
    private static partial Regex AmbiCheck2();
}
