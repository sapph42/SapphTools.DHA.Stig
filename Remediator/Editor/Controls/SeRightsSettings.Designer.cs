namespace SapphTools.DHA.Stig.Remediator.Editor.Controls {
    partial class SeRightsSettings {
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
            components = new System.ComponentModel.Container();
            label1 = new Label();
            AccountNames = new TextBox();
            label2 = new Label();
            ErrorProvider = new ErrorProvider(components);
            Target = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 11);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 1;
            label1.Text = "Target";
            // 
            // AccountNames
            // 
            AccountNames.Location = new Point(103, 53);
            AccountNames.Name = "AccountNames";
            AccountNames.Size = new Size(418, 23);
            AccountNames.TabIndex = 3;
            AccountNames.Validating += AccountNames_Validating;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(5, 56);
            label2.Name = "label2";
            label2.Size = new Size(92, 15);
            label2.TabIndex = 4;
            label2.Text = "Account Names";
            // 
            // ErrorProvider
            // 
            ErrorProvider.ContainerControl = this;
            // 
            // Target
            // 
            Target.FormattingEnabled = true;
            Target.Location = new Point(103, 8);
            Target.Name = "Target";
            Target.Size = new Size(418, 23);
            Target.TabIndex = 7;
            // 
            // SeRightsSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Target);
            Controls.Add(label2);
            Controls.Add(AccountNames);
            Controls.Add(label1);
            Name = "SeRightsSettings";
            Size = new Size(565, 87);
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private TextBox AccountNames;
        private Label label2;
        private ErrorProvider ErrorProvider;
        private ComboBox Target;
    }
}
