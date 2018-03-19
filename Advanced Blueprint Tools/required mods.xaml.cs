using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for required_mods.xaml
    /// </summary>
    public partial class required_mods : Window
    {
        private dynamic Mods;
        public required_mods(dynamic Mods)
        {

            this.Mods = Mods.mods;
            InitializeComponent();
            foreach (dynamic mod in this.Mods)
            {
                listBox_mods.Items.Add(mod);
            }
            listBox_mods.MouseDoubleClick += new MouseButtonEventHandler(Mod_Click);
            listBox_mods.SelectionChanged += new SelectionChangedEventHandler(Mod_Click);
            
        }
        

        private void Mod_Click(object sender, RoutedEventArgs e)
        {
            ListBox b = (ListBox)sender;
            dynamic mod = (b.SelectedItem);
            MessageBoxResult messageBoxResult = MessageBox.Show
                ("Name: "+mod.name+"\nAuthor: "+mod.author+"\nUrl: "+mod.url+"\n\nWould you like to go to this mod page?", "Selected Mod     -provided by http://scrapmechanic.xesau.eu/uuidservice/", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(@"steam://openurl/"+mod.url);
            }

        }
        
    }
}
