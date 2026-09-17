using SapphTools.DHA.Stig.Common.Classes;
using SapphTools.DHA.Stig.Common.Interfaces;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Remediator.Classes.Rollback; 
internal class LogBatch {
}
internal class LogBatchHost {

}
internal class LogRule : IEnumerable {
    internal readonly RemediationAction Source;
    private readonly List<LogSettingCollection> _settingCols = [];
    public string RuleId => Source.RuleId;
    public LogSettingCollection this[int index] {
        get => _settingCols[index];
    }
    public int Count => _settingCols.Count;
    public bool IsReadOnly => false;
    public LogRule(RemediationAction action) {
        Source = action;
        Add(new LogAction(action));
    }
    public void Add(LogAction action) {
        if (action.ComputerName != Source.ComputerName ||
                action.RuleId != Source.RuleId ||
                action.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Index instance");
        }
        if (_settingCols.Where(a => a.ContainsKey(action.SettingIndex)).FirstOrDefault() is LogSettingCollection col) {
            col.Add(action);
        } else {
            _settingCols.Add(new(action));
        }
    }
    public void Add(LogSetting setting) {
        if (setting.Source.ComputerName != Source.ComputerName ||
                setting.Source.RuleId != Source.RuleId ||
                setting.Source.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Index instance");
        }
        if (_settingCols.Where(a => a.ContainsValue(setting)).FirstOrDefault() is LogSettingCollection col) {
            col.Add(setting);
        } else {
            _settingCols.Add(new(setting));
        }
    }
    public void Clear() {
        _settingCols.Clear();
    }
    public List<LogSettingCollection>.Enumerator GetEnumerator() => _settingCols.GetEnumerator();
    public int IndexOf(LogSettingCollection item) => _settingCols.IndexOf(item);
    public int IndexOf(LogSetting item) {
        if (_settingCols.Where(a => a.ContainsValue(item)).FirstOrDefault() is LogSettingCollection col) {
            return _settingCols.IndexOf(col);
        } else {
            return -1;
        }
    }
    public void Insert(int index, LogSetting item) {
        if (item.Source.ComputerName != Source.ComputerName ||
                item.Source.RuleId != Source.RuleId ||
                item.Source.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Index instance");
        }
        if (_settingCols.Where(a => a.ContainsKey(item.SettingIndex)).FirstOrDefault() is LogSettingCollection col &&
            ) {
            col.Add(item);
        } else {
            _settingCols.Add(new(item));
        }
        _settingCols.Insert(index, item);
    }
    public bool Remove(LogAction item) => _settingCols.Remove(item);
    public void RemoveAt(int index) => _settingCols.RemoveAt(index);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
internal class LogSettingCollection : IEnumerable {
    internal readonly RemediationAction Source;
    private Dictionary<int, LogSetting> _indicies = [];

    public IEqualityComparer<int> Comparer => _indicies.Comparer;
    public int Count => _indicies.Count;
    public bool IsReadOnly => false;
    public LogSetting this[int index] => _indicies[index];
    public Dictionary<int, LogSetting>.KeyCollection Keys => _indicies.Keys;
    public Dictionary<int, LogSetting>.ValueCollection Values => _indicies.Values;

    public LogSettingCollection(RemediationAction source) {
        Source = source;
        Add(new LogAction(source));
    }
    public LogSettingCollection(LogSetting setting) {
        Source = setting.Source;
        Add(setting);
    }
    public void Add(LogSetting item) {
        if (_indicies.TryGetValue(item.SettingIndex, out LogSetting? value)) {
            value.Add(item);
        } else {
            _indicies.Add(item.SettingIndex, item);
        }
    }
    public void Add(LogAction action) {
        if (_indicies.TryGetValue(action.SettingIndex, out LogSetting? value)) {
            value.Add(action);
        } else {
            _indicies.Add(action.SettingIndex, new(action));
        }
    }
    public void Clear() => _indicies.Clear();
    public bool ContainsKey(int settingIndex) => _indicies.ContainsKey(settingIndex);
    public bool ContainsValue(LogSetting index) => _indicies.ContainsValue(index);
    public int EnsureCapacity(int capacity) => _indicies.EnsureCapacity(capacity);
    public Dictionary<int, LogSetting>.Enumerator GetEnumerator() => _indicies.GetEnumerator();
    public bool Remove(int settingIndex) => _indicies.Remove(settingIndex);
    public bool Remove(LogSetting index) => _indicies.Remove(index.SettingIndex);
    public bool Remove(LogAction action) => _indicies[action.SettingIndex].Remove(action);
    public bool Remove(int settingIndex, out LogSetting? index) => _indicies.Remove(settingIndex, out index);
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
internal class LogSetting : IEnumerable {
    internal readonly RemediationAction Source;
    private readonly List<LogAction> _actions = [];
    public int SettingIndex => Source.SettingIndex;
    public LogAction this[int index] { 
        get => _actions[index];
        set {
            if (value.SettingIndex != Source.SettingIndex ||
                    value.ComputerName != Source.ComputerName ||
                    value.RuleId != Source.RuleId ||
                    value.RemediationBatch != Source.RemediationBatch) {
                throw new ArgumentException("Provided value is not a proper child of this Index instance");
            }
            _actions[index] = value;
        }
    }
    public int Count => _actions.Count;
    public bool IsReadOnly => false;
    public LogSetting(RemediationAction action) {
        Source = action;
        Add(action);
    }
    public void Add(LogAction action) {
        if (action.SettingIndex != Source.SettingIndex ||
                action.ComputerName != Source.ComputerName ||
                action.RuleId != Source.RuleId ||
                action.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Index instance");
        }
        _actions.Add(action);
    }
    public void Add(LogSetting action) {
        if (action.Source.SettingIndex != Source.SettingIndex ||
                action.Source.ComputerName != Source.ComputerName ||
                action.Source.RuleId != Source.RuleId ||
                action.Source.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Index instance");
        }
        _actions.Add(action.Source);
    }
    public void Clear() {
        _actions.Clear();
    }
    public bool Contains(LogAction item) => _actions.Contains(item);
    public void CopyTo(LogAction[] array, int arrayIndex) => _actions.CopyTo(array, arrayIndex);
    public IEnumerator<LogAction> GetEnumerator() => _actions.GetEnumerator();
    public int IndexOf(LogAction item) => _actions.IndexOf(item);
    public void Insert(int index, LogAction item) {
        if (item.SettingIndex != Source.SettingIndex ||
                item.ComputerName != Source.ComputerName ||
                item.RuleId != Source.RuleId ||
                item.RemediationBatch != Source.RemediationBatch) {
            throw new ArgumentException("Provided value is not a proper child of this Index instance");
        }
        _actions.Insert(index, item);
    }
    public bool Remove(LogAction item) => _actions.Remove(item);
    public void RemoveAt(int index) => _actions.RemoveAt(index);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
internal class LogAction {
    private readonly RemediationAction _source;
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

    [SetsRequiredMembers]
    public LogAction(RemediationAction action) {
        _source = action;
    }
    public static implicit operator LogAction(RemediationAction action) => new(action);
    public static implicit operator RemediationAction(LogAction action) => new() {
        RemediationBatch = action.RemediationBatch,
        RuleId = action.RuleId,
        Description = action.Description,
        ComputerName = action.ComputerName,
        SettingIndex = action.SettingIndex,
        TargetType = action.TargetType,
        Target = action.Target,
        RollbackCapability = action.RollbackCapability,
        Before = action.Before,
        After = action.After,
        Result = action.Result,
        FailureMessage = action.FailureMessage,
        RemediationTimestamp = action.RemediationTimestamp
    };
}