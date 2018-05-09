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
        MainWindow mainWindow;
        public Settings(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            color_wire.Text = Properties.Settings.Default.wirecolor;
            color_blob.Text = Properties.Settings.Default.blobcolor;
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
            WiresOpacity.Text = Properties.Settings.Default.coloropacity.ToString();
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

        private void color_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox_color = (TextBox)sender;
            if (textbox_color.Text.Length == 7)
            {
                try
                {
                    var bc = new BrushConverter();
                    string color = "#FF" + textbox_color.Text.Substring(1);
                    textbox_color.Background = (Brush)bc.ConvertFrom(color);
                }
                catch
                {
                    //MessageBox.Show("Please use the right format\n \"#123abc\" where 1-9,a-f (hex)");
                    textbox_color.Text = "";
                }
            }
            else
            {
                var bc = new BrushConverter();
                textbox_color.Text = "#2CE6E6";
                textbox_color.Background = (Brush)bc.ConvertFrom("#2CE6E6");
            }
        }

        private void color_SET_Click(object sender, RoutedEventArgs e)
        {

            this.mainWindow.openpaintpicker();
            color_wire.Text = PaintSelector.PaintColor;
            Properties.Settings.Default.wirecolor = color_wire.Text;
        }

        private void color_SET2_Click(object sender, RoutedEventArgs e)
        {

            this.mainWindow.openpaintpicker();
            color_blob.Text = PaintSelector.PaintColor;
            Properties.Settings.Default.blobcolor = color_blob.Text;
        }

        private void Opacity_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            try
            {
                if (Convert.ToInt32(t.Text) < 0) t.Text = "0";
                if (Convert.ToInt32(t.Text) >255) t.Text = "255";
            }
            catch
            {
                t.Text = "0";
                t.Select(0, 1);
            }
            Properties.Settings.Default.coloropacity = Convert.ToByte(t.Text);
        }
    }
}
