using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Common
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public BaseViewModel()
        {
        }

        //Notify Property Changed Interface
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged( [CallerMemberName] string property = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( property ) );
        }

    }
}
