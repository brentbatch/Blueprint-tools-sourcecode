using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for Loadwindow.xaml
    /// </summary>
    public partial class Loadwindow : Window
    {
        MainWindow w;
        public Loadwindow(MainWindow window)
        {
            InitializeComponent();



            w = window;

            Thread t = new Thread(new ThreadStart(checkifloaded));
            t.Start();
        }
        
        public void checkifloaded()
        {
            while(true)
            {
                MainWindow win=null;
                this.Dispatcher.Invoke((Action)(() =>
                {//this refer to form in WPF application 
                    win = w;
                }));
                if (win.IsLoaded)
                    this.Close();
            }
        }
    }
}
