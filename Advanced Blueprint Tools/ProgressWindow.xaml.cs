using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        BackgroundWorker BackgroundWorker;
        public ProgressWindow(BackgroundWorker backgroundworker, string doing )
        {
            this.BackgroundWorker = backgroundworker;
            InitializeComponent();
            doing_label.Content = doing;
        }

        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (BackgroundWorker != null)
                BackgroundWorker.CancelAsync();
        }

        public void UpdateProgress(int currentprogress)
        {
            progressbar.Value = currentprogress;
        }
    }
}
