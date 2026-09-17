namespace SapphTools.DHA.Stig.Remediator.Editor.Controls {
    partial class CertificatesValueSettings {
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
            label2 = new Label();
            ErrorProvider = new ErrorProvider(components);
            Target = new TextBox();
            ArtifactsList = new ListBox();
            Add = new Button();
            Remove = new Button();
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
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(5, 56);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 4;
            label2.Text = "Artifacts";
            // 
            // ErrorProvider
            // 
            ErrorProvider.ContainerControl = this;
            // 
            // Target
            // 
            Target.Location = new Point(103, 8);
            Target.Name = "Target";
            Target.Size = new Size(425, 23);
            Target.TabIndex = 5;
            // 
            // ArtifactsList
            // 
            ArtifactsList.FormattingEnabled = true;
            ArtifactsList.ItemHeight = 15;
            ArtifactsList.Location = new Point(103, 56);
            ArtifactsList.Name = "ArtifactsList";
            ArtifactsList.Size = new Size(425, 94);
            ArtifactsList.TabIndex = 6;
            // 
            // Add
            // 
            Add.Location = new Point(103, 156);
            Add.Name = "Add";
            Add.Size = new Size(60, 23);
            Add.TabIndex = 7;
            Add.Text = "Add";
            Add.UseVisualStyleBackColor = true;
            Add.Click += Add_Click;
            // 
            // Remove
            // 
            Remove.Location = new Point(169, 156);
            Remove.Name = "Remove";
            Remove.Size = new Size(60, 23);
            Remove.TabIndex = 8;
            Remove.Text = "Remove";
            Remove.UseVisualStyleBackColor = true;
            Remove.Click += Remove_Click;
            // 
            // CertificatesValueSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Remove);
            Controls.Add(Add);
            Controls.Add(ArtifactsList);
            Controls.Add(Target);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "CertificatesValueSettings";
            Size = new Size(565, 196);
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Label label2;
        private ErrorProvider ErrorProvider;
        private TextBox Target;
        private Button Remove;
        private Button Add;
        private ListBox ArtifactsList;
    }
}
