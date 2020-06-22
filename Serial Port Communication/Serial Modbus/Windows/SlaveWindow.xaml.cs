using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Serial_Modbus
{
    /// <summary>
    /// Interaction logic for SlaveWindow.xaml
    /// </summary>
    public partial class SlaveWindow : Window
    {
        SlaveViewModel vm = new SlaveViewModel();

        public SlaveWindow()
        {
            this.InitializeComponent();
            this.DataContext = this.vm;
        }
    }
}
