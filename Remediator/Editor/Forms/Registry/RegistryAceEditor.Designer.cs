namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Registry;
partial class RegistryAceEditor {
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
        OnlyApply = new CheckBox();
        Clear = new Button();
        ReadControl = new CheckBox();
        WriteOwner = new CheckBox();
        WriteDac = new CheckBox();
        Delete = new CheckBox();
        CreateLink = new CheckBox();
        Notify = new CheckBox();
        EnumerateSubkeys = new CheckBox();
        CreateSubkey = new CheckBox();
        SetValue = new CheckBox();
        QueryValue = new CheckBox();
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
        groupBox1.Size = new Size(968, 105);
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
        PrincipalName.Text = "";
        // 
        // AceAppliesTo
        // 
        AceAppliesTo.FormattingEnabled = true;
        AceAppliesTo.Items.AddRange(new object[] { "This key only", "This key and subkeys", "Subkeys only" });
        AceAppliesTo.Location = new Point(84, 76);
        AceAppliesTo.Margin = new Padding(2, 1, 2, 1);
        AceAppliesTo.Name = "AceAppliesTo";
        AceAppliesTo.Size = new Size(306, 23);
        AceAppliesTo.TabIndex = 3;
        AceAppliesTo.Text = "This key and subkeys";
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
        groupBox2.Controls.Add(OnlyApply);
        groupBox2.Controls.Add(Clear);
        groupBox2.Controls.Add(ReadControl);
        groupBox2.Controls.Add(WriteOwner);
        groupBox2.Controls.Add(WriteDac);
        groupBox2.Controls.Add(Delete);
        groupBox2.Controls.Add(CreateLink);
        groupBox2.Controls.Add(Notify);
        groupBox2.Controls.Add(EnumerateSubkeys);
        groupBox2.Controls.Add(CreateSubkey);
        groupBox2.Controls.Add(SetValue);
        groupBox2.Controls.Add(QueryValue);
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
        // OnlyApply
        // 
        OnlyApply.AutoSize = true;
        OnlyApply.Location = new Point(14, 159);
        OnlyApply.Margin = new Padding(2, 1, 2, 1);
        OnlyApply.Name = "OnlyApply";
        OnlyApply.Size = new Size(443, 19);
        OnlyApply.TabIndex = 15;
        OnlyApply.Text = "Only apply these permissions to objects and/or containers within this container";
        OnlyApply.UseVisualStyleBackColor = true;
        // 
        // Clear
        // 
        Clear.Location = new Point(868, 150);
        Clear.Margin = new Padding(2, 1, 2, 1);
        Clear.Name = "Clear";
        Clear.Size = new Size(83, 23);
        Clear.TabIndex = 14;
        Clear.Text = "Clear all";
        Clear.UseVisualStyleBackColor = true;
        Clear.Click += Clear_Click;
        // 
        // ReadControl
        // 
        ReadControl.AutoSize = true;
        ReadControl.Location = new Point(487, 111);
        ReadControl.Margin = new Padding(2, 1, 2, 1);
        ReadControl.Name = "ReadControl";
        ReadControl.Size = new Size(95, 19);
        ReadControl.TabIndex = 13;
        ReadControl.Text = "Read Control";
        ReadControl.UseVisualStyleBackColor = true;
        // 
        // WriteOwner
        // 
        WriteOwner.AutoSize = true;
        WriteOwner.Location = new Point(487, 92);
        WriteOwner.Margin = new Padding(2, 1, 2, 1);
        WriteOwner.Name = "WriteOwner";
        WriteOwner.Size = new Size(92, 19);
        WriteOwner.TabIndex = 12;
        WriteOwner.Text = "Write Owner";
        WriteOwner.UseVisualStyleBackColor = true;
        // 
        // WriteDac
        // 
        WriteDac.AutoSize = true;
        WriteDac.Location = new Point(487, 75);
        WriteDac.Margin = new Padding(2, 1, 2, 1);
        WriteDac.Name = "WriteDac";
        WriteDac.Size = new Size(81, 19);
        WriteDac.TabIndex = 11;
        WriteDac.Text = "Write DAC";
        WriteDac.UseVisualStyleBackColor = true;
        // 
        // Delete
        // 
        Delete.AutoSize = true;
        Delete.Location = new Point(487, 57);
        Delete.Margin = new Padding(2, 1, 2, 1);
        Delete.Name = "Delete";
        Delete.Size = new Size(59, 19);
        Delete.TabIndex = 10;
        Delete.Text = "Delete";
        Delete.UseVisualStyleBackColor = true;
        // 
        // CreateLink
        // 
        CreateLink.AutoSize = true;
        CreateLink.Location = new Point(487, 39);
        CreateLink.Margin = new Padding(2, 1, 2, 1);
        CreateLink.Name = "CreateLink";
        CreateLink.Size = new Size(85, 19);
        CreateLink.TabIndex = 9;
        CreateLink.Text = "Create Link";
        CreateLink.UseVisualStyleBackColor = true;
        // 
        // Notify
        // 
        Notify.AutoSize = true;
        Notify.Location = new Point(86, 128);
        Notify.Margin = new Padding(2, 1, 2, 1);
        Notify.Name = "Notify";
        Notify.Size = new Size(59, 19);
        Notify.TabIndex = 8;
        Notify.Text = "Notify";
        Notify.UseVisualStyleBackColor = true;
        // 
        // EnumerateSubkeys
        // 
        EnumerateSubkeys.AutoSize = true;
        EnumerateSubkeys.Location = new Point(86, 110);
        EnumerateSubkeys.Margin = new Padding(2, 1, 2, 1);
        EnumerateSubkeys.Name = "EnumerateSubkeys";
        EnumerateSubkeys.Size = new Size(129, 19);
        EnumerateSubkeys.TabIndex = 7;
        EnumerateSubkeys.Text = "Enumerate Subkeys";
        EnumerateSubkeys.UseVisualStyleBackColor = true;
        // 
        // CreateSubkey
        // 
        CreateSubkey.AutoSize = true;
        CreateSubkey.Location = new Point(86, 92);
        CreateSubkey.Margin = new Padding(2, 1, 2, 1);
        CreateSubkey.Name = "CreateSubkey";
        CreateSubkey.Size = new Size(101, 19);
        CreateSubkey.TabIndex = 6;
        CreateSubkey.Text = "Create Subkey";
        CreateSubkey.UseVisualStyleBackColor = true;
        // 
        // SetValue
        // 
        SetValue.AutoSize = true;
        SetValue.Location = new Point(86, 74);
        SetValue.Margin = new Padding(2, 1, 2, 1);
        SetValue.Name = "SetValue";
        SetValue.Size = new Size(73, 19);
        SetValue.TabIndex = 5;
        SetValue.Text = "Set Value";
        SetValue.UseVisualStyleBackColor = true;
        // 
        // QueryValue
        // 
        QueryValue.AutoSize = true;
        QueryValue.Location = new Point(86, 56);
        QueryValue.Margin = new Padding(2, 1, 2, 1);
        QueryValue.Name = "QueryValue";
        QueryValue.Size = new Size(89, 19);
        QueryValue.TabIndex = 4;
        QueryValue.Text = "Query Value";
        QueryValue.UseVisualStyleBackColor = true;
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
        // RegistryAceEditor
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
        Name = "RegistryAceEditor";
        ShowInTaskbar = false;
        Text = "Permission Entry for Registry Key Object";
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
    private CheckBox ReadControl;
    private CheckBox WriteOwner;
    private CheckBox WriteDac;
    private CheckBox Delete;
    private CheckBox CreateLink;
    private CheckBox Notify;
    private CheckBox EnumerateSubkeys;
    private CheckBox CreateSubkey;
    private CheckBox SetValue;
    private CheckBox QueryValue;
    private CheckBox FullControl;
    private CheckBox OnlyApply;
    private Button Ok;
    private Button Cancel;
    private Label PrincipalName;
    private TableLayoutPanel tableLayoutPanel1;
    private LinkLabel SelectPrincipal;
}