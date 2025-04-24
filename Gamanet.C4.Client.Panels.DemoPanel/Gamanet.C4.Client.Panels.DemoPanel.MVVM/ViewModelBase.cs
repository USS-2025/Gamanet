using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Interfaces;
using System.Diagnostics;

namespace Gamanet.C4.Client.Panels.DemoPanel.MVVM
{
    public class ViewModelBase : PropertyChangedBase
                                , IViewModel
    {
        private static int _instanceCounter;

        public int ID { get; }

        #region Some basic properties needed by many/most view models

        private string? _errorText;

        public string? ErrorText
        {
            get { return _errorText; }
            set
            {
                if (value == _errorText)
                {
                    return;
                }

                _errorText = value;
                OnPropertyChanged();
                // Don't forget to update according error flag
                OnPropertyChanged(nameof(this.HasErrors));
            }
        }

        public bool HasErrors => !string.IsNullOrWhiteSpace(ErrorText);

        private string? _statusText;

        public string? StatusText
        {
            get { return _statusText; }
            set
            {
                if (value == _statusText)
                {
                    return;
                }

                _statusText = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public ViewModelBase()
        {
            // atomic increment to ensure thread safety
            this.ID = Interlocked.Increment(ref _instanceCounter);

            this.StatusText = $"{GetType().Name} {ID} instantiated.";
            Trace.TraceInformation(this.StatusText);
        }

        public override string ToString() => $"{GetType().Name} {ID}";
    }
}
