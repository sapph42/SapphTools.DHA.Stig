using SapphTools.SecurityDescriptor.Classes;

namespace SapphTools.DHA.Stig.Remediator.Editor.Forms.Common {
    public partial class Disambiguation : Form {
        public Trustee? Selection { get; private set; }
        public Disambiguation(IEnumerable<string> names, string search) {
            InitializeComponent();
            Label.Text = string.Format(Label.Text, search);
            foreach (string name in names) {
                DataGridViewRow row = (DataGridViewRow)Results.RowTemplate.Clone();
                row.Cells["PrincipalDgv"].Value = row.Tag = Trustee.Construct(name);
                row.Cells["AuthorityDgv"].Value = name.Split('\\')[0];
                Results.Rows.Add(row);
            }
        }
        private void Results_SelectionChanged(object sender, EventArgs e) {
            if (Results.SelectedRows.Count > 0) {
                Ok.Enabled = true;
            } else {
                Ok.Enabled = false;
            }
        }
        private void Cancel_Click(object sender, EventArgs e) {
            Selection = null;
            Close();
        }
        private void Ok_Click(object sender, EventArgs e) {
            Selection = (Trustee)Results.SelectedRows.Cast<DataGridViewRow>().First().Tag!;
            Close();
        }
    }
}
