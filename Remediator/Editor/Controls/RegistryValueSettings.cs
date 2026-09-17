using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class RegistryValueSettings : ValueSettings, IValueSettings<RegistryValueValue> {
    private object? _data;
    public override bool IsValid =>
        !string.IsNullOrWhiteSpace(Target.Text) &&
        IsValidPath() &&
        IsValidData();
    public override RegistryValueValue? Value {
        get {
            if (!IsValid) {
                return null;
            }
            return new() {
                Target = Target.Text,
                Name = ValueName.Text,
                Kind = (RegistryValueKind)DataType.SelectedItem!,
                Data = _data,
                Overwrite = Overwrite.Checked,
            };
        }
    }
    public RegistryValueSettings(RegistryValueValue? value) : this() {
        if (value is not null) {
            Target.Text = value.Target;
            ValueName.Text = value.Name;
            DataType.SelectedItem = value.Kind;
            Data.Text = value.Data?.ToString();
            Overwrite.Checked = value.Overwrite;
        }
    }
    public RegistryValueSettings(Setting? setting) : this() {
        if (setting is not null && setting.Data is RegistryValueValue value) {
            Target.Text = value.Target;
            ValueName.Text = value.Name;
            DataType.SelectedItem = value.Kind;
            Data.Text = value.Data?.ToString();
            Overwrite.Checked = value.Overwrite;
        }
    }
    public RegistryValueSettings() {
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
    private void Target_Validating(object sender, CancelEventArgs e) {
        Target.Text = Target.Text.Replace("\\\\", "\\");
        e.Cancel = !string.IsNullOrEmpty(Target.Text) && !IsValidPath();
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
        string hive = HivePattern().Replace(Target.Text, "$1$4$7$9");
        string path = HivePattern().Replace(Target.Text, "$10");
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
                Target.Text = key.Name;
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


    [GeneratedRegex(@"(HK)((([A-Z]{1,2})(?:[:\\]+))|((?:EY)((?:_)([A-Z])(?:[^_:\\]+))((?:_)([A-Z])(?:[^_:\\]+))?(?:\\)))(\S*)")]
    private partial Regex HivePattern();
}
