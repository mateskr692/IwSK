using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Serial_Modbus
{
    public class MainViewModel : BaseViewModel
    {
        public delegate void WindowDelegate();
        public event WindowDelegate CloseWindow;
        protected void RaiseCloseEvent( object parameter = null ) => this.CloseWindow();
        public char mode = 'M';

        public MainViewModel()
        {
            this.MasterCheckedCommand = new RelayCommand( this.OnMasterCheckedCommand );
            this.SlaveCheckedCommand = new RelayCommand( this.OnSlaveCheckedCommand );
            this.SelectCommand = new RelayCommand( this.OnSelectCommand );
        }


        public RelayCommand MasterCheckedCommand { get; private set; }
        public void OnMasterCheckedCommand( object o ) => this.mode = 'M';

        public RelayCommand SlaveCheckedCommand { get; private set; }
        public void OnSlaveCheckedCommand( object o ) => this.mode = 'S';

        public RelayCommand SelectCommand { get; private set; }
        public void OnSelectCommand( object o ) => this.RaiseCloseEvent();

    }
}
