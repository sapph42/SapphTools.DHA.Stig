namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms {
    partial class ArrayEditor {
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
            label3 = new Label();
            Tests = new DataGridView();
            Cancel = new Button();
            Ok = new Button();
            RemoveTest = new Button();
            EditTest = new Button();
            AddTest = new Button();
            label1 = new Label();
            LocalTarget = new TextBox();
            DevTargets = new TextBox();
            label2 = new Label();
            ProdTargets = new TextBox();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)Tests).BeginInit();
            SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 92);
            label3.Name = "label3";
            label3.Size = new Size(33, 15);
            label3.TabIndex = 2;
            label3.Text = "Tests";
            // 
            // Tests
            // 
            Tests.AllowUserToAddRows = false;
            Tests.AllowUserToDeleteRows = false;
            Tests.AllowUserToResizeRows = false;
            Tests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Tests.Location = new Point(12, 110);
            Tests.Name = "Tests";
            Tests.RowHeadersVisible = false;
            Tests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Tests.Size = new Size(775, 428);
            Tests.TabIndex = 6;
            Tests.SelectionChanged += Tests_SelectionChanged;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(712, 545);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 23);
            Cancel.TabIndex = 17;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(618, 544);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 16;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // RemoveTest
            // 
            RemoveTest.Enabled = false;
            RemoveTest.Location = new Point(212, 544);
            RemoveTest.Name = "RemoveTest";
            RemoveTest.Size = new Size(84, 23);
            RemoveTest.TabIndex = 15;
            RemoveTest.Text = "Remove";
            RemoveTest.UseVisualStyleBackColor = true;
            RemoveTest.Click += RemoveTest_Click;
            // 
            // EditTest
            // 
            EditTest.Enabled = false;
            EditTest.Location = new Point(112, 544);
            EditTest.Name = "EditTest";
            EditTest.Size = new Size(84, 23);
            EditTest.TabIndex = 14;
            EditTest.Text = "Edit";
            EditTest.UseVisualStyleBackColor = true;
            EditTest.Click += EditTest_Click;
            // 
            // AddTest
            // 
            AddTest.Location = new Point(12, 545);
            AddTest.Name = "AddTest";
            AddTest.Size = new Size(84, 23);
            AddTest.TabIndex = 13;
            AddTest.Text = "Add";
            AddTest.UseVisualStyleBackColor = true;
            AddTest.Click += AddTest_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(71, 15);
            label1.TabIndex = 18;
            label1.Text = "Local Target";
            // 
            // LocalTarget
            // 
            LocalTarget.Location = new Point(89, 12);
            LocalTarget.Name = "LocalTarget";
            LocalTarget.Size = new Size(176, 23);
            LocalTarget.TabIndex = 19;
            // 
            // DevTargets
            // 
            DevTargets.Location = new Point(345, 12);
            DevTargets.Multiline = true;
            DevTargets.Name = "DevTargets";
            DevTargets.Size = new Size(176, 92);
            DevTargets.TabIndex = 21;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(271, 15);
            label2.Name = "label2";
            label2.Size = new Size(68, 15);
            label2.TabIndex = 20;
            label2.Text = "Dev Targets";
            // 
            // ProdTargets
            // 
            ProdTargets.Location = new Point(611, 12);
            ProdTargets.Multiline = true;
            ProdTargets.Name = "ProdTargets";
            ProdTargets.Size = new Size(176, 92);
            ProdTargets.TabIndex = 23;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(532, 15);
            label4.Name = "label4";
            label4.Size = new Size(73, 15);
            label4.TabIndex = 22;
            label4.Text = "Prod Targets";
            // 
            // ArrayEditor
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new Size(799, 580);
            Controls.Add(ProdTargets);
            Controls.Add(label4);
            Controls.Add(DevTargets);
            Controls.Add(label2);
            Controls.Add(LocalTarget);
            Controls.Add(label1);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(RemoveTest);
            Controls.Add(EditTest);
            Controls.Add(AddTest);
            Controls.Add(Tests);
            Controls.Add(label3);
            Name = "ArrayEditor";
            Text = "Test Array Editor";
            Load += ArrayEditor_Load;
            ((System.ComponentModel.ISupportInitialize)Tests).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label3;
        private DataGridView Tests;
        private Button Cancel;
        private Button Ok;
        private Button RemoveTest;
        private Button EditTest;
        private Button AddTest;
        private Label label1;
        private TextBox LocalTarget;
        private TextBox DevTargets;
        private Label label2;
        private TextBox ProdTargets;
        private Label label4;
    }
}