namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.File;
partial class FileAceEditor {
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
        AceTypeCombo = new ComboBox();
        tableLayoutPanel1 = new TableLayoutPanel();
        SelectPrincipal = new LinkLabel();
        PrincipalName = new Label();
        AceAppliesTo = new ComboBox();
        label3 = new Label();
        label2 = new Label();
        label1 = new Label();
        groupBox2 = new GroupBox();
        TakeOwnership = new CheckBox();
        ChangePermissions = new CheckBox();
        CreateFolders = new CheckBox();
        OnlyApply = new CheckBox();
        Clear = new Button();
        ReadPermissions = new CheckBox();
        Delete = new CheckBox();
        DeleteSubfolders = new CheckBox();
        WriteExtendedAttributes = new CheckBox();
        WriteAttributes = new CheckBox();
        CreateFiles = new CheckBox();
        ReadExtendedAttributes = new CheckBox();
        ReadAttributes = new CheckBox();
        ListFolder = new CheckBox();
        Traverse = new CheckBox();
        FullControl = new CheckBox();
        label4 = new Label();
        Ok = new Button();
        Cancel = new Button();
        groupBox1.SuspendLayout();
        tableLayoutPanel1.SuspendLayout();
        groupBox2.SuspendLayout();
        SuspendLayout();
        // 
        // groupBox1
        // 
        groupBox1.BackColor = SystemColors.ControlLightLight;
        groupBox1.Controls.Add(AceTypeCombo);
        groupBox1.Controls.Add(tableLayoutPanel1);
        groupBox1.Controls.Add(AceAppliesTo);
        groupBox1.Controls.Add(label3);
        groupBox1.Controls.Add(label2);
        groupBox1.Controls.Add(label1);
        groupBox1.Location = new Point(6, 12);
        groupBox1.Margin = new Padding(2, 1, 2, 1);
        groupBox1.Name = "groupBox1";
        groupBox1.Padding = new Padding(2, 1, 2, 1);
        groupBox1.Size = new Size(968, 111);
        groupBox1.TabIndex = 0;
        groupBox1.TabStop = false;
        // 
        // AceTypeCombo
        // 
        AceTypeCombo.FormattingEnabled = true;
        AceTypeCombo.Items.AddRange(new object[] { "Allow", "Deny" });
        AceTypeCombo.Location = new Point(84, 44);
        AceTypeCombo.Margin = new Padding(2, 1, 2, 1);
        AceTypeCombo.Name = "AceTypeCombo";
        AceTypeCombo.Size = new Size(306, 23);
        AceTypeCombo.TabIndex = 4;
        AceTypeCombo.Text = "Allow";
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
        tableLayoutPanel1.Location = new Point(85, 17);
        tableLayoutPanel1.Margin = new Padding(2, 1, 2, 0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 1;
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Size = new Size(236, 30);
        tableLayoutPanel1.TabIndex = 6;
        // 
        // SelectPrincipal
        // 
        SelectPrincipal.LinkBehavior = LinkBehavior.NeverUnderline;
        SelectPrincipal.Location = new Point(93, 0);
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
        PrincipalName.Size = new Size(87, 15);
        PrincipalName.TabIndex = 5;
        PrincipalName.Text = "TrustedInstaller";
        // 
        // AceAppliesTo
        // 
        AceAppliesTo.FormattingEnabled = true;
        AceAppliesTo.Items.AddRange(new object[] { "This folder only", "This folder, subfolders and files", "This folder and subfolders", "This folder and files", "Subfolders and files only", "Subfolders only", "Files only" });
        AceAppliesTo.Location = new Point(84, 76);
        AceAppliesTo.Margin = new Padding(2, 1, 2, 1);
        AceAppliesTo.Name = "AceAppliesTo";
        AceAppliesTo.Size = new Size(306, 23);
        AceAppliesTo.TabIndex = 3;
        AceAppliesTo.Text = "This folder, subfolders and files";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(13, 77);
        label3.Margin = new Padding(2, 0, 2, 0);
        label3.Name = "label3";
        label3.Size = new Size(63, 15);
        label3.TabIndex = 2;
        label3.Text = "Applies to:";
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(13, 47);
        label2.Margin = new Padding(2, 0, 2, 0);
        label2.Name = "label2";
        label2.Size = new Size(35, 15);
        label2.TabIndex = 1;
        label2.Text = "Type:";
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(13, 16);
        label1.Margin = new Padding(2, 0, 2, 0);
        label1.Name = "label1";
        label1.Size = new Size(56, 15);
        label1.TabIndex = 0;
        label1.Text = "Principal:";
        // 
        // groupBox2
        // 
        groupBox2.BackColor = SystemColors.ControlLightLight;
        groupBox2.Controls.Add(TakeOwnership);
        groupBox2.Controls.Add(ChangePermissions);
        groupBox2.Controls.Add(CreateFolders);
        groupBox2.Controls.Add(OnlyApply);
        groupBox2.Controls.Add(Clear);
        groupBox2.Controls.Add(ReadPermissions);
        groupBox2.Controls.Add(Delete);
        groupBox2.Controls.Add(DeleteSubfolders);
        groupBox2.Controls.Add(WriteExtendedAttributes);
        groupBox2.Controls.Add(WriteAttributes);
        groupBox2.Controls.Add(CreateFiles);
        groupBox2.Controls.Add(ReadExtendedAttributes);
        groupBox2.Controls.Add(ReadAttributes);
        groupBox2.Controls.Add(ListFolder);
        groupBox2.Controls.Add(Traverse);
        groupBox2.Controls.Add(FullControl);
        groupBox2.Controls.Add(label4);
        groupBox2.Location = new Point(6, 137);
        groupBox2.Margin = new Padding(2, 1, 2, 1);
        groupBox2.Name = "groupBox2";
        groupBox2.Padding = new Padding(2, 1, 2, 1);
        groupBox2.Size = new Size(968, 322);
        groupBox2.TabIndex = 1;
        groupBox2.TabStop = false;
        // 
        // TakeOwnership
        // 
        TakeOwnership.AutoSize = true;
        TakeOwnership.Location = new Point(486, 165);
        TakeOwnership.Margin = new Padding(2, 1, 2, 1);
        TakeOwnership.Name = "TakeOwnership";
        TakeOwnership.Size = new Size(108, 19);
        TakeOwnership.TabIndex = 18;
        TakeOwnership.Text = "Take ownership";
        TakeOwnership.UseVisualStyleBackColor = true;
        // 
        // ChangePermissions
        // 
        ChangePermissions.AutoSize = true;
        ChangePermissions.Location = new Point(487, 144);
        ChangePermissions.Margin = new Padding(2, 1, 2, 1);
        ChangePermissions.Name = "ChangePermissions";
        ChangePermissions.Size = new Size(133, 19);
        ChangePermissions.TabIndex = 17;
        ChangePermissions.Text = "Change permissions";
        ChangePermissions.UseVisualStyleBackColor = true;
        // 
        // CreateFolders
        // 
        CreateFolders.AutoSize = true;
        CreateFolders.Location = new Point(86, 170);
        CreateFolders.Margin = new Padding(2, 1, 2, 1);
        CreateFolders.Name = "CreateFolders";
        CreateFolders.Size = new Size(99, 19);
        CreateFolders.TabIndex = 16;
        CreateFolders.Text = "Create folders";
        CreateFolders.UseVisualStyleBackColor = true;
        // 
        // OnlyApply
        // 
        OnlyApply.AutoSize = true;
        OnlyApply.Location = new Point(14, 205);
        OnlyApply.Margin = new Padding(2, 1, 2, 1);
        OnlyApply.Name = "OnlyApply";
        OnlyApply.Size = new Size(443, 19);
        OnlyApply.TabIndex = 15;
        OnlyApply.Text = "Only apply these permissions to objects and/or containers within this container";
        OnlyApply.UseVisualStyleBackColor = true;
        // 
        // Clear
        // 
        Clear.Location = new Point(868, 196);
        Clear.Margin = new Padding(2, 1, 2, 1);
        Clear.Name = "Clear";
        Clear.Size = new Size(83, 23);
        Clear.TabIndex = 14;
        Clear.Text = "Clear all";
        Clear.UseVisualStyleBackColor = true;
        Clear.Click += Clear_Click;
        // 
        // ReadPermissions
        // 
        ReadPermissions.AutoSize = true;
        ReadPermissions.Location = new Point(487, 123);
        ReadPermissions.Margin = new Padding(2, 1, 2, 1);
        ReadPermissions.Name = "ReadPermissions";
        ReadPermissions.Size = new Size(118, 19);
        ReadPermissions.TabIndex = 13;
        ReadPermissions.Text = "Read permissions";
        ReadPermissions.UseVisualStyleBackColor = true;
        // 
        // Delete
        // 
        Delete.AutoSize = true;
        Delete.Location = new Point(487, 102);
        Delete.Margin = new Padding(2, 1, 2, 1);
        Delete.Name = "Delete";
        Delete.Size = new Size(59, 19);
        Delete.TabIndex = 12;
        Delete.Text = "Delete";
        Delete.UseVisualStyleBackColor = true;
        // 
        // DeleteSubfolders
        // 
        DeleteSubfolders.AutoSize = true;
        DeleteSubfolders.Location = new Point(487, 81);
        DeleteSubfolders.Margin = new Padding(2, 1, 2, 1);
        DeleteSubfolders.Name = "DeleteSubfolders";
        DeleteSubfolders.Size = new Size(117, 19);
        DeleteSubfolders.TabIndex = 11;
        DeleteSubfolders.Text = "Delete subfolders";
        DeleteSubfolders.UseVisualStyleBackColor = true;
        // 
        // WriteExtendedAttributes
        // 
        WriteExtendedAttributes.AutoSize = true;
        WriteExtendedAttributes.Location = new Point(487, 60);
        WriteExtendedAttributes.Margin = new Padding(2, 1, 2, 1);
        WriteExtendedAttributes.Name = "WriteExtendedAttributes";
        WriteExtendedAttributes.Size = new Size(158, 19);
        WriteExtendedAttributes.TabIndex = 10;
        WriteExtendedAttributes.Text = "Write extended attributes";
        WriteExtendedAttributes.UseVisualStyleBackColor = true;
        // 
        // WriteAttributes
        // 
        WriteAttributes.AutoSize = true;
        WriteAttributes.Location = new Point(487, 39);
        WriteAttributes.Margin = new Padding(2, 1, 2, 1);
        WriteAttributes.Name = "WriteAttributes";
        WriteAttributes.Size = new Size(107, 19);
        WriteAttributes.TabIndex = 9;
        WriteAttributes.Text = "Write attributes";
        WriteAttributes.UseVisualStyleBackColor = true;
        // 
        // CreateFiles
        // 
        CreateFiles.AutoSize = true;
        CreateFiles.Location = new Point(86, 148);
        CreateFiles.Margin = new Padding(2, 1, 2, 1);
        CreateFiles.Name = "CreateFiles";
        CreateFiles.Size = new Size(84, 19);
        CreateFiles.TabIndex = 8;
        CreateFiles.Text = "Create files";
        CreateFiles.UseVisualStyleBackColor = true;
        // 
        // ReadExtendedAttributes
        // 
        ReadExtendedAttributes.AutoSize = true;
        ReadExtendedAttributes.Location = new Point(86, 126);
        ReadExtendedAttributes.Margin = new Padding(2, 1, 2, 1);
        ReadExtendedAttributes.Name = "ReadExtendedAttributes";
        ReadExtendedAttributes.Size = new Size(156, 19);
        ReadExtendedAttributes.TabIndex = 7;
        ReadExtendedAttributes.Text = "Read extended attributes";
        ReadExtendedAttributes.UseVisualStyleBackColor = true;
        // 
        // ReadAttributes
        // 
        ReadAttributes.AutoSize = true;
        ReadAttributes.Location = new Point(86, 104);
        ReadAttributes.Margin = new Padding(2, 1, 2, 1);
        ReadAttributes.Name = "ReadAttributes";
        ReadAttributes.Size = new Size(105, 19);
        ReadAttributes.TabIndex = 6;
        ReadAttributes.Text = "Read attributes";
        ReadAttributes.UseVisualStyleBackColor = true;
        // 
        // ListFolder
        // 
        ListFolder.AutoSize = true;
        ListFolder.Location = new Point(86, 82);
        ListFolder.Margin = new Padding(2, 1, 2, 1);
        ListFolder.Name = "ListFolder";
        ListFolder.Size = new Size(78, 19);
        ListFolder.TabIndex = 5;
        ListFolder.Text = "List folder";
        ListFolder.UseVisualStyleBackColor = true;
        // 
        // Traverse
        // 
        Traverse.AutoSize = true;
        Traverse.Location = new Point(86, 60);
        Traverse.Margin = new Padding(2, 1, 2, 1);
        Traverse.Name = "Traverse";
        Traverse.Size = new Size(103, 19);
        Traverse.TabIndex = 4;
        Traverse.Text = "Traverse folder";
        Traverse.UseVisualStyleBackColor = true;
        // 
        // FullControl
        // 
        FullControl.AutoSize = true;
        FullControl.Location = new Point(86, 38);
        FullControl.Margin = new Padding(2, 1, 2, 1);
        FullControl.Name = "FullControl";
        FullControl.Size = new Size(88, 19);
        FullControl.TabIndex = 3;
        FullControl.Text = "Full Control";
        FullControl.UseVisualStyleBackColor = true;
        FullControl.CheckedChanged += FullControl_CheckedChanged;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new Point(13, 15);
        label4.Margin = new Padding(2, 0, 2, 0);
        label4.Name = "label4";
        label4.Size = new Size(129, 15);
        label4.TabIndex = 2;
        label4.Text = "Advanced permissions:";
        // 
        // Ok
        // 
        Ok.DialogResult = DialogResult.OK;
        Ok.Location = new Point(806, 479);
        Ok.Margin = new Padding(2, 1, 2, 1);
        Ok.Name = "Ok";
        Ok.Size = new Size(79, 22);
        Ok.TabIndex = 2;
        Ok.Text = "OK";
        Ok.UseVisualStyleBackColor = true;
        Ok.Click += Ok_Click;
        // 
        // Cancel
        // 
        Cancel.DialogResult = DialogResult.Cancel;
        Cancel.Location = new Point(893, 479);
        Cancel.Margin = new Padding(2, 1, 2, 1);
        Cancel.Name = "Cancel";
        Cancel.Size = new Size(79, 22);
        Cancel.TabIndex = 3;
        Cancel.Text = "Cancel";
        Cancel.UseVisualStyleBackColor = true;
        // 
        // FileAceEditor
        // 
        AcceptButton = Ok;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = Cancel;
        ClientSize = new Size(981, 509);
        Controls.Add(Cancel);
        Controls.Add(Ok);
        Controls.Add(groupBox2);
        Controls.Add(groupBox1);
        Margin = new Padding(2, 1, 2, 1);
        Name = "FileAceEditor";
        ShowInTaskbar = false;
        Text = "Permission Entry for File System Object";
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        tableLayoutPanel1.ResumeLayout(false);
        tableLayoutPanel1.PerformLayout();
        groupBox2.ResumeLayout(false);
        groupBox2.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private GroupBox groupBox1;
    private Label label1;
    private Label label2;
    private ComboBox AceTypeCombo;
    private ComboBox AceAppliesTo;
    private Label label3;
    private GroupBox groupBox2;
    private Label label4;
    private Button Clear;
    private CheckBox ReadPermissions;
    private CheckBox Delete;
    private CheckBox DeleteSubfolders;
    private CheckBox WriteExtendedAttributes;
    private CheckBox WriteAttributes;
    private CheckBox CreateFiles;
    private CheckBox ReadExtendedAttributes;
    private CheckBox ReadAttributes;
    private CheckBox ListFolder;
    private CheckBox Traverse;
    private CheckBox FullControl;
    private CheckBox OnlyApply;
    private Button Ok;
    private Button Cancel;
    private Label PrincipalName;
    private TableLayoutPanel tableLayoutPanel1;
    private LinkLabel SelectPrincipal;
    private CheckBox TakeOwnership;
    private CheckBox ChangePermissions;
    private CheckBox CreateFolders;
}