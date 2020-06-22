using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Serial_Modbus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Window _currentwindow { get; set; }

        protected override void OnStartup( StartupEventArgs e )
        {
            base.OnStartup( e );
            var window = new MainWindow();
            window.Show();
        }
    }
}
