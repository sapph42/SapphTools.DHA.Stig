using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;

namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public interface IRegistryTreeNode : IDisposable {
    public RegistryKey? Key { get; }
    public IEnumerable<string> SubkeyNames => Key?.GetSubKeyNames() ?? [];
}
public abstract class RegistryTreeNode : TreeNode, IRegistryTreeNode {
    protected int _disposing;
    protected bool disposedValue;
    protected Lazy<RegistryKey?> _key = new(default(RegistryKey));
    public RegistryKey? Key {
        get {
            try {
                return _key.Value;
            } catch {
                Dispose();
                return null;
            }
        }
    }
    public IEnumerable<string> SubkeyNames => Key?.GetSubKeyNames() ?? [];
    protected virtual void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposing, 1) != 0) {
            return;
        }
        if (!disposedValue) {
            if (disposing) {
                foreach (IRegistryTreeNode node in Nodes.OfType<IRegistryTreeNode>().ToArray()) {
                    node.Dispose();
                }
                if (_key.IsValueCreated) {
                    Key?.Dispose();
                }
                Parent.Nodes.Remove(this);
            }
            disposedValue = true;
        }
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
public class RegistryBaseTreeNode : RegistryTreeNode {
    private RegistryHive _hive;

    public required RegistryHive Hive {
        get => _hive;
        init {
            _hive = value;
            _key = new(() => { return RegistryKey.OpenBaseKey(_hive, RegistryView.Registry64); });
        }
    }
    public RegistryBaseTreeNode() : base() { }
    [SetsRequiredMembers]
    public RegistryBaseTreeNode(RegistryHive hive) : this() {
        Hive = hive;
        Text = hive.ToString();
        Nodes.Add(new TreeNode());
    }
}
public class RegistryKeyTreeNode : RegistryTreeNode {
    private string? _keyName;
    public required string KeyName {
        get => _keyName ?? string.Empty;
        init {
            _keyName = value;
            _key = new(() => {
                return (Parent as IRegistryTreeNode)?.Key?.OpenSubKey(_keyName);
            });
        }
    }
    public RegistryKeyTreeNode() : base() { }

    [SetsRequiredMembers]
    public RegistryKeyTreeNode(string keyName) {
        KeyName = keyName;
        Text = keyName;
        Nodes.Add(new TreeNode());
    }
}