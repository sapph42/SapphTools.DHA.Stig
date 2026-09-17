using SapphTools.DHA.Stig.Remediator.Controls;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common {
    partial class Disambiguation {
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
            Label = new Label();
            label1 = new Label();
            Results = new DataGridView();
            PrincipalDgv = new DataGridViewPrincipalColumn();
            AuthorityDgv = new DataGridViewTextBoxColumn();
            Ok = new Button();
            Cancel = new Button();
            ((System.ComponentModel.ISupportInitialize)Results).BeginInit();
            SuspendLayout();
            // 
            // Label
            // 
            Label.BackColor = Color.Transparent;
            Label.Font = new Font("Segoe UI", 9F);
            Label.Location = new Point(10, 11);
            Label.Name = "Label";
            Label.Size = new Size(374, 35);
            Label.TabIndex = 0;
            Label.Text = "More than one object matches the following object name: \"{0}\". Select an object from this list or, to reenter the name, click Cancel.";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(26, 62);
            label1.Name = "label1";
            label1.Size = new Size(99, 15);
            label1.TabIndex = 1;
            label1.Text = "&Matching names:";
            // 
            // Results
            // 
            Results.AllowUserToAddRows = false;
            Results.AllowUserToDeleteRows = false;
            Results.AllowUserToResizeRows = false;
            Results.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Results.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            Results.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Results.Columns.AddRange(new DataGridViewColumn[] { PrincipalDgv, AuthorityDgv });
            Results.Location = new Point(30, 80);
            Results.MultiSelect = false;
            Results.Name = "Results";
            Results.ReadOnly = true;
            Results.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            Results.RowHeadersVisible = false;
            Results.ScrollBars = ScrollBars.Vertical;
            Results.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Results.ShowEditingIcon = false;
            Results.ShowRowErrors = false;
            Results.Size = new Size(530, 194);
            Results.TabIndex = 2;
            Results.SelectionChanged += Results_SelectionChanged;
            // 
            // PrincipalDgv
            // 
            PrincipalDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            PrincipalDgv.HeaderText = "Principal";
            PrincipalDgv.Name = "PrincipalDgv";
            PrincipalDgv.ReadOnly = true;
            PrincipalDgv.Width = 59;
            // 
            // AuthorityDgv
            // 
            AuthorityDgv.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            AuthorityDgv.HeaderText = "Authority";
            AuthorityDgv.Name = "AuthorityDgv";
            AuthorityDgv.ReadOnly = true;
            AuthorityDgv.Width = 82;
            // 
            // Ok
            // 
            Ok.DialogResult = DialogResult.OK;
            Ok.Enabled = false;
            Ok.Location = new Point(405, 285);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 3;
            Ok.Text = "OK";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new Point(485, 285);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(75, 23);
            Cancel.TabIndex = 4;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // Disambiguation
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new Size(572, 320);
            Controls.Add(Cancel);
            Controls.Add(Ok);
            Controls.Add(Results);
            Controls.Add(label1);
            Controls.Add(Label);
            Name = "Disambiguation";
            Text = "Multiple Names Found";
            ((System.ComponentModel.ISupportInitialize)Results).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Label;
        private Label label1;
        private DataGridView Results;
        private DataGridViewPrincipalColumn PrincipalDgv;
        private DataGridViewTextBoxColumn AuthorityDgv;
        private Button Ok;
        private Button Cancel;
    }
}