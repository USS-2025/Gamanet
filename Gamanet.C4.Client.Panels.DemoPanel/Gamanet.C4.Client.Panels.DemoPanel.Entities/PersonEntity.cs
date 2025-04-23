using CommunityToolkit.Mvvm.ComponentModel;
using Gamanet.C4.Client.Utils.Helpers;

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

        public string? Country { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }


    }

}
