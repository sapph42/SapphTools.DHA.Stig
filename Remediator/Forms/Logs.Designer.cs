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
            Actions = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)Actions).BeginInit();
            SuspendLayout();
            // 
            // Actions
            // 
            Actions.AllowUserToAddRows = false;
            Actions.AllowUserToDeleteRows = false;
            Actions.AllowUserToResizeRows = false;
            Actions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Actions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Actions.Location = new Point(12, 12);
            Actions.Name = "Actions";
            Actions.RowHeadersVisible = false;
            Actions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Actions.Size = new Size(1211, 611);
            Actions.TabIndex = 0;
            Actions.ColumnHeaderMouseClick += Actions_ColumnHeaderMouseClick;
            Actions.SelectionChanged += Actions_SelectionChanged;
            // 
            // Logs
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1235, 675);
            Controls.Add(Actions);
            Name = "Logs";
            Text = "Logs";
            WindowState = FormWindowState.Maximized;
            Load += Logs_Load;
            ((System.ComponentModel.ISupportInitialize)Actions).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView Actions;
    }
}