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
        throw new NotImplementedException();
    }

    private static RemediationActionResult RemediateNoAction(RemediationPreAction preAction, Logger logger) {
        return logger.LogNoAction(preAction, TargetType.Service, preAction.ComputerName, null);
    }
    private static RemediationActionResult RemediateUnquotedPath(RemediationPreAction preAction, ServiceValue data, Logger logger, bool whatIf = true) {
        using ServiceManager scm = new(preAction.ComputerName);
        scm.FetchServiceData();
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
            try {
                bool changeRes = scm.ServiceDispatch((service) => {
                    return service.SetBinPath(flaggedServices[serviceName]);
                }, serviceName);
                if (changeRes) {
                    thisRes = logger.LogSuccess(preAction, TargetType.Service, serviceName, RollbackCapability.Automatic, before, after);
                } else {
                    thisRes = logger.LogError(preAction, TargetType.Service, serviceName, "Unknown failure", false);
                }
            } catch (Exception ex) {
                thisRes = logger.LogError(preAction, TargetType.Service, serviceName, ex.Message, false);
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
        if (path.StartsWith('"')) {
            int secondQuote = path.IndexOf('"', 1);
            path = path[0..(secondQuote + 1)];
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
        return path;
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
}
