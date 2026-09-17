namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common {
    partial class SettingEditor {
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
            ActionType = new ComboBox();
            label1 = new Label();
            SettingsContainer = new GroupBox();
            Ok = new Button();
            Cancel = new Button();
            Dangerous = new CheckBox();
            RequiredContext = new ComboBox();
            label2 = new Label();
            SuspendLayout();
            // 
            // ActionType
            // 
            ActionType.AutoCompleteMode = AutoCompleteMode.Append;
            ActionType.AutoCompleteSource = AutoCompleteSource.ListItems;
            ActionType.DropDownStyle = ComboBoxStyle.DropDownList;
            ActionType.FormattingEnabled = true;
            ActionType.Location = new Point(126, 12);
            ActionType.Name = "ActionType";
            ActionType.Size = new Size(164, 23);
            ActionType.TabIndex = 0;
            ActionType.SelectedIndexChanged += ActionType_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(108, 15);
            label1.TabIndex = 1;
            label1.Text = "Setting Target Type";
            // 
            // SettingsContainer
            // 
            SettingsContainer.Location = new Point(12, 41);
            SettingsContainer.Name = "SettingsContainer";
            SettingsContainer.Size = new Size(592, 393);
            SettingsContainer.TabIndex = 3;
            SettingsContainer.TabStop = false;
            SettingsContainer.Text = "Setting Values";
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(439, 490);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 4;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(529, 490);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 23);
            Cancel.TabIndex = 5;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // Dangerous
            // 
            Dangerous.Location = new Point(12, 440);
            Dangerous.Name = "Dangerous";
            Dangerous.Size = new Size(278, 60);
            Dangerous.TabIndex = 6;
            Dangerous.Text = "This setting is potentially dangerous or can have unintended side effects and should not be auto-remediated by default.";
            Dangerous.UseVisualStyleBackColor = true;
            // 
            // RequiredContext
            // 
            RequiredContext.FormattingEnabled = true;
            RequiredContext.Location = new Point(439, 12);
            RequiredContext.Name = "RequiredContext";
            RequiredContext.Size = new Size(165, 23);
            RequiredContext.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(335, 15);
            label2.Name = "label2";
            label2.Size = new Size(98, 15);
            label2.TabIndex = 8;
            label2.Text = "Required Context";
            // 
            // SettingEditor
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new Size(616, 525);
            Controls.Add(label2);
            Controls.Add(RequiredContext);
            Controls.Add(Dangerous);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(SettingsContainer);
            Controls.Add(label1);
            Controls.Add(ActionType);
            Name = "SettingEditor";
            Text = "Setting Editor";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox ActionType;
        private Label label1;
        private Controls.ValueSettings ValueSetting;
        private GroupBox SettingsContainer;
        private Button Ok;
        private Button Cancel;
        private CheckBox Dangerous;
        private ComboBox RequiredContext;
        private Label label2;
    }
}