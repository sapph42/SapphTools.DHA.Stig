namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Registry {
    partial class RegistryBrowserDialog {
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
            RegistryTree = new TreeView();
            label1 = new Label();
            Ok = new Button();
            Cancel = new Button();
            SuspendLayout();
            // 
            // RegistryTree
            // 
            RegistryTree.Location = new Point(12, 27);
            RegistryTree.Name = "RegistryTree";
            RegistryTree.Size = new Size(449, 506);
            RegistryTree.TabIndex = 0;
            RegistryTree.BeforeExpand += RegistryTree_BeforeExpand;
            RegistryTree.AfterSelect += RegistryTree_AfterSelect;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(29, 15);
            label1.TabIndex = 1;
            label1.Text = "Key:";
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Enabled = false;
            Ok.Location = new Point(305, 539);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 2;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(386, 539);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 23);
            Cancel.TabIndex = 3;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // RegistryBrowserDialog
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new Size(473, 574);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(label1);
            Controls.Add(RegistryTree);
            Name = "RegistryBrowserDialog";
            Text = "Registry Key Selection";
            FormClosing += RegistryBrowserDialog_FormClosing;
            Load += RegistryBrowserDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeView RegistryTree;
        private Label label1;
        private Button Ok;
        private Button Cancel;
    }
}