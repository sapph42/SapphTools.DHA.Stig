namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common {
    partial class PrincipalPicker {
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
            Ok = new Button();
            Cancel = new Button();
            Check = new Button();
            PrincipalName = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // Ok
            // 
            Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(290, 56);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 0;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // Cancel
            // 
            Cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(371, 56);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 23);
            Cancel.TabIndex = 1;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // Check
            // 
            Check.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Check.Location = new Point(357, 26);
            Check.Name = "Check";
            Check.Size = new Size(89, 23);
            Check.TabIndex = 2;
            Check.Text = "&Check Names";
            Check.UseVisualStyleBackColor = true;
            Check.Click += Check_Click;
            // 
            // PrincipalName
            // 
            PrincipalName.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            PrincipalName.Location = new Point(12, 27);
            PrincipalName.Name = "PrincipalName";
            PrincipalName.Size = new Size(339, 23);
            PrincipalName.TabIndex = 3;
            PrincipalName.TextChanged += Name_TextChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(170, 15);
            label1.TabIndex = 4;
            label1.Text = "Enter the object name to select";
            // 
            // PrincipalPicker
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new Size(458, 87);
            ControlBox = false;
            Controls.Add(label1);
            Controls.Add(PrincipalName);
            Controls.Add(Check);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PrincipalPicker";
            Text = "Select Principal";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Ok;
        private Button Cancel;
        private Button Check;
        private TextBox PrincipalName;
        private Label label1;
    }
}