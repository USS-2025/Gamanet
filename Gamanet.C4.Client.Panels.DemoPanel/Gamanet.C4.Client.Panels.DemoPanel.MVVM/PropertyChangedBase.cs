using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gamanet.C4.Client.Panels.DemoPanel.MVVM
{
    public class PropertyChangedBase: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if(string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
