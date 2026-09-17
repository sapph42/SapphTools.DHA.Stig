using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class RegistryValuePatternSettings : ValueSettings, IValueSettings<RegistryValuePatternValue> {
    private object? _data;
    private Regex? _targetPattern;
    private Regex? _pathPattern;
    private Regex? TargetRegex {
        get => _targetPattern;
        set {
            _targetPattern = value;
            CheckTestPath();
        }
    }
    private Regex? PathRegex {
        get => _pathPattern;
        set {
            _pathPattern = value;
            CheckTestPath();
        }
    }
    public override bool IsValid =>
        !string.IsNullOrWhiteSpace(Target.Text) &&
        IsValidPath() &&
        IsValidData();
    public override RegistryValuePatternValue? Value {
        get {
            if (!IsValid) {
                return null;
            }
            return new() {
                Target = Target.Text,
                TargetPattern = TargetRegex,
                SubPath = Subpath.Text,
                PathPattern = PathRegex,
                Name = ValueName.Text,
                Kind = (RegistryValueKind)DataType.SelectedItem!,
                Data = _data,
                Overwrite = Overwrite.Checked,
            };
        }
    }
    public RegistryValuePatternSettings(RegistryValuePatternValue? value) : this() {
        if (value is not null) {
            Target.Text = value.Target;
            TargetRegex = value.TargetPattern;
            TargetPattern.Text = TargetRegex?.ToString();
            Subpath.Text = value.SubPath;
            PathRegex = value.PathPattern;
            PathPattern.Text = PathRegex?.ToString();
            ValueName.Text = value.Name;
            DataType.SelectedItem = value.Kind;
            Data.Text = value.Data?.ToString();
            Overwrite.Checked = value.Overwrite;
        }
    }
    public RegistryValuePatternSettings(Setting? setting) : this() {
        if (setting is not null && setting.Data is RegistryValuePatternValue value) {
            Target.Text = value.Target;
            TargetRegex = value.TargetPattern;
            TargetPattern.Text = TargetRegex?.ToString();
            Subpath.Text = value.SubPath;
            PathRegex = value.PathPattern;
            PathPattern.Text = PathRegex?.ToString();
            ValueName.Text = value.Name;
            DataType.SelectedItem = value.Kind;
            Data.Text = value.Data?.ToString();
            Overwrite.Checked = value.Overwrite;
        }
    }
    public RegistryValuePatternSettings() {
        InitializeComponent();
        DataType.Items.Add(RegistryValueKind.Binary);
        DataType.Items.Add(RegistryValueKind.DWord);
        DataType.Items.Add(RegistryValueKind.ExpandString);
        DataType.Items.Add(RegistryValueKind.MultiString);
        DataType.Items.Add(RegistryValueKind.QWord);
        DataType.Items.Add(RegistryValueKind.String);
        DataType.Format += (_, e) => {
            if (e.ListItem is RegistryValueKind kind) {
                e.Value = kind.ToString();
            }
        };
    }
    private void Browse_Click(object sender, EventArgs e) {
        RegistryBrowserDialog browser = new(Target.Text);
        if (browser.ShowDialog() == DialogResult.OK) {
            Target.Text = browser.SelectedKey;
        }
    }
    private void DataType_SelectedIndexChanged(object sender, EventArgs e) {
        if (DataType.SelectedItem is RegistryValueKind kind && kind == RegistryValueKind.MultiString) {
            Data.Size = new Size(367, 96);
            Data.Multiline = true;
            Data.ScrollBars = ScrollBars.Vertical;
            Data.AcceptsReturn = true;
        } else {
            Data.Size = new Size(367, 23);
            Data.Multiline = false;
            Data.ScrollBars = ScrollBars.None;
            Data.AcceptsReturn = false;
        }
    }
    private void RunPatternTester(object sender, EventArgs e) {
        CheckTestPath();
    }
    private void Target_Validating(object sender, CancelEventArgs e) {
        e.Cancel = !string.IsNullOrEmpty(Target.Text) && !IsValidPath();
        Target.Text = Target.Text.Replace("\\\\", "\\");
    }
    private void Pattern_TextChanged(object sender, EventArgs e) {
        if (sender is TextBox patternBox) {
            try {
                if (TryGetRegex(patternBox.Text, out Regex? regex)) {
                    patternBox.ForeColor = SystemColors.WindowText;
                    switch (patternBox.Name) {
                        case "TargetPattern":
                            TargetRegex = regex;
                            break;
                        case "PathPattern":
                            PathRegex = regex;
                            break;
                        default:
                            return;
                    }
                } else {
                    patternBox.ForeColor = Color.Red;
                    switch (patternBox.Name) {
                        case "TargetPattern":
                            TargetRegex = null;
                            break;
                        case "PathPattern":
                            PathRegex = null;
                            break;
                        default:
                            return;
                    }
                }
            } finally {
                CheckTestPath();
            }
        }
    }
    private void PathPattern_Validating(object sender, CancelEventArgs e) {
        e.Cancel = !string.IsNullOrEmpty(Subpath.Text) && !IsValidSubpath();
        Subpath.Text = Subpath.Text.Replace("\\\\", "\\");
    }
    private void ValueName_Validating(object sender, CancelEventArgs e) {
        if (ValueName.Text.Length > 16383) {
            e.Cancel = true;
            ErrorProvider.SetError(ValueName, $"Value Name exceeds 16383 character limit ({ValueName.Text.Length}). Seriously?");
        } else if (ValueName.Text.Any(char.IsControl)) {
            e.Cancel = true;
            int badIndex = ValueName.Text.IndexOf(ValueName.Text.First(char.IsControl));
            ErrorProvider.SetError(ValueName, $"Invalid Value Name Character at character {badIndex}");
        } else {
            ErrorProvider.SetError(ValueName, "");
        }
    }
    private void DataType_Validating(object sender, CancelEventArgs e) {
        if (DataType.SelectedItem is RegistryValueKind) {
            ErrorProvider.SetError(DataType, "");
        }
        e.Cancel = !string.IsNullOrEmpty(Data.Text) && !IsValidData();
    }
    private void Data_Validating(object sender, CancelEventArgs e) {
        e.Cancel = !string.IsNullOrEmpty(Data.Text) && !IsValidData();
    }
    private void CheckTestPath() {
        TestResult.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(Target.Text)) {
            return;
        }
        string testPath = new(TestPath.Text);
        if (!testPath.StartsWith(Target.Text, StringComparison.OrdinalIgnoreCase)) {
            TestResult.Text = "Test path does not start with Target.";
            return;
        }
        testPath = testPath[Target.Text.Length..].TrimStart('\\');
        string targetPatternBranch = testPath.Split('\\')[0].Trim('\\');
        Match match;
        if (TargetRegex is not null) {
            match = TargetRegex.Match(targetPatternBranch);
            if (!match.Success || match.Index != 0) {
                TestResult.Text = "Test path after Target does not match Target Pattern.";
                return;
            }
            testPath = testPath[match.Value.Length..].TrimStart('\\');
        }
        if (string.IsNullOrWhiteSpace(Subpath.Text) && PathRegex is not null) {
            TestResult.Text = "Path Pattern should not be used with an empty Subpath. Use a single Target Pattern instead.";
            return;
        }
        if (!string.IsNullOrWhiteSpace(Subpath.Text) && !testPath.StartsWith(Subpath.Text, StringComparison.OrdinalIgnoreCase)) {
            TestResult.Text = "Test path after Target Pattern does not match Subpath";
            return;
        }
        testPath = testPath[Subpath.Text.Length..].TrimStart('\\');
        string subpathPatternBranch = testPath.Split('\\')[0].Trim('\\');
        if (PathRegex is not null) {
            match = PathRegex.Match(subpathPatternBranch);
            if (!match.Success || match.Index != 0) {
                TestResult.Text = "Test path after Subpath does not match Path Pattern.";
                return;
            }
            testPath = testPath[match.Value.Length..].TrimStart('\\');
        }
        if (testPath.Length > 0) {
            TestResult.Text = "Test path is descendent of key that matches all properties and patterns.";
        } else {
            TestResult.Text = "Test path matched all properties and patterns.";
        }
    }
    private bool IsValidData() {
        if (DataType.SelectedItem is RegistryValueKind kind) {
            switch (kind) {
                case RegistryValueKind.Binary:
                    return ParseAsBinary();
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    _data = Data.Text;
                    return true;
                case RegistryValueKind.MultiString:
                    _data = Data.Lines;
                    return true;
                case RegistryValueKind.DWord:
                    if (ParseNumber(Data.Text.Trim(), true, out int val)) {
                        _data = val;
                        return true;
                    }
                    return false;
                case RegistryValueKind.QWord:
                    if (ParseNumber(Data.Text.Trim(), true, out long val2)) {
                        _data = val2;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        } else {
            ErrorProvider.SetError(DataType, $"Data type must be selected!");
            return false;
        }
    }
    private bool IsValidPath() {
        ErrorProvider.SetError(Target, "");
        if (!HivePattern().IsMatch(Target.Text)) {
            ErrorProvider.SetError(Target, "Target does not appear to start with a valid hive.");
            return false;
        }
        string hive = HivePattern().Replace(Target.Text, "$1$4$7");
        string path = HivePattern().Replace(Target.Text, "$11");
        path = path.TrimEnd('\\');
        if (path.Any(char.IsControl)) {
            int badIndex = path.IndexOf(path.First(char.IsControl));
            ErrorProvider.SetError(Target, $"Invalid Target Character at character {badIndex}");
            return false;
        }
        RegistryHive? regHive = hive switch {
            "HKCR" => RegistryHive.ClassesRoot,
            "HKLM" => RegistryHive.LocalMachine,
            "HKU" => RegistryHive.Users,
            _ => null
        };
        if (regHive is null) {
            ErrorProvider.SetError(Target, "Target does not appear to start with a valid hive.");
            return false;
        }
        hive = hive switch {
            "HKCR" => "HKEY_CLASSES_ROOT",
            "HKLM" => "HKEY_LOCAL_MACHINE",
            "HKU" => "HKEY_USERS",
            _ => throw new UnreachableException()
        };
        RegistryKey? key = RegistryKey.OpenBaseKey(regHive.Value, RegistryView.Default);
        try {
            key = key.OpenSubKey(path);
            if (key is not null) {
                Target.Text = key.Name.TrimEnd('\\');
            } else {
                Target.Text = hive + '\\' + path;
            }
        } catch { } finally {
            key?.Dispose();
        }
        if (Target.Text.Length > 255) {
            ErrorProvider.SetError(Target, $"Target exceeds 255 character limit ({Target.Text.Length})");
            return false;
        }
        return true;
    }
    private bool IsValidSubpath() {
        ErrorProvider.SetError(Subpath, "");
        if (HivePattern().IsMatch(Subpath.Text)) {
            ErrorProvider.SetError(Target, "Subpath appears to start with a hive.");
            return false;
        }
        Subpath.Text = Subpath.Text.Trim('\\');
        if (Subpath.Text.Any(char.IsControl)) {
            int badIndex = Subpath.Text.IndexOf(Subpath.Text.First(char.IsControl));
            ErrorProvider.SetError(Subpath, $"Invalid Target Character at character {badIndex}");
            return false;
        }
        return true;
    }
    private bool ParseAsBinary() {
        byte[]? data;
        Data.CausesValidation = false;
        ErrorProvider.SetError(Data, "");
        try {
            string[] dataString = Data.Text.Split(',');
            for (int i = 0; i < dataString.Length; i++) {
                dataString[i] = dataString[i].Trim();
            }
            if (dataString.All(d => IsFormattedHex<byte>(d, false, out _))) {
                if (TryParseFormattedHex(dataString, out data)) {
                    _data = data;
                    Data.Text = data.ToHexString();
                    return true;
                }
            } else if (dataString.All(d => IsUnformattedHex<byte>(d, false, out _))) {
                if (TryParseUnformattedHex(dataString, out data)) {
                    _data = data;
                    Data.Text = data.ToHexString();
                    return true;
                }
            } else if (dataString.All(d => IsDecimal<byte>(d, false, out _))) {
                if (TryParseDecimal(dataString, out data)) {
                    _data = data;
                    Data.Text = data.ToHexString();
                    return true;
                }
            }
        } finally {
            Data.CausesValidation = true;
        }
        return false;
    }
    private static bool ParseNumber<T>(string num, bool parse, out T? result) where T : IMinMaxValue<T>, IBinaryInteger<T> {
        return IsFormattedHex(num, parse, out result) ||
            IsUnformattedHex(num, parse, out result) ||
            IsDecimal(num, parse, out result);
    }
    private static bool IsFormattedHex<T>(string num, bool parse, out T? result) where T : IMinMaxValue<T>, IBinaryInteger<T> {
        int width = T.MaxValue.ToString("X", null).Length;
        result = default;
        return num.StartsWith("0x") &&
            num[2..].All(char.IsAsciiHexDigit) &&
            num.Length == width + 2 &&
            (!parse || T.TryParse(num[2..], System.Globalization.NumberStyles.HexNumber, null, out result));
    }
    private static bool IsUnformattedHex<T>(string num, bool parse, out T? result) where T : IMinMaxValue<T>, IBinaryInteger<T> {
        int width = T.MaxValue.ToString("X", null).Length;
        result = default;
        return !num.StartsWith("0x") &&
            num.All(char.IsAsciiHexDigit) &&
            num.Length == width &&
            (!parse || T.TryParse(num, System.Globalization.NumberStyles.HexNumber, null, out result));
    }
    private static bool IsDecimal<T>(string num, bool parse, out T? result) where T : IMinMaxValue<T>, IBinaryInteger<T> {
        ulong max = Convert.ToUInt64(T.MaxValue);
        result = default;
        return !num.StartsWith("0x") &&
            num.All(char.IsAsciiDigit) &&
            num.Length <= max.ToString().Length &&
            (!parse || T.TryParse(num, System.Globalization.NumberStyles.None, null, out result));
    }
    private static bool TryGetRegex(string pattern, out Regex? regex) {
        regex = null;
        if (string.IsNullOrWhiteSpace(pattern)) {
            return true;
        }
        try {
            regex = new($@"\A(?:{pattern})\z");
            return true;
        } catch {
            return false;
        }
    }
    private bool TryParseDecimal(string[] items, [NotNullWhen(true)] out byte[]? data) {
        List<byte> dataAcc = [];
        for (int i = 0; i < items.Length; i++) {
            if (IsDecimal(items[i], true, out byte hb)) {
                dataAcc.Add(hb);
            } else {
                ErrorProvider.SetError(Data, $"Data items appear to be decimal, but item at {i} was an invalid byte value");
                data = null;
                return false;
            }
        }
        data = [.. dataAcc];
        return true;
    }
    private bool TryParseFormattedHex(string[] items, [NotNullWhen(true)] out byte[]? data) {
        List<byte> dataAcc = [];
        for (int i = 0; i < items.Length; i++) {
            if (IsFormattedHex(items[i], true, out byte hb)) {
                dataAcc.Add(hb);
            } else {
                ErrorProvider.SetError(Data, $"Data items formatted as 0x00 hex, but item at {i} was an invalid byte value");
                data = null;
                return false;
            }
        }
        data = [.. dataAcc];
        return true;
    }
    private bool TryParseUnformattedHex(string[] items, [NotNullWhen(true)] out byte[]? data) {
        List<byte> dataAcc = [];
        for (int i = 0; i < items.Length; i++) {
            if (IsUnformattedHex(items[i], true, out byte hb)) {
                dataAcc.Add(hb);
            } else {
                ErrorProvider.SetError(Data, $"Data items appear to be 00 hex, but item at {i} was an invalid byte value");
                data = null;
                return false;
            }
        }
        data = [.. dataAcc];
        return true;
    }


    [GeneratedRegex(@"(HK)((([A-Z]{1,2})(?:[:\\]+))|((?:EY)((?:_)([A-Z])(?:[^_:\\]+))))(((?:_)([A-Z])(?:[^_:\\]+))?(?:\\)(\S*))?")]
    private partial Regex HivePattern();
}
