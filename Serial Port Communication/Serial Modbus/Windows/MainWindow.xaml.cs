using System.Windows;

namespace Serial_Modbus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel vm = new MainViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            this.vm.CloseWindow += delegate { this.OnClose(); };
            this.DataContext = this.vm;
        }

        public void OnClose()
        {
            if ( this.vm.mode == 'M' )
            {
                var window = new MasterWindow();
                window.Show();
            }
            else
            {
                var window = new SlaveWindow();
                window.Show();
            }

            this.Close();
        }
    }
}
