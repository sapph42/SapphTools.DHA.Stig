namespace SapphTools.DHA.Stig.Remediator.Editor.Controls {
    partial class ServiceValueSettings {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            label1 = new Label();
            Subtype = new ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 11);
            label1.Name = "label1";
            label1.Size = new Size(50, 15);
            label1.TabIndex = 0;
            label1.Text = "Subtype";
            // 
            // Subtype
            // 
            Subtype.FormattingEnabled = true;
            Subtype.Location = new Point(69, 8);
            Subtype.Name = "Subtype";
            Subtype.Size = new Size(357, 23);
            Subtype.TabIndex = 1;
            // 
            // ServiceValueSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Subtype);
            Controls.Add(label1);
            Name = "ServiceValueSettings";
            Size = new Size(565, 87);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox Subtype;
    }
}
