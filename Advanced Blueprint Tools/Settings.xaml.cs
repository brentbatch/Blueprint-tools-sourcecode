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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            Update();
        }
        void Update()
        {
            Button_Branch.Content = "Branch: \t\t" + Properties.Settings.Default.branch.ToString();
            Button_SafeMode.Content = "Safe mode: \t" + (Properties.Settings.Default.safemode ? "ON" : "OFF");
            Button_Debug.Content = "Debug Mode: \tNot Available";
            Text_Path.Text = Properties.Settings.Default.steamapps;
            Text_Version.Content = "Version: \tV" + Properties.Settings.Default.version.ToString();
            Button_wires.Content = "Wires: \t\t" + (Properties.Settings.Default.wires == true ? "ON" : "OFF");
            Button_colorwires.Content = "ColorWires:\t" + (Properties.Settings.Default.colorwires == true ? "ON" : "OFF");
        }

        private void Button_Branch_Click(object sender, RoutedEventArgs e)
        {
            if(Properties.Settings.Default.branch.ToString() == "Test")
            {
                Properties.Settings.Default.branch = "Public";
            }
            else
            {
                Properties.Settings.Default.branch = "Test";
            }
            Update();
        }

        private void Button_SafeMode_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.safemode = !Properties.Settings.Default.safemode;

            //MessageBox.Show("this doesn't affect anything yet\nWill allow you to swap any blocks with any blocks, can possibly brick your blueprint");
            
            Update();
        }

        private void Button_Debug_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Text_Path_TextChanged(object sender, TextChangedEventArgs e)
        {
            var bc = new BrushConverter();
            if (System.IO.Directory.Exists(Text_Path.Text))
            {
                Text_Path.Background = (Brush)bc.ConvertFrom("#61937C");
                Properties.Settings.Default.steamapps = Text_Path.Text;
            }
            else
            {
                Text_Path.Background = (Brush)bc.ConvertFrom("#81737C");
            }
        }

        private void Button_update_Click(object sender, RoutedEventArgs e)
        {
            if(new Updater().CheckForUpdates())
            {
                MessageBox.Show("updates available!");
            }
            else
            {
                MessageBox.Show("up to date!");
            }
        }
        private void Button_wires_Click(object sender, RoutedEventArgs e)
        {

            Properties.Settings.Default.wires = !Properties.Settings.Default.wires;
            Update();
        }

        private void Save_settings_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists(Text_Path.Text))
            {
                var bc = new BrushConverter();
                Text_Path.Background = (Brush)bc.ConvertFrom("#ff0000");

            }
            Properties.Settings.Default.Save();
            this.Close();
            MessageBox.Show("Settings saved!");

        }

        private void Button_colorwires_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.colorwires = !Properties.Settings.Default.colorwires;
            Update();
        }
    }
}
