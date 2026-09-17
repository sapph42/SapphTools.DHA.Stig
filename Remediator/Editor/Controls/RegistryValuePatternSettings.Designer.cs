namespace SapphTools.DHA.Stig.Remediator.Editor.Controls {
    partial class RegistryValuePatternSettings {
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
            label5 = new Label();
            TargetPattern = new TextBox();
            label6 = new Label();
            Subpath = new TextBox();
            PathPattern = new TextBox();
            label7 = new Label();
            groupBox1 = new GroupBox();
            TestResult = new Label();
            TestPath = new TextBox();
            label8 = new Label();
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // Target
            // 
            Target.Location = new Point(90, 8);
            Target.Name = "Target";
            Target.Size = new Size(358, 23);
            Target.TabIndex = 0;
            Target.TextChanged += RunPatternTester;
            Target.Validating += Target_Validating;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 12);
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
            ValueName.Location = new Point(90, 168);
            ValueName.Name = "ValueName";
            ValueName.PlaceholderText = "(Default)";
            ValueName.Size = new Size(358, 23);
            ValueName.TabIndex = 3;
            ValueName.Validating += ValueName_Validating;
            // 
            // Data
            // 
            Data.Location = new Point(90, 248);
            Data.Name = "Data";
            Data.Size = new Size(358, 23);
            Data.TabIndex = 4;
            Data.Validating += Data_Validating;
            // 
            // DataType
            // 
            DataType.FormattingEnabled = true;
            DataType.Location = new Point(90, 208);
            DataType.Name = "DataType";
            DataType.Size = new Size(358, 23);
            DataType.TabIndex = 5;
            DataType.SelectedIndexChanged += DataType_SelectedIndexChanged;
            DataType.Validating += DataType_Validating;
            // 
            // Overwrite
            // 
            Overwrite.AutoSize = true;
            Overwrite.Location = new Point(3, 292);
            Overwrite.Name = "Overwrite";
            Overwrite.Size = new Size(317, 19);
            Overwrite.TabIndex = 6;
            Overwrite.Text = "Overwrite existing data with the data and kind specified";
            Overwrite.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 171);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 7;
            label2.Text = "Value Name";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 211);
            label3.Name = "label3";
            label3.Size = new Size(62, 15);
            label3.TabIndex = 8;
            label3.Text = "Value Kind";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(3, 254);
            label4.Name = "label4";
            label4.Size = new Size(31, 15);
            label4.TabIndex = 9;
            label4.Text = "Data";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(3, 51);
            label5.Name = "label5";
            label5.Size = new Size(81, 15);
            label5.TabIndex = 10;
            label5.Text = "Target Pattern";
            // 
            // TargetPattern
            // 
            TargetPattern.Location = new Point(90, 48);
            TargetPattern.Name = "TargetPattern";
            TargetPattern.Size = new Size(358, 23);
            TargetPattern.TabIndex = 11;
            TargetPattern.TextChanged += Pattern_TextChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(3, 91);
            label6.Name = "label6";
            label6.Size = new Size(51, 15);
            label6.TabIndex = 12;
            label6.Text = "Subpath";
            // 
            // Subpath
            // 
            Subpath.Location = new Point(90, 88);
            Subpath.Name = "Subpath";
            Subpath.Size = new Size(358, 23);
            Subpath.TabIndex = 13;
            Subpath.TextChanged += RunPatternTester;
            // 
            // PathPattern
            // 
            PathPattern.Location = new Point(90, 128);
            PathPattern.Name = "PathPattern";
            PathPattern.Size = new Size(358, 23);
            PathPattern.TabIndex = 15;
            PathPattern.TextChanged += Pattern_TextChanged;
            PathPattern.Validating += PathPattern_Validating;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(3, 131);
            label7.Name = "label7";
            label7.Size = new Size(72, 15);
            label7.TabIndex = 14;
            label7.Text = "Path Pattern";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(TestResult);
            groupBox1.Controls.Add(TestPath);
            groupBox1.Controls.Add(label8);
            groupBox1.Location = new Point(3, 317);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(556, 81);
            groupBox1.TabIndex = 16;
            groupBox1.TabStop = false;
            groupBox1.Text = "Pattern Tester";
            // 
            // TestResult
            // 
            TestResult.AutoSize = true;
            TestResult.Location = new Point(6, 63);
            TestResult.Name = "TestResult";
            TestResult.Size = new Size(0, 15);
            TestResult.TabIndex = 2;
            // 
            // TestPath
            // 
            TestPath.Location = new Point(67, 22);
            TestPath.Name = "TestPath";
            TestPath.Size = new Size(483, 23);
            TestPath.TabIndex = 1;
            TestPath.TextChanged += RunPatternTester;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 25);
            label8.Name = "label8";
            label8.Size = new Size(55, 15);
            label8.TabIndex = 0;
            label8.Text = "Test Path";
            // 
            // RegistryValuePatternSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox1);
            Controls.Add(PathPattern);
            Controls.Add(label7);
            Controls.Add(Subpath);
            Controls.Add(label6);
            Controls.Add(TargetPattern);
            Controls.Add(label5);
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
            Name = "RegistryValuePatternSettings";
            Size = new Size(565, 401);
            ((System.ComponentModel.ISupportInitialize)ErrorProvider).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
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
        private GroupBox groupBox1;
        private Label label8;
        private TextBox PathPattern;
        private Label label7;
        private TextBox Subpath;
        private Label label6;
        private TextBox TargetPattern;
        private Label label5;
        private Label TestResult;
        private TextBox TestPath;
    }
}
