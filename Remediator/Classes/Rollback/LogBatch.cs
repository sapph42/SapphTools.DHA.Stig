using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Remediator.Classes.Rollback; 
internal class LogBatch {
    internal readonly Guid Batch;
    private readonly LogMachineCollection _machines;
    public ReadOnlyDictionary <string, LogMachine> MachineLogs => _machines.MachineLogs;
    public LogBatch(RemediationAction action) {
        Batch = action.RemediationBatch;
        _machines = new(action);
    }
    public void Add(RemediationAction action) {
        if (action.RemediationBatch != Batch) {
            throw new ArgumentException("Provided value is not a proper child of this Batch instance");
        }
        _machines.Add(action);
    }
}
internal class LogMachineCollection : IEnumerable {
    internal readonly Guid Batch;
    private readonly Dictionary<string, LogMachine> _machines = new(StringComparer.OrdinalIgnoreCase);
    public ReadOnlyDictionary<string, LogMachine> MachineLogs => _machines.AsReadOnly();
    public LogMachine this[string computerName] => _machines[computerName];
    public int Count => _machines.Count;
    public LogMachineCollection(RemediationAction action) {
        Batch = action.RemediationBatch;
        Add(action);
    }
    public void Add(RemediationAction action) {
        if (action.RemediationBatch != Batch) {
            throw new ArgumentException("Provided value is not a proper child of this Batch instance");
        }
        if (_machines.TryGetValue(action.ComputerName, out LogMachine? value)) {
            value.Add(action);
        } else {
            _machines.Add(action.ComputerName, new(action));
        }
    }
    public bool ContainsKey(string computerName) => _machines.ContainsKey(computerName);
    public bool TryGetValue(string computerName, out LogMachine? machine) => _machines.TryGetValue(computerName, out machine);
    public Dictionary<string, LogMachine>.ValueCollection.Enumerator GetEnumerator() => _machines.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
internal class LogMachine(RemediationAction action) {
    internal readonly RemediationAction Source = action;
    private readonly LogRuleCollection _rules = new(action);
    public ReadOnlyDictionary<string, LogRule> RuleLogs => _rules.RuleCollection;
    public string ComputerName => Source.ComputerName;

    public void Add(RemediationAction action) {
        if (!StringComparer.OrdinalIgnoreCase.Equals(action.ComputerName, Source.ComputerName) ||
                action.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Machine instance");
        }
        _rules.Add(action);
    }
}
internal class LogRuleCollection : IEnumerable {
    internal readonly RemediationAction Source;
    private readonly Dictionary<string, LogRule> _rules = new(StringComparer.OrdinalIgnoreCase);
    public ReadOnlyDictionary<string, LogRule> RuleCollection => _rules.AsReadOnly();
    public LogRule this[string ruleId] => _rules[ruleId];
    public int Count => _rules.Count;
    public LogRuleCollection(RemediationAction action) {
        Source = action;
        Add(action);
    }
    public void Add(RemediationAction action) {
        if (!StringComparer.OrdinalIgnoreCase.Equals(action.ComputerName, Source.ComputerName) ||
                action.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Machine instance");
        }
        if (_rules.TryGetValue(action.RuleId, out LogRule? value)) {
            value.Add(action);
        } else {
            _rules.Add(action.RuleId, new(action));
        }
    }
    public bool ContainsKey(string ruleId) => _rules.ContainsKey(ruleId);
    public bool TryGetValue(string ruleId, out LogRule? rule) => _rules.TryGetValue(ruleId, out rule);
    public Dictionary<string, LogRule>.ValueCollection.Enumerator GetEnumerator() => _rules.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
internal class LogRule(RemediationAction action) {
    internal readonly RemediationAction Source = action;
    private readonly LogSettingCollection _settings = new(action);
    public ReadOnlyDictionary<int, LogSetting> SettingLogs => _settings.SettingCollection;
    public string RuleId => Source.RuleId;

    public void Add(RemediationAction action) {
        if (action.RuleId != Source.RuleId ||
                action.ComputerName != Source.ComputerName ||
                action.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Rule instance");
        }
        _settings.Add(action);
    }
}
internal class LogSettingCollection : IEnumerable {
    internal readonly RemediationAction Source;
    private readonly Dictionary<int, LogSetting> _settings = [];
    public ReadOnlyDictionary<int, LogSetting> SettingCollection => _settings.AsReadOnly();
    public LogSetting this[int index] => _settings[index];
    public int Count => _settings.Count;
    public LogSettingCollection(RemediationAction action) {
        Source = action;
        Add(action);
    }
    public void Add(RemediationAction action) {
        if (!StringComparer.OrdinalIgnoreCase.Equals(action.RuleId, Source.RuleId) ||
                !StringComparer.OrdinalIgnoreCase.Equals(action.ComputerName, Source.ComputerName) ||
                action.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Rule instance");
        }
        if (_settings.TryGetValue(action.SettingIndex, out LogSetting? value)) {
            value.Add(action);
        } else {
            _settings.Add(action.SettingIndex, new(action));
        }
    }
    public bool ContainsKey(int settingIndex) => _settings.ContainsKey(settingIndex);
    public bool ContainsValue(LogSetting index) => _settings.ContainsValue(index);
    public Dictionary<int, LogSetting>.ValueCollection.Enumerator GetEnumerator() => _settings.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
internal class LogSetting : IEnumerable {
    internal readonly RemediationAction Source;
    private readonly List<LogAction> _actions = [];
    public int SettingIndex => Source.SettingIndex;
    public LogAction this[int index] => _actions[index];
    public int Count => _actions.Count;
    public LogSetting(RemediationAction action) {
        Source = action;
        Add(action);
    }
    public void Add(RemediationAction action) {
        if (action.SettingIndex != Source.SettingIndex ||
                !StringComparer.OrdinalIgnoreCase.Equals(action.RuleId, Source.RuleId) ||
                !StringComparer.OrdinalIgnoreCase.Equals(action.ComputerName, Source.ComputerName) ||
                action.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Setting instance");
        }
        _actions.Add(action);
    }
    public bool Contains(LogAction item) => _actions.Contains(item);
    public IEnumerator<LogAction> GetEnumerator() => _actions.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
[method: SetsRequiredMembers]
internal class LogAction(RemediationAction action) {
    private readonly RemediationAction _source = action;
    internal Guid RemediationBatch => _source.RemediationBatch;
    internal string RuleId => _source.RuleId;
    internal string? Description => _source.Description;
    internal string ComputerName => _source.ComputerName;
    internal int SettingIndex => _source.SettingIndex;
    public TargetType TargetType => _source.TargetType;
    public string Target => _source.Target;
    public RollbackCapability RollbackCapability => _source.RollbackCapability;
    public IValue? Before => _source.Before;
    public IValue? After => _source.After;
    public ActionResult Result => _source.Result;
    public string? FailureMessage => _source.FailureMessage;
    public DateTimeOffset RemediationTimestamp => _source.RemediationTimestamp;

    public static implicit operator LogAction(RemediationAction action) => new(action);
}