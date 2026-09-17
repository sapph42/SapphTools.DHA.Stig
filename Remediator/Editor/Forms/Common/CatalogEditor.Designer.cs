namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common {
    partial class CatalogEditor {
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
            label2 = new Label();
            label3 = new Label();
            CatalogName = new TextBox();
            SchemaVersion = new Label();
            Rules = new DataGridView();
            Cancel = new Button();
            Ok = new Button();
            RemoveRule = new Button();
            EditRule = new Button();
            AddRule = new Button();
            Blacklist = new DataGridView();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)Rules).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Blacklist).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 21);
            label1.Name = "label1";
            label1.Size = new Size(83, 15);
            label1.TabIndex = 0;
            label1.Text = "Catalog Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 56);
            label2.Name = "label2";
            label2.Size = new Size(90, 15);
            label2.TabIndex = 1;
            label2.Text = "Schema Version";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 92);
            label3.Name = "label3";
            label3.Size = new Size(35, 15);
            label3.TabIndex = 2;
            label3.Text = "Rules";
            // 
            // CatalogName
            // 
            CatalogName.Location = new Point(108, 18);
            CatalogName.Name = "CatalogName";
            CatalogName.Size = new Size(324, 23);
            CatalogName.TabIndex = 4;
            // 
            // SchemaVersion
            // 
            SchemaVersion.AutoSize = true;
            SchemaVersion.Location = new Point(108, 56);
            SchemaVersion.Name = "SchemaVersion";
            SchemaVersion.Size = new Size(13, 15);
            SchemaVersion.TabIndex = 5;
            SchemaVersion.Text = "1";
            // 
            // Rules
            // 
            Rules.AllowUserToAddRows = false;
            Rules.AllowUserToDeleteRows = false;
            Rules.AllowUserToResizeRows = false;
            Rules.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Rules.Location = new Point(12, 110);
            Rules.Name = "Rules";
            Rules.RowHeadersVisible = false;
            Rules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Rules.Size = new Size(762, 428);
            Rules.TabIndex = 6;
            Rules.SelectionChanged += Rules_SelectionChanged;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(699, 768);
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
            Ok.Location = new Point(618, 768);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 16;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // RemoveRule
            // 
            RemoveRule.Enabled = false;
            RemoveRule.Location = new Point(212, 544);
            RemoveRule.Name = "RemoveRule";
            RemoveRule.Size = new Size(84, 23);
            RemoveRule.TabIndex = 15;
            RemoveRule.Text = "Remove";
            RemoveRule.UseVisualStyleBackColor = true;
            RemoveRule.Click += RemoveRule_Click;
            // 
            // EditRule
            // 
            EditRule.Enabled = false;
            EditRule.Location = new Point(115, 544);
            EditRule.Name = "EditRule";
            EditRule.Size = new Size(84, 23);
            EditRule.TabIndex = 14;
            EditRule.Text = "Edit";
            EditRule.UseVisualStyleBackColor = true;
            EditRule.Click += EditRule_Click;
            // 
            // AddRule
            // 
            AddRule.Location = new Point(18, 544);
            AddRule.Name = "AddRule";
            AddRule.Size = new Size(84, 23);
            AddRule.TabIndex = 13;
            AddRule.Text = "Add";
            AddRule.UseVisualStyleBackColor = true;
            AddRule.Click += AddRule_Click;
            // 
            // Blacklist
            // 
            Blacklist.AllowUserToResizeRows = false;
            Blacklist.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Blacklist.Location = new Point(18, 618);
            Blacklist.Name = "Blacklist";
            Blacklist.Size = new Size(581, 173);
            Blacklist.TabIndex = 18;
            Blacklist.CellValidating += Blacklist_CellValidating;
            Blacklist.RowsAdded += Blacklist_RowsAdded;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(18, 600);
            label4.Name = "label4";
            label4.Size = new Size(105, 15);
            label4.TabIndex = 19;
            label4.Text = "Blacklisted Plugins";
            // 
            // CatalogEditor
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new Size(792, 803);
            Controls.Add(label4);
            Controls.Add(Blacklist);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(RemoveRule);
            Controls.Add(EditRule);
            Controls.Add(AddRule);
            Controls.Add(Rules);
            Controls.Add(SchemaVersion);
            Controls.Add(CatalogName);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "CatalogEditor";
            Text = "Catalog Editor";
            Load += CatalogEditor_Load;
            ((System.ComponentModel.ISupportInitialize)Rules).EndInit();
            ((System.ComponentModel.ISupportInitialize)Blacklist).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox CatalogName;
        private Label SchemaVersion;
        private DataGridView Rules;
        private Button Cancel;
        private Button Ok;
        private Button RemoveRule;
        private Button EditRule;
        private Button AddRule;
        private DataGridView Blacklist;
        private Label label4;
    }
}