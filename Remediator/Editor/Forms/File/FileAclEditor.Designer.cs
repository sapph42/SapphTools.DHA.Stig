using SapphTools.DHA.Stig.Remediator.Controls;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.File;
partial class FileAclEditor {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
        groupBox1 = new GroupBox();
        tableLayoutPanel1 = new TableLayoutPanel();
        SelectPrincipal = new LinkLabel();
        PrincipalName = new Label();
        label3 = new Label();
        tabControl1 = new TabControl();
        tabPage1 = new TabPage();
        checkBox1 = new CheckBox();
        Permissions = new DataGridView();
        PrincipalDgv = new DataGridViewPrincipalColumn();
        TypeDgv = new DataGridViewTextBoxColumn();
        AccessDgv = new DataGridViewTextBoxColumn();
        InheritedFromDgv = new DataGridViewTextBoxColumn();
        AppliesToDgv = new DataGridViewTextBoxColumn();
        EditPermissions = new Button();
        RemovePermissions = new Button();
        AddPermissions = new Button();
        label2 = new Label();
        label1 = new Label();
        tabPage2 = new TabPage();
        Ok = new Button();
        Cancel = new Button();
        Apply = new Button();
        groupBox1.SuspendLayout();
        tableLayoutPanel1.SuspendLayout();
        tabControl1.SuspendLayout();
        tabPage1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)Permissions).BeginInit();
        SuspendLayout();
        // 
        // groupBox1
        // 
        groupBox1.BackColor = SystemColors.ControlLightLight;
        groupBox1.Controls.Add(tableLayoutPanel1);
        groupBox1.Controls.Add(label3);
        groupBox1.Controls.Add(tabControl1);
        groupBox1.Location = new Point(12, 10);
        groupBox1.Margin = new Padding(2, 1, 2, 1);
        groupBox1.Name = "groupBox1";
        groupBox1.Padding = new Padding(2, 1, 2, 1);
        groupBox1.Size = new Size(744, 446);
        groupBox1.TabIndex = 0;
        groupBox1.TabStop = false;
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.AutoSize = true;
        tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayoutPanel1.ColumnCount = 2;
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Controls.Add(SelectPrincipal, 1, 0);
        tableLayoutPanel1.Controls.Add(PrincipalName, 0, 0);
        tableLayoutPanel1.Location = new Point(95, 27);
        tableLayoutPanel1.Margin = new Padding(2, 1, 2, 0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 1;
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Size = new Size(149, 30);
        tableLayoutPanel1.TabIndex = 7;
        // 
        // SelectPrincipal
        // 
        SelectPrincipal.LinkBehavior = LinkBehavior.NeverUnderline;
        SelectPrincipal.Location = new Point(6, 0);
        SelectPrincipal.Margin = new Padding(2, 0, 2, 0);
        SelectPrincipal.Name = "SelectPrincipal";
        SelectPrincipal.Padding = new Padding(4, 0, 0, 0);
        SelectPrincipal.Size = new Size(141, 30);
        SelectPrincipal.TabIndex = 7;
        SelectPrincipal.TabStop = true;
        SelectPrincipal.Text = "Select a principal";
        SelectPrincipal.LinkClicked += SelectPrincipal_LinkClicked;
        // 
        // PrincipalName
        // 
        PrincipalName.AutoSize = true;
        PrincipalName.Location = new Point(2, 0);
        PrincipalName.Margin = new Padding(2, 0, 2, 0);
        PrincipalName.Name = "PrincipalName";
        PrincipalName.Size = new Size(0, 15);
        PrincipalName.TabIndex = 5;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(9, 28);
        label3.Name = "label3";
        label3.Size = new Size(42, 15);
        label3.TabIndex = 1;
        label3.Text = "Owner";
        // 
        // tabControl1
        // 
        tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Controls.Add(tabPage2);
        tabControl1.Location = new Point(0, 54);
        tabControl1.Margin = new Padding(2, 1, 2, 1);
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new Size(744, 390);
        tabControl1.TabIndex = 0;
        // 
        // tabPage1
        // 
        tabPage1.BackColor = SystemColors.ControlLightLight;
        tabPage1.Controls.Add(checkBox1);
        tabPage1.Controls.Add(Permissions);
        tabPage1.Controls.Add(EditPermissions);
        tabPage1.Controls.Add(RemovePermissions);
        tabPage1.Controls.Add(AddPermissions);
        tabPage1.Controls.Add(label2);
        tabPage1.Controls.Add(label1);
        tabPage1.Location = new Point(4, 24);
        tabPage1.Margin = new Padding(2, 1, 2, 1);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new Padding(2, 1, 2, 1);
        tabPage1.Size = new Size(736, 362);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "  Permissions  ";
        // 
        // checkBox1
        // 
        checkBox1.AutoSize = true;
        checkBox1.BackColor = Color.Transparent;
        checkBox1.Location = new Point(7, 329);
        checkBox1.Margin = new Padding(2, 1, 2, 1);
        checkBox1.Name = "checkBox1";
        checkBox1.Size = new Size(517, 19);
        checkBox1.TabIndex = 2;
        checkBox1.Text = "Replace all child object permission entries with inheritable permission entries from this object";
        checkBox1.UseVisualStyleBackColor = false;
        // 
        // Permissions
        // 
        Permissions.AllowUserToAddRows = false;
        Permissions.AllowUserToDeleteRows = false;
        Permissions.AllowUserToResizeRows = false;
        Permissions.BackgroundColor = SystemColors.ControlLightLight;
        Permissions.CellBorderStyle = DataGridViewCellBorderStyle.None;
        Permissions.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        Permissions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        Permissions.Columns.AddRange(new DataGridViewColumn[] { PrincipalDgv, TypeDgv, AccessDgv, InheritedFromDgv, AppliesToDgv });
        Permissions.EditMode = DataGridViewEditMode.EditProgrammatically;
        Permissions.Location = new Point(6, 57);
        Permissions.Margin = new Padding(2, 1, 2, 1);
        Permissions.Name = "Permissions";
        Permissions.ReadOnly = true;
        Permissions.RowHeadersVisible = false;
        Permissions.RowHeadersWidth = 82;
        Permissions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        Permissions.Size = new Size(660, 207);
        Permissions.TabIndex = 6;
        Permissions.CellDoubleClick += Permissions_CellDoubleClick;
        // 
        // PrincipalDgv
        // 
        PrincipalDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        PrincipalDgv.HeaderText = "Principal";
        PrincipalDgv.MinimumWidth = 10;
        PrincipalDgv.Name = "PrincipalDgv";
        PrincipalDgv.ReadOnly = true;
        // 
        // TypeDgv
        // 
        TypeDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        TypeDgv.HeaderText = "Type";
        TypeDgv.MinimumWidth = 10;
        TypeDgv.Name = "TypeDgv";
        TypeDgv.ReadOnly = true;
        TypeDgv.SortMode = DataGridViewColumnSortMode.NotSortable;
        TypeDgv.Width = 38;
        // 
        // AccessDgv
        // 
        AccessDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        AccessDgv.HeaderText = "Access";
        AccessDgv.MinimumWidth = 10;
        AccessDgv.Name = "AccessDgv";
        AccessDgv.ReadOnly = true;
        AccessDgv.SortMode = DataGridViewColumnSortMode.NotSortable;
        AccessDgv.Width = 49;
        // 
        // InheritedFromDgv
        // 
        InheritedFromDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        InheritedFromDgv.HeaderText = "Inherited from";
        InheritedFromDgv.MinimumWidth = 10;
        InheritedFromDgv.Name = "InheritedFromDgv";
        InheritedFromDgv.ReadOnly = true;
        InheritedFromDgv.SortMode = DataGridViewColumnSortMode.NotSortable;
        InheritedFromDgv.Width = 89;
        // 
        // AppliesToDgv
        // 
        AppliesToDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        AppliesToDgv.HeaderText = "Applies to";
        AppliesToDgv.MinimumWidth = 10;
        AppliesToDgv.Name = "AppliesToDgv";
        AppliesToDgv.ReadOnly = true;
        AppliesToDgv.SortMode = DataGridViewColumnSortMode.NotSortable;
        AppliesToDgv.Width = 66;
        // 
        // EditPermissions
        // 
        EditPermissions.Location = new Point(192, 275);
        EditPermissions.Margin = new Padding(2, 1, 2, 1);
        EditPermissions.Name = "EditPermissions";
        EditPermissions.Size = new Size(84, 23);
        EditPermissions.TabIndex = 5;
        EditPermissions.Text = "Edit";
        EditPermissions.UseVisualStyleBackColor = true;
        EditPermissions.Click += EditPermissions_Click;
        // 
        // RemovePermissions
        // 
        RemovePermissions.Location = new Point(100, 275);
        RemovePermissions.Margin = new Padding(2, 1, 2, 1);
        RemovePermissions.Name = "RemovePermissions";
        RemovePermissions.Size = new Size(84, 23);
        RemovePermissions.TabIndex = 4;
        RemovePermissions.Text = "Remove";
        RemovePermissions.UseVisualStyleBackColor = true;
        RemovePermissions.Click += RemovePermissions_Click;
        // 
        // AddPermissions
        // 
        AddPermissions.Location = new Point(7, 275);
        AddPermissions.Margin = new Padding(2, 1, 2, 1);
        AddPermissions.Name = "AddPermissions";
        AddPermissions.Size = new Size(84, 23);
        AddPermissions.TabIndex = 3;
        AddPermissions.Text = "Add";
        AddPermissions.UseVisualStyleBackColor = true;
        AddPermissions.Click += AddPermissions_Click;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(6, 34);
        label2.Margin = new Padding(2, 0, 2, 0);
        label2.Name = "label2";
        label2.Size = new Size(106, 15);
        label2.TabIndex = 1;
        label2.Text = "Permission entries:";
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(6, 9);
        label1.Margin = new Padding(2, 0, 2, 0);
        label1.Name = "label1";
        label1.Size = new Size(707, 15);
        label1.TabIndex = 0;
        label1.Text = "For additional information, double-click a permission entry. To modify a permission entry, select the entry and click Edit (if available).";
        // 
        // tabPage2
        // 
        tabPage2.BackColor = SystemColors.ControlLightLight;
        tabPage2.Location = new Point(4, 24);
        tabPage2.Margin = new Padding(2, 1, 2, 1);
        tabPage2.Name = "tabPage2";
        tabPage2.Padding = new Padding(2, 1, 2, 1);
        tabPage2.Size = new Size(736, 362);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "      Auditing      ";
        // 
        // Ok
        // 
        Ok.DialogResult = DialogResult.OK;
        Ok.Location = new Point(522, 458);
        Ok.Margin = new Padding(2, 1, 2, 1);
        Ok.Name = "Ok";
        Ok.Size = new Size(73, 23);
        Ok.TabIndex = 7;
        Ok.Text = "OK";
        Ok.UseVisualStyleBackColor = true;
        Ok.Click += Ok_Click;
        // 
        // Cancel
        // 
        Cancel.DialogResult = DialogResult.Cancel;
        Cancel.Location = new Point(602, 458);
        Cancel.Margin = new Padding(2, 1, 2, 1);
        Cancel.Name = "Cancel";
        Cancel.Size = new Size(73, 23);
        Cancel.TabIndex = 8;
        Cancel.Text = "Cancel";
        Cancel.UseVisualStyleBackColor = true;
        Cancel.Click += Cancel_Click;
        // 
        // Apply
        // 
        Apply.Enabled = false;
        Apply.Location = new Point(683, 458);
        Apply.Margin = new Padding(2, 1, 2, 1);
        Apply.Name = "Apply";
        Apply.Size = new Size(73, 23);
        Apply.TabIndex = 9;
        Apply.Text = "Apply";
        Apply.UseVisualStyleBackColor = true;
        Apply.Click += Apply_Click;
        // 
        // FileAclEditor
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(766, 488);
        Controls.Add(Apply);
        Controls.Add(groupBox1);
        Controls.Add(Cancel);
        Controls.Add(Ok);
        DoubleBuffered = true;
        Margin = new Padding(2, 1, 2, 1);
        Name = "FileAclEditor";
        Text = "RegAclEditor";
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        tableLayoutPanel1.ResumeLayout(false);
        tableLayoutPanel1.PerformLayout();
        tabControl1.ResumeLayout(false);
        tabPage1.ResumeLayout(false);
        tabPage1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)Permissions).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private GroupBox groupBox1;
    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private Label label1;
    private Button AddPermissions;
    private CheckBox checkBox1;
    private Label label2;
    private DataGridView Permissions;
    private DataGridViewPrincipalColumn PrincipalDgv;
    private DataGridViewTextBoxColumn TypeDgv;
    private DataGridViewTextBoxColumn AccessDgv;
    private DataGridViewTextBoxColumn InheritedFromDgv;
    private DataGridViewTextBoxColumn AppliesToDgv;
    private Button EditPermissions;
    private Button RemovePermissions;
    private Button Ok;
    private Button Cancel;
    private Button Apply;
    private Label label3;
    private TableLayoutPanel tableLayoutPanel1;
    private LinkLabel SelectPrincipal;
    private Label PrincipalName;
}