namespace SapphTools.DHA.Stig.Remediator.Editor.Controls {
    partial class RegistryValueSettings {
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
            ErrorProvider = new ErrorProvider(components);
            ValueName = new TextBox();
            Data = new TextBox();
            DataType = new ComboBox();
            Overwrite = new CheckBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).BeginInit();
            SuspendLayout();
            // 
            // Target
            // 
            Target.Location = new Point(81, 8);
            Target.Name = "Target";
            Target.Size = new Size(367, 23);
            Target.TabIndex = 0;
            Target.Validating += Target_Validating;
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
            // ErrorProvider
            // 
            ErrorProvider.ContainerControl = this;
            // 
            // ValueName
            // 
            ValueName.Location = new Point(81, 50);
            ValueName.Name = "ValueName";
            ValueName.PlaceholderText = "(Default)";
            ValueName.Size = new Size(367, 23);
            ValueName.TabIndex = 3;
            ValueName.Validating += ValueName_Validating;
            // 
            // Data
            // 
            Data.Location = new Point(81, 134);
            Data.Name = "Data";
            Data.Size = new Size(367, 23);
            Data.TabIndex = 4;
            Data.Validating += Data_Validating;
            // 
            // DataType
            // 
            DataType.FormattingEnabled = true;
            DataType.Location = new Point(82, 92);
            DataType.Name = "DataType";
            DataType.Size = new Size(366, 23);
            DataType.TabIndex = 5;
            DataType.SelectedIndexChanged += DataType_SelectedIndexChanged;
            DataType.Validating += DataType_Validating;
            // 
            // Overwrite
            // 
            Overwrite.AutoSize = true;
            Overwrite.Location = new Point(3, 236);
            Overwrite.Name = "Overwrite";
            Overwrite.Size = new Size(317, 19);
            Overwrite.TabIndex = 6;
            Overwrite.Text = "Overwrite existing data with the data and kind specified";
            Overwrite.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 53);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 7;
            label2.Text = "Value Name";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 95);
            label3.Name = "label3";
            label3.Size = new Size(62, 15);
            label3.TabIndex = 8;
            label3.Text = "Value Kind";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(3, 137);
            label4.Name = "label4";
            label4.Size = new Size(31, 15);
            label4.TabIndex = 9;
            label4.Text = "Data";
            // 
            // RegistryValueSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(Overwrite);
            Controls.Add(DataType);
            Controls.Add(Data);
            Controls.Add(ValueName);
            Controls.Add(Browse);
            Controls.Add(label1);
            Controls.Add(Target);
            Name = "RegistryValueSettings";
            Size = new Size(565, 258);
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox Target;
        private Label label1;
        private Button Browse;
        private ErrorProvider ErrorProvider;
        private TextBox ValueName;
        private Label label4;
        private Label label3;
        private Label label2;
        private CheckBox Overwrite;
        private ComboBox DataType;
        private TextBox Data;
    }
}
