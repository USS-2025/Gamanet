using Gamanet.C4.Client.Panels.DemoPanel.MVVM;

namespace Gamanet.C4.Client.Panels.DemoPanel.Entities
{
    public class PersonEntity : PropertyChangedBase
    {
        private string? _name;

        public string? Name
        {
            get => _name;
            set
            {
                if (value == _name)
                {
                    return;
                }

                this._name = value;
                OnPropertyChanged();
            }
        }

        // ToDo: If this entity type editable, add all the other OnPropertyChanged()
        // (and add full getter and setter before)

        public string? Country { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public override string ToString() => $"{this.Name} ({this.Country})";
    }

}
