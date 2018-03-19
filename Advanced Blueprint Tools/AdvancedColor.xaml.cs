using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for AdvancedColor.xaml
    /// </summary>
    public partial class AdvancedColor : Window
    {
        MainWindow window;
        List<Item> ItemList;
        dynamic uuidsbackup;

        public AdvancedColor(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            this.update();
        }


        public void update()
        {
            ItemList = new List<Item>();
            ItemList.Add(new Item("any", "*"));
            if (uuidsbackup != this.window.OpenedBlueprint.useduuids)
            {
                foreach (string uuid in this.window.OpenedBlueprint.useduuids)
                {
                    if (this.window.getgameblocks()[uuid] != null)
                    {

                        dynamic part = this.window.getgameblocks()[uuid];
                        ItemList.Add(new Item(part.Name.ToString(), part.uuid.ToString()));
                    }
                }

                this.Dispatcher.Invoke((Action)(() =>
                {//this refer to form in WPF application 
                    comboBox_old.Items.Clear();
                }));
                foreach (Item item in ItemList)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                        comboBox_old.Items.Add(item.Name);
                    }));
                }
            }
            uuidsbackup = this.window.OpenedBlueprint.useduuids;
        }



        private void textBox_color_TextChanged(object sender, TextChangedEventArgs e)
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
                    MessageBox.Show("Please use the right format\n \"#123abc\" where 1-9,a-f (hex)");
                }
            }
            else
            {
                var bc = new BrushConverter();
                textbox_color.Background = (Brush)bc.ConvertFrom("#eeeeee");
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            window.openpaintpicker();
            textBox_color1.Text = this.window.PaintColor;
        }

        private void button3_Copy_Click(object sender, RoutedEventArgs e)
        {
            window.openpaintpicker();
            textBox_color2.Text = this.window.PaintColor;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Watch this video on how to use: \nhttps://www.youtube.com/c/brentbatch", "Tutorial?", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
            if (messageBoxResult.ToString() == "Yes")
            { System.Diagnostics.Process.Start("https://www.youtube.com/c/brentbatch"); }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            TextBox t1 = (TextBox)textBox_color1;
            Brush b = t1.Background;
            string oldcolor = "#" + (b as SolidColorBrush).Color.ToString().Substring(3);

            TextBox t2 = (TextBox)textBox_color2;
            Brush b1 = t2.Background;
            string newcolor = "#" + (b1 as SolidColorBrush).Color.ToString().Substring(3);

            int amountcolored = 0;
            foreach (dynamic body in window.OpenedBlueprint.blueprint.bodies)
            {
                foreach (dynamic child in body.childs)
                {
                    string token = "#";
                    string color = child.color.ToString();
                    if (color[0] == '#') token = "";
                    if ((token + child.color.ToString().ToLower() == oldcolor.ToLower() || textBox_color1.Text == "#" || textBox_color1.Text == "") && ((comboBox_old.SelectedIndex <= 0) || child.shapeId.ToString().ToLower() == ItemList[comboBox_old.SelectedIndex].UUID.ToLower()))
                    {
                        amountcolored++;
                        child.color = newcolor;
                    }
                }
            }
            if (comboBox_old.SelectedIndex < 0) comboBox_old.SelectedIndex = 0;
            string message = "++ " + amountcolored + " " + oldcolor + " " + ItemList[comboBox_old.SelectedIndex].Name + " are now painted " + newcolor + " color";
            if (textBox_color1.Text == "#" || textBox_color1.Text == "")
                message = "++ " + amountcolored + ItemList[comboBox_old.SelectedIndex].Name + " are now painted " + newcolor + " color";
            MessageBox.Show(message);
            if (amountcolored > 0)
            {
                window.OpenedBlueprint.setblueprint(window.OpenedBlueprint.blueprint);

                window.OpenedBlueprint.description.description = window.OpenedBlueprint.description.description + "\n" + message;
                window.UpdateOpenedBlueprint();
            }


        }
    }

    public class Item
    {
        public string Name { get; set; }
        public string UUID { get; set; }

        public Item(string Name, string UUID)
        {
            this.Name = Name;
            this.UUID = UUID;
        }

    }
}
