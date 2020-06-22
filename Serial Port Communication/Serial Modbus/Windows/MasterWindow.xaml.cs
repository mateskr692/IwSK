using System.Windows;

namespace Serial_Modbus
{
    /// <summary>
    /// Interaction logic for MasterWindow.xaml
    /// </summary>
    public partial class MasterWindow : Window
    {
        MasterViewModel vm = new MasterViewModel();

        public MasterWindow()
        {
            this.InitializeComponent();
            this.DataContext = this.vm;
        }


    }
}
