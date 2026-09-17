namespace SapphTools.DHA.Stig.Remediator.Forms {
    partial class Main {
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
            Catalog = new DataGridView();
            label2 = new Label();
            Findings = new DataGridView();
            label3 = new Label();
            LoadFindings = new Button();
            Filter = new CheckBox();
            Remediate = new Button();
            RuleCount = new Label();
            FindingsCount = new Label();
            WhatIf = new CheckBox();
            Menu = new MenuStrip();
            remediatorToolStripMenuItem = new ToolStripMenuItem();
            SetCatalogPathMenu = new ToolStripMenuItem();
            EditCatalogMenu = new ToolStripMenuItem();
            LoadFindingsMenu = new ToolStripMenuItem();
            FilterFindingsMenu = new ToolStripMenuItem();
            SimulationModeMenu = new ToolStripMenuItem();
            CatalogPath = new Label();
            toolStripSeparator1 = new ToolStripSeparator();
            RemediateMenu = new ToolStripMenuItem();
            logsAndRollbackToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)Catalog).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Findings).BeginInit();
            Menu.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 40);
            label1.Name = "label1";
            label1.Size = new Size(75, 15);
            label1.TabIndex = 0;
            label1.Text = "Catalog Path";
            // 
            // Catalog
            // 
            Catalog.AllowUserToAddRows = false;
            Catalog.AllowUserToDeleteRows = false;
            Catalog.AllowUserToResizeRows = false;
            Catalog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Catalog.Location = new Point(12, 89);
            Catalog.Name = "Catalog";
            Catalog.RowHeadersVisible = false;
            Catalog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Catalog.Size = new Size(513, 518);
            Catalog.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 71);
            label2.Name = "label2";
            label2.Size = new Size(48, 15);
            label2.TabIndex = 4;
            label2.Text = "Catalog";
            // 
            // Findings
            // 
            Findings.AllowUserToAddRows = false;
            Findings.AllowUserToDeleteRows = false;
            Findings.AllowUserToResizeRows = false;
            Findings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Findings.Location = new Point(576, 89);
            Findings.Name = "Findings";
            Findings.RowHeadersVisible = false;
            Findings.Size = new Size(621, 518);
            Findings.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(576, 71);
            label3.Name = "label3";
            label3.Size = new Size(52, 15);
            label3.TabIndex = 6;
            label3.Text = "Findings";
            // 
            // LoadFindings
            // 
            LoadFindings.Location = new Point(634, 63);
            LoadFindings.Name = "LoadFindings";
            LoadFindings.Size = new Size(75, 23);
            LoadFindings.TabIndex = 7;
            LoadFindings.Text = "Load";
            LoadFindings.UseVisualStyleBackColor = true;
            LoadFindings.Click += LoadFindings_Click;
            // 
            // Filter
            // 
            Filter.AutoSize = true;
            Filter.Location = new Point(715, 63);
            Filter.Name = "Filter";
            Filter.Size = new Size(302, 19);
            Filter.TabIndex = 9;
            Filter.Text = "Filter findings without a configured remediation rule";
            Filter.UseVisualStyleBackColor = true;
            Filter.CheckedChanged += Filter_CheckedChanged;
            // 
            // Remediate
            // 
            Remediate.Location = new Point(1029, 619);
            Remediate.Name = "Remediate";
            Remediate.Size = new Size(168, 31);
            Remediate.TabIndex = 10;
            Remediate.Text = "Remediate Selected Entries";
            Remediate.UseVisualStyleBackColor = true;
            Remediate.Click += Remediate_Click;
            // 
            // RuleCount
            // 
            RuleCount.AutoSize = true;
            RuleCount.Location = new Point(12, 619);
            RuleCount.Name = "RuleCount";
            RuleCount.Size = new Size(247, 15);
            RuleCount.TabIndex = 11;
            RuleCount.Text = "Catalog not loaded. Remediation unavailable.";
            // 
            // FindingsCount
            // 
            FindingsCount.AutoSize = true;
            FindingsCount.Location = new Point(576, 619);
            FindingsCount.Name = "FindingsCount";
            FindingsCount.Size = new Size(251, 15);
            FindingsCount.TabIndex = 12;
            FindingsCount.Text = "Findings not loaded. Remediation unavailable.";
            // 
            // WhatIf
            // 
            WhatIf.AutoSize = true;
            WhatIf.Checked = true;
            WhatIf.CheckState = CheckState.Checked;
            WhatIf.Location = new Point(940, 626);
            WhatIf.Name = "WhatIf";
            WhatIf.Size = new Size(64, 19);
            WhatIf.TabIndex = 13;
            WhatIf.Text = "What If";
            WhatIf.UseVisualStyleBackColor = true;
            WhatIf.CheckedChanged += WhatIf_CheckedChanged;
            // 
            // Menu
            // 
            Menu.Items.AddRange(new ToolStripItem[] { remediatorToolStripMenuItem });
            Menu.Location = new Point(0, 0);
            Menu.Name = "Menu";
            Menu.Size = new Size(1209, 24);
            Menu.TabIndex = 14;
            Menu.Text = "menuStrip1";
            // 
            // remediatorToolStripMenuItem
            // 
            remediatorToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { SetCatalogPathMenu, EditCatalogMenu, LoadFindingsMenu, FilterFindingsMenu, SimulationModeMenu, toolStripSeparator1, RemediateMenu, logsAndRollbackToolStripMenuItem });
            remediatorToolStripMenuItem.Name = "remediatorToolStripMenuItem";
            remediatorToolStripMenuItem.Size = new Size(80, 20);
            remediatorToolStripMenuItem.Text = "Remediator";
            // 
            // SetCatalogPathMenu
            // 
            SetCatalogPathMenu.Name = "SetCatalogPathMenu";
            SetCatalogPathMenu.Size = new Size(214, 22);
            SetCatalogPathMenu.Text = "Set Catalog Path";
            SetCatalogPathMenu.Click += SetCatalogPathMenu_Click;
            // 
            // EditCatalogMenu
            // 
            EditCatalogMenu.Name = "EditCatalogMenu";
            EditCatalogMenu.Size = new Size(214, 22);
            EditCatalogMenu.Text = "Edit Catalog";
            EditCatalogMenu.Click += EditCatalogMenu_Click;
            // 
            // LoadFindingsMenu
            // 
            LoadFindingsMenu.Name = "LoadFindingsMenu";
            LoadFindingsMenu.Size = new Size(214, 22);
            LoadFindingsMenu.Text = "Load Findings";
            LoadFindingsMenu.Click += LoadFindings_Click;
            // 
            // FilterFindingsMenu
            // 
            FilterFindingsMenu.CheckOnClick = true;
            FilterFindingsMenu.Name = "FilterFindingsMenu";
            FilterFindingsMenu.Size = new Size(214, 22);
            FilterFindingsMenu.Text = "Filter Findings";
            FilterFindingsMenu.Click += FilterFindingsMenu_Click;
            // 
            // SimulationModeMenu
            // 
            SimulationModeMenu.Checked = true;
            SimulationModeMenu.CheckOnClick = true;
            SimulationModeMenu.CheckState = CheckState.Checked;
            SimulationModeMenu.Name = "SimulationModeMenu";
            SimulationModeMenu.Size = new Size(214, 22);
            SimulationModeMenu.Text = "Simulation Mode (What If)";
            SimulationModeMenu.Click += SimulationModeMenu_Click;
            // 
            // CatalogPath
            // 
            CatalogPath.AutoSize = true;
            CatalogPath.Location = new Point(103, 40);
            CatalogPath.Name = "CatalogPath";
            CatalogPath.Size = new Size(0, 15);
            CatalogPath.TabIndex = 15;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(211, 6);
            // 
            // RemediateMenu
            // 
            RemediateMenu.Name = "RemediateMenu";
            RemediateMenu.Size = new Size(214, 22);
            RemediateMenu.Text = "Remediate";
            RemediateMenu.Click += Remediate_Click;
            // 
            // logsAndRollbackToolStripMenuItem
            // 
            logsAndRollbackToolStripMenuItem.Name = "logsAndRollbackToolStripMenuItem";
            logsAndRollbackToolStripMenuItem.Size = new Size(214, 22);
            logsAndRollbackToolStripMenuItem.Text = "Logs and Rollback";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1209, 680);
            Controls.Add(CatalogPath);
            Controls.Add(WhatIf);
            Controls.Add(FindingsCount);
            Controls.Add(RuleCount);
            Controls.Add(Remediate);
            Controls.Add(Filter);
            Controls.Add(LoadFindings);
            Controls.Add(label3);
            Controls.Add(Findings);
            Controls.Add(label2);
            Controls.Add(Catalog);
            Controls.Add(label1);
            Controls.Add(Menu);
            MainMenuStrip = Menu;
            Name = "Main";
            Text = "STIG Remediator";
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)Catalog).EndInit();
            ((System.ComponentModel.ISupportInitialize)Findings).EndInit();
            Menu.ResumeLayout(false);
            Menu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private DataGridView Catalog;
        private DataGridView Findings;
        private Label label3;
        private Button LoadFindings;
        private Label label2;
        private CheckBox Filter;
        private Button Remediate;
        private Label RuleCount;
        private Label FindingsCount;
        private CheckBox WhatIf;
        private MenuStrip Menu;
        private ToolStripMenuItem remediatorToolStripMenuItem;
        private ToolStripMenuItem SetCatalogPathMenu;
        private ToolStripMenuItem EditCatalogMenu;
        private Label CatalogPath;
        private ToolStripMenuItem LoadFindingsMenu;
        private ToolStripMenuItem FilterFindingsMenu;
        private ToolStripMenuItem SimulationModeMenu;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem RemediateMenu;
        private ToolStripMenuItem logsAndRollbackToolStripMenuItem;
    }
}