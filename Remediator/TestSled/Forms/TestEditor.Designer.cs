namespace SapphTools.DHA.Stig.Remediator.TestSled.Forms {
    partial class TestEditor {
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
            TestId = new TextBox();
            label2 = new Label();
            label3 = new Label();
            Phase = new TextBox();
            Order = new TextBox();
            groupBox1 = new GroupBox();
            ConfigRollback = new CheckBox();
            ConfigWhatIf = new CheckBox();
            ConfigElevate = new CheckBox();
            ConfigTarget = new ComboBox();
            label4 = new Label();
            TestSuite = new DataGridView();
            label5 = new Label();
            label6 = new Label();
            RecoverySuite = new DataGridView();
            RemoveTestCase = new Button();
            EditTestCase = new Button();
            AddTestCase = new Button();
            RemoveRecoveryCase = new Button();
            EditRecoveryCase = new Button();
            AddRecoveryCase = new Button();
            Cancel = new Button();
            Ok = new Button();
            MoveTestDown = new Button();
            MoveTestUp = new Button();
            MoveRecoveryDown = new Button();
            MoveRecoveryUp = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TestSuite).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RecoverySuite).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 14);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 0;
            label1.Text = "TestId";
            // 
            // TestId
            // 
            TestId.Location = new Point(56, 12);
            TestId.Name = "TestId";
            TestId.ReadOnly = true;
            TestId.Size = new Size(31, 23);
            TestId.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 67);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 3;
            label2.Text = "Phase";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(13, 117);
            label3.Name = "label3";
            label3.Size = new Size(37, 15);
            label3.TabIndex = 4;
            label3.Text = "Order";
            // 
            // Phase
            // 
            Phase.Location = new Point(56, 65);
            Phase.Name = "Phase";
            Phase.Size = new Size(31, 23);
            Phase.TabIndex = 5;
            // 
            // Order
            // 
            Order.Location = new Point(56, 115);
            Order.Name = "Order";
            Order.ReadOnly = true;
            Order.Size = new Size(31, 23);
            Order.TabIndex = 6;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(ConfigRollback);
            groupBox1.Controls.Add(ConfigWhatIf);
            groupBox1.Controls.Add(ConfigElevate);
            groupBox1.Controls.Add(ConfigTarget);
            groupBox1.Controls.Add(label4);
            groupBox1.Location = new Point(730, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(315, 136);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Test Config";
            // 
            // ConfigRollback
            // 
            ConfigRollback.AutoSize = true;
            ConfigRollback.Location = new Point(6, 105);
            ConfigRollback.Name = "ConfigRollback";
            ConfigRollback.Size = new Size(118, 19);
            ConfigRollback.TabIndex = 4;
            ConfigRollback.Text = "Attempt Rollback";
            ConfigRollback.UseVisualStyleBackColor = true;
            // 
            // ConfigWhatIf
            // 
            ConfigWhatIf.AutoSize = true;
            ConfigWhatIf.Location = new Point(6, 80);
            ConfigWhatIf.Name = "ConfigWhatIf";
            ConfigWhatIf.Size = new Size(85, 19);
            ConfigWhatIf.TabIndex = 3;
            ConfigWhatIf.Text = "WhatIf Test";
            ConfigWhatIf.UseVisualStyleBackColor = true;
            // 
            // ConfigElevate
            // 
            ConfigElevate.AutoSize = true;
            ConfigElevate.Location = new Point(6, 55);
            ConfigElevate.Name = "ConfigElevate";
            ConfigElevate.Size = new Size(124, 19);
            ConfigElevate.TabIndex = 2;
            ConfigElevate.Text = "Elevation Required";
            ConfigElevate.UseVisualStyleBackColor = true;
            // 
            // ConfigTarget
            // 
            ConfigTarget.FormattingEnabled = true;
            ConfigTarget.Location = new Point(52, 26);
            ConfigTarget.Name = "ConfigTarget";
            ConfigTarget.Size = new Size(157, 23);
            ConfigTarget.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 29);
            label4.Name = "label4";
            label4.Size = new Size(40, 15);
            label4.TabIndex = 0;
            label4.Text = "Target";
            // 
            // TestSuite
            // 
            TestSuite.AllowUserToAddRows = false;
            TestSuite.AllowUserToDeleteRows = false;
            TestSuite.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            TestSuite.EditMode = DataGridViewEditMode.EditProgrammatically;
            TestSuite.Location = new Point(12, 178);
            TestSuite.MultiSelect = false;
            TestSuite.Name = "TestSuite";
            TestSuite.RowHeadersVisible = false;
            TestSuite.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            TestSuite.Size = new Size(500, 374);
            TestSuite.TabIndex = 8;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 160);
            label5.Name = "label5";
            label5.Size = new Size(89, 15);
            label5.TabIndex = 9;
            label5.Text = "Test Suite Items";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(545, 160);
            label6.Name = "label6";
            label6.Size = new Size(116, 15);
            label6.TabIndex = 11;
            label6.Text = "Recovery Suite Items";
            // 
            // RecoverySuite
            // 
            RecoverySuite.AllowUserToAddRows = false;
            RecoverySuite.AllowUserToDeleteRows = false;
            RecoverySuite.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            RecoverySuite.EditMode = DataGridViewEditMode.EditProgrammatically;
            RecoverySuite.Location = new Point(545, 178);
            RecoverySuite.MultiSelect = false;
            RecoverySuite.Name = "RecoverySuite";
            RecoverySuite.RowHeadersVisible = false;
            RecoverySuite.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            RecoverySuite.Size = new Size(500, 374);
            RecoverySuite.TabIndex = 10;
            // 
            // RemoveTestCase
            // 
            RemoveTestCase.Enabled = false;
            RemoveTestCase.Location = new Point(212, 557);
            RemoveTestCase.Name = "RemoveTestCase";
            RemoveTestCase.Size = new Size(84, 23);
            RemoveTestCase.TabIndex = 26;
            RemoveTestCase.Text = "Remove";
            RemoveTestCase.UseVisualStyleBackColor = true;
            RemoveTestCase.Click += RemoveTestCase_Click;
            // 
            // EditTestCase
            // 
            EditTestCase.Enabled = false;
            EditTestCase.Location = new Point(112, 557);
            EditTestCase.Name = "EditTestCase";
            EditTestCase.Size = new Size(84, 23);
            EditTestCase.TabIndex = 25;
            EditTestCase.Text = "Edit";
            EditTestCase.UseVisualStyleBackColor = true;
            EditTestCase.Click += EditTestCase_Click;
            // 
            // AddTestCase
            // 
            AddTestCase.Location = new Point(12, 558);
            AddTestCase.Name = "AddTestCase";
            AddTestCase.Size = new Size(84, 23);
            AddTestCase.TabIndex = 24;
            AddTestCase.Text = "Add";
            AddTestCase.UseVisualStyleBackColor = true;
            AddTestCase.Click += AddTestCase_Click;
            // 
            // RemoveRecoveryCase
            // 
            RemoveRecoveryCase.Enabled = false;
            RemoveRecoveryCase.Location = new Point(745, 557);
            RemoveRecoveryCase.Name = "RemoveRecoveryCase";
            RemoveRecoveryCase.Size = new Size(84, 23);
            RemoveRecoveryCase.TabIndex = 29;
            RemoveRecoveryCase.Text = "Remove";
            RemoveRecoveryCase.UseVisualStyleBackColor = true;
            RemoveRecoveryCase.Click += RemoveRecoveryCase_Click;
            // 
            // EditRecoveryCase
            // 
            EditRecoveryCase.Enabled = false;
            EditRecoveryCase.Location = new Point(645, 557);
            EditRecoveryCase.Name = "EditRecoveryCase";
            EditRecoveryCase.Size = new Size(84, 23);
            EditRecoveryCase.TabIndex = 28;
            EditRecoveryCase.Text = "Edit";
            EditRecoveryCase.UseVisualStyleBackColor = true;
            EditRecoveryCase.Click += EditRecoveryCase_Click;
            // 
            // AddRecoveryCase
            // 
            AddRecoveryCase.Location = new Point(545, 558);
            AddRecoveryCase.Name = "AddRecoveryCase";
            AddRecoveryCase.Size = new Size(84, 23);
            AddRecoveryCase.TabIndex = 27;
            AddRecoveryCase.Text = "Add";
            AddRecoveryCase.UseVisualStyleBackColor = true;
            AddRecoveryCase.Click += AddRecoveryCase_Click;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(970, 589);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 23);
            Cancel.TabIndex = 31;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(876, 588);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 30;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // MoveTestDown
            // 
            MoveTestDown.Enabled = false;
            MoveTestDown.Image = Resources.Down;
            MoveTestDown.Location = new Point(518, 246);
            MoveTestDown.Name = "MoveTestDown";
            MoveTestDown.Size = new Size(23, 23);
            MoveTestDown.TabIndex = 33;
            MoveTestDown.UseVisualStyleBackColor = true;
            MoveTestDown.Click += MoveTestDown_Click;
            // 
            // MoveTestUp
            // 
            MoveTestUp.Enabled = false;
            MoveTestUp.Image = Resources.Up;
            MoveTestUp.Location = new Point(518, 217);
            MoveTestUp.Name = "MoveTestUp";
            MoveTestUp.Size = new Size(23, 23);
            MoveTestUp.TabIndex = 32;
            MoveTestUp.UseVisualStyleBackColor = true;
            MoveTestUp.Click += MoveTestUp_Click;
            // 
            // MoveRecoveryDown
            // 
            MoveRecoveryDown.Enabled = false;
            MoveRecoveryDown.Image = Resources.Down;
            MoveRecoveryDown.Location = new Point(1051, 246);
            MoveRecoveryDown.Name = "MoveRecoveryDown";
            MoveRecoveryDown.Size = new Size(23, 23);
            MoveRecoveryDown.TabIndex = 35;
            MoveRecoveryDown.UseVisualStyleBackColor = true;
            MoveRecoveryDown.Click += MoveRecoveryDown_Click;
            // 
            // MoveRecoveryUp
            // 
            MoveRecoveryUp.Enabled = false;
            MoveRecoveryUp.Image = Resources.Up;
            MoveRecoveryUp.Location = new Point(1051, 217);
            MoveRecoveryUp.Name = "MoveRecoveryUp";
            MoveRecoveryUp.Size = new Size(23, 23);
            MoveRecoveryUp.TabIndex = 34;
            MoveRecoveryUp.UseVisualStyleBackColor = true;
            MoveRecoveryUp.Click += MoveRecoveryUp_Click;
            // 
            // TestEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1081, 624);
            Controls.Add(MoveRecoveryDown);
            Controls.Add(MoveRecoveryUp);
            Controls.Add(MoveTestDown);
            Controls.Add(MoveTestUp);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(RemoveRecoveryCase);
            Controls.Add(EditRecoveryCase);
            Controls.Add(AddRecoveryCase);
            Controls.Add(RemoveTestCase);
            Controls.Add(EditTestCase);
            Controls.Add(AddTestCase);
            Controls.Add(label6);
            Controls.Add(RecoverySuite);
            Controls.Add(label5);
            Controls.Add(TestSuite);
            Controls.Add(groupBox1);
            Controls.Add(Order);
            Controls.Add(Phase);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(TestId);
            Controls.Add(label1);
            Name = "TestEditor";
            Text = "Test Editor";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TestSuite).EndInit();
            ((System.ComponentModel.ISupportInitialize)RecoverySuite).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox TestId;
        private Label label2;
        private Label label3;
        private TextBox Phase;
        private TextBox Order;
        private GroupBox groupBox1;
        private CheckBox ConfigRollback;
        private CheckBox ConfigWhatIf;
        private CheckBox ConfigElevate;
        private ComboBox ConfigTarget;
        private Label label4;
        private DataGridView TestSuite;
        private Label label5;
        private Label label6;
        private DataGridView RecoverySuite;
        private Button RemoveTestCase;
        private Button EditTestCase;
        private Button AddTestCase;
        private Button RemoveRecoveryCase;
        private Button EditRecoveryCase;
        private Button AddRecoveryCase;
        private Button Cancel;
        private Button Ok;
        private Button MoveTestDown;
        private Button MoveTestUp;
        private Button MoveRecoveryDown;
        private Button MoveRecoveryUp;
    }
}