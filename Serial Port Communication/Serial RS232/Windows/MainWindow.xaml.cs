using System.Windows;

namespace Serial_RS232
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
            this.DataContext = this.vm;
        }
    }
}
