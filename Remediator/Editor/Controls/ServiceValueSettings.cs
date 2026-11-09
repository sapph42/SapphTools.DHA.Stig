namespace SapphTools.DHA.Stig.Remediator.Editor.Controls;
public partial class ServiceValueSettings : ValueSettings, IValueSettings<ServiceValue> {
    public override bool IsValid => Subtype.SelectedItem is ServiceType type 
        && type != ServiceType.NoAction;
    public override ServiceValue? Value {
        get {
            if (!IsValid) {
                return null;
            }
            return new() {
                ActionSubtype = (ServiceType)Subtype.SelectedItem!
            };
        }
    }
    public ServiceValueSettings() {
        InitializeComponent();
        foreach (ServiceType type in Enum.GetValues(typeof(ServiceType))) {
            Subtype.Items.Add(type);
        }
        Subtype.Format += (_, e) => {
            if (e.ListItem is ServiceType type) {
                e.Value = type.ToString();
            }
        };
    }
    public ServiceValueSettings(ServiceValue? value) : this() {
        if (value is not null) {
            Subtype.SelectedItem = value.ActionSubtype;
        }
    }
    public ServiceValueSettings(Setting? setting) : this() {
        if (setting is not null && setting.Data is ServiceValue value) {
            Subtype.SelectedItem = value.ActionSubtype;
        }
    }
}