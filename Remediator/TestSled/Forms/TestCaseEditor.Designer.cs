namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms {
    partial class TestCaseEditor {
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
            label5 = new Label();
            SettingView = new DataGridView();
            ExpectedResult = new ComboBox();
            label1 = new Label();
            EditSetting = new Button();
            SettingsContainer = new GroupBox();
            ValueSetting = new ValueSettings();
            Ok = new Button();
            Cancel = new Button();
            ((System.ComponentModel.ISupportInitialize)SettingView).BeginInit();
            SettingsContainer.SuspendLayout();
            SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 9);
            label5.Name = "label5";
            label5.Size = new Size(44, 15);
            label5.TabIndex = 11;
            label5.Text = "Setting";
            // 
            // SettingView
            // 
            SettingView.AllowUserToAddRows = false;
            SettingView.AllowUserToDeleteRows = false;
            SettingView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            SettingView.EditMode = DataGridViewEditMode.EditProgrammatically;
            SettingView.Location = new Point(12, 27);
            SettingView.MultiSelect = false;
            SettingView.Name = "SettingView";
            SettingView.RowHeadersVisible = false;
            SettingView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            SettingView.Size = new Size(776, 55);
            SettingView.TabIndex = 10;
            // 
            // ExpectedResult
            // 
            ExpectedResult.FormattingEnabled = true;
            ExpectedResult.Location = new Point(107, 109);
            ExpectedResult.Name = "ExpectedResult";
            ExpectedResult.Size = new Size(237, 23);
            ExpectedResult.TabIndex = 12;
            ExpectedResult.SelectedIndexChanged += ExpectedResult_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 112);
            label1.Name = "label1";
            label1.Size = new Size(89, 15);
            label1.TabIndex = 13;
            label1.Text = "Expected Result";
            // 
            // EditSetting
            // 
            EditSetting.Location = new Point(713, 88);
            EditSetting.Name = "EditSetting";
            EditSetting.Size = new Size(75, 23);
            EditSetting.TabIndex = 14;
            EditSetting.Text = "Edit Setting";
            EditSetting.UseVisualStyleBackColor = true;
            EditSetting.Click += EditSetting_Click;
            // 
            // SettingsContainer
            // 
            SettingsContainer.Controls.Add(ValueSetting);
            SettingsContainer.Location = new Point(12, 138);
            SettingsContainer.Name = "SettingsContainer";
            SettingsContainer.Size = new Size(776, 346);
            SettingsContainer.TabIndex = 15;
            SettingsContainer.TabStop = false;
            SettingsContainer.Text = "Expected After State";
            // 
            // ValueSetting
            // 
            ValueSetting.Location = new Point(6, 22);
            ValueSetting.Name = "ValueSetting";
            ValueSetting.Size = new Size(764, 318);
            ValueSetting.TabIndex = 0;
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(632, 490);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 16;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(713, 490);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 23);
            Cancel.TabIndex = 17;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // TestCaseEditor
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new Size(801, 524);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(SettingsContainer);
            Controls.Add(EditSetting);
            Controls.Add(label1);
            Controls.Add(ExpectedResult);
            Controls.Add(label5);
            Controls.Add(SettingView);
            Name = "TestCaseEditor";
            Text = "Test Case Editor";
            ((System.ComponentModel.ISupportInitialize)SettingView).EndInit();
            SettingsContainer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label5;
        private DataGridView SettingView;
        private ComboBox ExpectedResult;
        private Label label1;
        private Button EditSetting;
        private GroupBox SettingsContainer;
        private ValueSettings ValueSetting;
        private Button Ok;
        private Button Cancel;
    }
}