namespace SapphTools.DHA.Stig.Remediator.Editor.Controls {
    partial class RegistryAclSettings {
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
            Target = new TextBox();
            label1 = new Label();
            Browse = new Button();
            Permissions = new TextBox();
            label2 = new Label();
            BuildSddl = new Button();
            ValidateSddl = new Button();
            ErrorProvider = new ErrorProvider(components);
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).BeginInit();
            SuspendLayout();
            // 
            // Target
            // 
            Target.Location = new Point(81, 8);
            Target.Name = "Target";
            Target.Size = new Size(367, 23);
            Target.TabIndex = 0;
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
            // Browse
            // 
            Browse.Location = new Point(454, 8);
            Browse.Name = "Browse";
            Browse.Size = new Size(105, 23);
            Browse.TabIndex = 2;
            Browse.Text = "Browse";
            Browse.UseVisualStyleBackColor = true;
            Browse.Click += Browse_Click;
            // 
            // Permissions
            // 
            Permissions.Location = new Point(81, 53);
            Permissions.Name = "Permissions";
            Permissions.Size = new Size(367, 23);
            Permissions.TabIndex = 3;
            Permissions.Validating += Permissions_Validating;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(5, 56);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 4;
            label2.Text = "Permissions";
            // 
            // BuildSddl
            // 
            BuildSddl.Location = new Point(484, 53);
            BuildSddl.Name = "BuildSddl";
            BuildSddl.Size = new Size(75, 23);
            BuildSddl.TabIndex = 5;
            BuildSddl.Text = "Build";
            BuildSddl.UseVisualStyleBackColor = true;
            BuildSddl.Click += BuildSddl_Click;
            // 
            // Validate
            // 
            ValidateSddl.BackgroundImage = Properties.Resources.Validate;
            ValidateSddl.BackgroundImageLayout = ImageLayout.Zoom;
            ValidateSddl.Location = new Point(454, 53);
            ValidateSddl.Name = "Validate";
            ValidateSddl.Size = new Size(24, 23);
            ValidateSddl.TabIndex = 6;
            ValidateSddl.UseVisualStyleBackColor = true;
            ValidateSddl.Click += Validate_Click;
            // 
            // ErrorProvider
            // 
            ErrorProvider.ContainerControl = this;
            // 
            // FileSystemAclSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(ValidateSddl);
            Controls.Add(BuildSddl);
            Controls.Add(label2);
            Controls.Add(Permissions);
            Controls.Add(Browse);
            Controls.Add(label1);
            Controls.Add(Target);
            Name = "FileSystemAclSettings";
            Size = new Size(565, 87);
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox Target;
        private Label label1;
        private Button Browse;
        private TextBox Permissions;
        private Label label2;
        private Button BuildSddl;
        private Button ValidateSddl;
        private ErrorProvider ErrorProvider;
    }
}
