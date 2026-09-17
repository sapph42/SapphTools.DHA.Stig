namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common {
    partial class RuleEditor {
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
            label1 = new Label();
            RuleId = new TextBox();
            Description = new TextBox();
            label2 = new Label();
            label3 = new Label();
            Settings = new DataGridView();
            SettingOrderDgv = new DataGridViewTextBoxColumn();
            TargetTypeDgv = new DataGridViewTextBoxColumn();
            TargetDgv = new DataGridViewTextBoxColumn();
            MoveUp = new Button();
            MoveDown = new Button();
            AddSetting = new Button();
            EditSetting = new Button();
            RemoveSetting = new Button();
            Ok = new Button();
            Cancel = new Button();
            ((System.ComponentModel.ISupportInitialize)Settings).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(55, 15);
            label1.TabIndex = 0;
            label1.Text = "Plugin ID";
            // 
            // RuleId
            // 
            RuleId.Location = new Point(114, 12);
            RuleId.Name = "RuleId";
            RuleId.PlaceholderText = "327598";
            RuleId.Size = new Size(206, 23);
            RuleId.TabIndex = 1;
            RuleId.TextChanged += RuleId_TextChanged;
            // 
            // Description
            // 
            Description.Location = new Point(114, 53);
            Description.Name = "Description";
            Description.PlaceholderText = "Windows Server 2022 must have PowerShell Transcription enabled.";
            Description.Size = new Size(654, 23);
            Description.TabIndex = 2;
            Description.TextChanged += Description_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 56);
            label2.Name = "label2";
            label2.Size = new Size(67, 15);
            label2.TabIndex = 3;
            label2.Text = "Description";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 97);
            label3.Name = "label3";
            label3.Size = new Size(49, 15);
            label3.TabIndex = 4;
            label3.Text = "Settings";
            // 
            // Settings
            // 
            Settings.AllowUserToAddRows = false;
            Settings.AllowUserToDeleteRows = false;
            Settings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Settings.Columns.AddRange(new DataGridViewColumn[] { SettingOrderDgv, TargetTypeDgv, TargetDgv });
            Settings.Location = new Point(12, 115);
            Settings.MultiSelect = false;
            Settings.Name = "Settings";
            Settings.ReadOnly = true;
            Settings.RowHeadersVisible = false;
            Settings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Settings.Size = new Size(756, 249);
            Settings.TabIndex = 5;
            Settings.SelectionChanged += Settings_SelectionChanged;
            // 
            // SettingOrderDgv
            // 
            SettingOrderDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            SettingOrderDgv.HeaderText = "Order";
            SettingOrderDgv.Name = "OrderDgv";
            SettingOrderDgv.ReadOnly = true;
            SettingOrderDgv.Width = 62;
            // 
            // TargetTypeDgv
            // 
            TargetTypeDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TargetTypeDgv.HeaderText = "Target Type";
            TargetTypeDgv.Name = "TargetTypeDgv";
            TargetTypeDgv.ReadOnly = true;
            TargetTypeDgv.Width = 93;
            // 
            // TargetDgv
            // 
            TargetDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            TargetDgv.HeaderText = "Target";
            TargetDgv.Name = "TargetDgv";
            TargetDgv.ReadOnly = true;
            TargetDgv.ValueType = typeof(IValue);
            // 
            // MoveUp
            // 
            MoveUp.Enabled = false;
            MoveUp.Image = Properties.Resources.Up;
            MoveUp.Location = new Point(774, 208);
            MoveUp.Name = "MoveUp";
            MoveUp.Size = new Size(23, 23);
            MoveUp.TabIndex = 6;
            MoveUp.UseVisualStyleBackColor = true;
            MoveUp.Click += MoveUp_Click;
            // 
            // MoveDown
            // 
            MoveDown.Enabled = false;
            MoveDown.Image = Properties.Resources.Down;
            MoveDown.Location = new Point(774, 237);
            MoveDown.Name = "MoveDown";
            MoveDown.Size = new Size(23, 23);
            MoveDown.TabIndex = 7;
            MoveDown.UseVisualStyleBackColor = true;
            MoveDown.Click += MoveDown_Click;
            // 
            // AddSetting
            // 
            AddSetting.Location = new Point(12, 379);
            AddSetting.Name = "AddSetting";
            AddSetting.Size = new Size(84, 23);
            AddSetting.TabIndex = 8;
            AddSetting.Text = "Add";
            AddSetting.UseVisualStyleBackColor = true;
            AddSetting.Click += AddSetting_Click;
            // 
            // EditSetting
            // 
            EditSetting.Enabled = false;
            EditSetting.Location = new Point(109, 379);
            EditSetting.Name = "EditSetting";
            EditSetting.Size = new Size(84, 23);
            EditSetting.TabIndex = 9;
            EditSetting.Text = "Edit";
            EditSetting.UseVisualStyleBackColor = true;
            EditSetting.Click += EditSetting_Click;
            // 
            // RemoveSetting
            // 
            RemoveSetting.Enabled = false;
            RemoveSetting.Location = new Point(206, 379);
            RemoveSetting.Name = "RemoveSetting";
            RemoveSetting.Size = new Size(84, 23);
            RemoveSetting.TabIndex = 10;
            RemoveSetting.Text = "Remove";
            RemoveSetting.UseVisualStyleBackColor = true;
            RemoveSetting.Click += RemoveSetting_Click;
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(612, 436);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 11;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(693, 436);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 23);
            Cancel.TabIndex = 12;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // RuleEditor
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new Size(800, 471);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(RemoveSetting);
            Controls.Add(EditSetting);
            Controls.Add(AddSetting);
            Controls.Add(MoveDown);
            Controls.Add(MoveUp);
            Controls.Add(Settings);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(Description);
            Controls.Add(RuleId);
            Controls.Add(label1);
            Name = "RuleEditor";
            Text = "Rule Editor";
            Load += RuleEditor_Load;
            ((System.ComponentModel.ISupportInitialize)Settings).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox RuleId;
        private TextBox Description;
        private Label label2;
        private Label label3;
        private DataGridView Settings;
        private DataGridViewTextBoxColumn SettingOrderDgv;
        private DataGridViewTextBoxColumn TargetTypeDgv;
        private DataGridViewTextBoxColumn TargetDgv;
        private Button MoveUp;
        private Button MoveDown;
        private Button AddSetting;
        private Button EditSetting;
        private Button RemoveSetting;
        private Button Ok;
        private Button Cancel;
    }
}