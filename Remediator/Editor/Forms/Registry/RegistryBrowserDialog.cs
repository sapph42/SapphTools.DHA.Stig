using Microsoft.Win32;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Registry;
public partial class RegistryBrowserDialog : Form {
    private readonly string? _originalKey;
    private bool _closing = false;
    public string? SelectedKey { get; protected set; }
    public RegistryBrowserDialog() {
        InitializeComponent();
        TreeNode root = new("Computer");
        root.Nodes.Add(new RegistryBaseTreeNode(RegistryHive.ClassesRoot));
        root.Nodes.Add(new RegistryBaseTreeNode(RegistryHive.LocalMachine));
        root.Nodes.Add(new RegistryBaseTreeNode(RegistryHive.Users));
        root.Nodes.Add(new RegistryBaseTreeNode(RegistryHive.CurrentConfig));
        RegistryTree.Nodes.Add(root);
    }
    public RegistryBrowserDialog(string? keyPath) : this() {
        _originalKey = keyPath;
    }
    private void RegistryTree_AfterSelect(object sender, TreeViewEventArgs e) {
        if (_closing) {
            return;
        }
        if (RegistryTree.SelectedNode is RegistryTreeNode regNode) {
            Ok.Enabled = true;
            SelectedKey = regNode.Key?.Name;
        } else {
            Ok.Enabled = false;
        }
    }
    private void RegistryTree_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
        if (e.Action != TreeViewAction.Expand) {
            return;
        }
        if (e.Node is not RegistryTreeNode regNode) {
            return;
        }
        if (regNode.Nodes.Count == 1 && regNode.Nodes[0] is not RegistryTreeNode) {
            regNode.Nodes.Clear();
        }
        if (regNode.Nodes.Count > 0) {
            return;
        }
        foreach (string subkeyName in regNode.SubkeyNames) {
            regNode.Nodes.Add(new RegistryKeyTreeNode(subkeyName));
        }
    }
    private void RegistryBrowserDialog_Load(object sender, EventArgs e) {
        TreeNode root = RegistryTree.Nodes[0];
        root.Expand();
        if (!string.IsNullOrWhiteSpace(_originalKey)) {
            foreach (RegistryTreeNode node in root.Nodes.OfType<RegistryTreeNode>()) {
                ExpandToKey(node, _originalKey);
            }
        }
    }
    private void RegistryBrowserDialog_FormClosing(object sender, FormClosingEventArgs e) {
        _closing = true;
        TreeNode root = RegistryTree.Nodes[0];
        foreach (IRegistryTreeNode node in root.Nodes.OfType<IRegistryTreeNode>().ToArray()) {
            node.Dispose();
        }
    }
    private void Ok_Click(object sender, EventArgs e) {
        Close();
    }
    private void Cancel_Click(object sender, EventArgs e) {
        SelectedKey = null;
        Close();
    }
    private void ExpandToKey(RegistryTreeNode node, string path) {
        if (node.Key is null) {
            return;
        }
        if (node.Key.Name.Equals(path, StringComparison.OrdinalIgnoreCase)) {
            RegistryTree.SelectedNode = node;
            return;
        }
        if (node.Key.IsAncestorOf(path)) {
            node.Expand();
            foreach (RegistryTreeNode child in node.Nodes.OfType<RegistryTreeNode>()) {
                ExpandToKey(child, path);
            }
        }
    }
}