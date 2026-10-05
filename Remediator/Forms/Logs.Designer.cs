namespace SapphTools.DHA.Stig.Remediator.Forms {
    partial class Logs {
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
            LogsView = new DataGridView();
            Rollback = new Button();
            ((System.ComponentModel.ISupportInitialize)LogsView).BeginInit();
            SuspendLayout();
            // 
            // Actions
            // 
            LogsView.AllowUserToAddRows = false;
            LogsView.AllowUserToDeleteRows = false;
            LogsView.AllowUserToResizeRows = false;
            LogsView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LogsView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            LogsView.Location = new Point(12, 12);
            LogsView.Name = "Actions";
            LogsView.RowHeadersVisible = false;
            LogsView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            LogsView.Size = new Size(1211, 611);
            LogsView.TabIndex = 0;
            LogsView.ColumnHeaderMouseClick += Actions_ColumnHeaderMouseClick;
            LogsView.SelectionChanged += Actions_SelectionChanged;
            // 
            // Rollback
            // 
            Rollback.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Rollback.BackColor = Color.Red;
            Rollback.Location = new Point(542, 629);
            Rollback.Name = "Rollback";
            Rollback.Size = new Size(158, 34);
            Rollback.TabIndex = 1;
            Rollback.Text = "Rollback Selected Setting";
            Rollback.UseVisualStyleBackColor = false;
            Rollback.Click += Rollback_Click;
            // 
            // Logs
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1235, 675);
            Controls.Add(Rollback);
            Controls.Add(LogsView);
            Name = "Logs";
            Text = "Logs";
            WindowState = FormWindowState.Maximized;
            Load += Logs_Load;
            ((System.ComponentModel.ISupportInitialize)LogsView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView LogsView;
        private Button Rollback;
    }
}