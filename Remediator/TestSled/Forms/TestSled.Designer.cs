namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms {
    partial class TestSled {
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
            label1 = new Label();
            TestSledDefinitionPath = new TextBox();
            label2 = new Label();
            TestSledResultsPath = new TextBox();
            Tests = new DataGridView();
            label3 = new Label();
            DefBrowse = new Button();
            ResultBrowse = new Button();
            Status = new Label();
            RunTests = new Button();
            ((System.ComponentModel.ISupportInitialize)Tests).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(129, 15);
            label1.TabIndex = 0;
            label1.Text = "Test Sled Definition File";
            // 
            // TestSledDefinitionPath
            // 
            TestSledDefinitionPath.Location = new Point(173, 12);
            TestSledDefinitionPath.Name = "TestSledDefinitionPath";
            TestSledDefinitionPath.Size = new Size(641, 23);
            TestSledDefinitionPath.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 53);
            label2.Name = "label2";
            label2.Size = new Size(114, 15);
            label2.TabIndex = 2;
            label2.Text = "Test Sled Results File";
            // 
            // TestSledResultsPath
            // 
            TestSledResultsPath.Location = new Point(173, 50);
            TestSledResultsPath.Name = "TestSledResultsPath";
            TestSledResultsPath.Size = new Size(641, 23);
            TestSledResultsPath.TabIndex = 3;
            // 
            // Tests
            // 
            Tests.AllowUserToAddRows = false;
            Tests.AllowUserToDeleteRows = false;
            Tests.AllowUserToResizeRows = false;
            Tests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Tests.Location = new Point(12, 113);
            Tests.Name = "Tests";
            Tests.RowHeadersVisible = false;
            Tests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Tests.Size = new Size(973, 438);
            Tests.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 95);
            label3.Name = "label3";
            label3.Size = new Size(93, 15);
            label3.TabIndex = 8;
            label3.Text = "Test Sled Results";
            // 
            // DefBrowse
            // 
            DefBrowse.Location = new Point(820, 12);
            DefBrowse.Name = "DefBrowse";
            DefBrowse.Size = new Size(75, 23);
            DefBrowse.TabIndex = 9;
            DefBrowse.Text = "Browse";
            DefBrowse.UseVisualStyleBackColor = true;
            DefBrowse.Click += DefBrowse_Click;
            // 
            // ResultBrowse
            // 
            ResultBrowse.Location = new Point(820, 50);
            ResultBrowse.Name = "ResultBrowse";
            ResultBrowse.Size = new Size(75, 23);
            ResultBrowse.TabIndex = 10;
            ResultBrowse.Text = "Browse";
            ResultBrowse.UseVisualStyleBackColor = true;
            ResultBrowse.Click += ResultBrowse_Click;
            // 
            // Status
            // 
            Status.AutoSize = true;
            Status.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Status.ForeColor = Color.Red;
            Status.Location = new Point(173, 95);
            Status.Name = "Status";
            Status.Size = new Size(40, 15);
            Status.TabIndex = 11;
            Status.Text = "label4";
            // 
            // RunTests
            // 
            RunTests.Location = new Point(910, 84);
            RunTests.Name = "RunTests";
            RunTests.Size = new Size(75, 23);
            RunTests.TabIndex = 12;
            RunTests.Text = "Run Tests";
            RunTests.UseVisualStyleBackColor = true;
            RunTests.Click += RunTests_Click;
            // 
            // TestSled
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(998, 563);
            Controls.Add(RunTests);
            Controls.Add(Status);
            Controls.Add(ResultBrowse);
            Controls.Add(DefBrowse);
            Controls.Add(label3);
            Controls.Add(Tests);
            Controls.Add(TestSledResultsPath);
            Controls.Add(label2);
            Controls.Add(TestSledDefinitionPath);
            Controls.Add(label1);
            Name = "TestSled";
            Text = "Test Sled";
            Load += TestSled_Load;
            ((System.ComponentModel.ISupportInitialize)Tests).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox TestSledDefinitionPath;
        private Label label2;
        private TextBox TestSledResultsPath;
        private DataGridView Tests;
        private Label label3;
        private Button DefBrowse;
        private Button ResultBrowse;
        private Label Status;
        private Button RunTests;
    }
}