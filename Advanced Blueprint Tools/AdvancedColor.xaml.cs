using Newtonsoft.Json.Linq;
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
            this.Update();
        }


        public void Update()
        {
            if (uuidsbackup != BP.GetUsedUuids())
            {
                ItemList = new List<Item>
                {
                    new Item("any", "*")
                };
                foreach (string uuid in BP.GetUsedUuids())
                {
                    if (Database.blocks.ContainsKey(uuid))
                    {
                        ItemList.Add(new Item(Database.blocks[uuid].Name, uuid));
                    }
                }
                dynamic colorlist = new JObject();
                foreach (dynamic body in BP.Blueprint.bodies)
                    foreach(dynamic child in body.childs)
                    {
                        if (child.color.ToString().StartsWith("#")) child.color = child.color.ToString().Substring(1);
                        if (colorlist[child.color.ToString().ToLower()] == null)
                        {
                            colorlist[child.color.ToString().ToLower()] = true;
                            string c = child.color.ToString();

                            Button b = new Button();
                            var bc = new BrushConverter();
                            b.Background = (Brush)bc.ConvertFrom("#"+c);
                            color_list.Items.Add(b);
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
                        comboBox_old.Items.Add(item);
                    }));
                }
            }
            uuidsbackup = BP.GetUsedUuids(); 
        }



        private void TextBox_color_TextChanged(object sender, TextChangedEventArgs e)
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

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.openpaintpicker();
            textBox_color1.Text = PaintSelector.PaintColor;
        }

        private void Button3_Copy_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.openpaintpicker();
            textBox_color2.Text = PaintSelector.PaintColor;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://youtu.be/glLgQemUS2I?t=677");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox t1 = (TextBox)textBox_color1;
            Brush b = t1.Background;
            string oldcolor = "#" + (b as SolidColorBrush).Color.ToString().Substring(3);

            TextBox t2 = (TextBox)textBox_color2;
            Brush b1 = t2.Background;
            string newcolor = "#" + (b1 as SolidColorBrush).Color.ToString().Substring(3);

            int amountcolored = 0;
            foreach (dynamic body in BP.Blueprint.bodies)
            {
                foreach (dynamic child in body.childs)
                {
                    string token = "#";
                    string color = child.color.ToString();
                    if (color[0] == '#') token = "";
                    if ((token + child.color.ToString().ToLower() == oldcolor.ToLower() || textBox_color1.Text == "#" || textBox_color1.Text == "") && ((comboBox_old.SelectedIndex <= 0) || child.shapeId.ToString() == ((Item)comboBox_old.SelectedItem).UUID.ToLower()))
                    {
                        amountcolored++;
                        child.color = newcolor;
                    }
                }
            }
            if(BP.Blueprint.joints != null)
                foreach (dynamic child in BP.Blueprint.joints)
                {
                    string token = "#";
                    string color = child.color.ToString();
                    if (color[0] == '#') token = "";
                    if ((token + child.color.ToString().ToLower() == oldcolor.ToLower() || textBox_color1.Text == "#" || textBox_color1.Text == "") && ((comboBox_old.SelectedIndex <= 0) || child.shapeId.ToString() == ((Item)comboBox_old.SelectedItem).UUID.ToLower()))
                    {
                        amountcolored++;
                        child.color = newcolor.Substring(1); 
                    }
                }
            if (comboBox_old.SelectedIndex < 0) comboBox_old.SelectedIndex = 0;
            string message = "++ " + amountcolored + " " + oldcolor + " " + ((Item)comboBox_old.SelectedItem).Name + " are now painted " + newcolor + " color";
            if (textBox_color1.Text == "#" || textBox_color1.Text == "")
                message = "++ " + amountcolored + ((Item)comboBox_old.SelectedItem).Name + " are now painted " + newcolor + " color";
            
            new System.Threading.Thread(new System.Threading.ThreadStart(() => { MessageBox.Show(message); })).Start();

            if (amountcolored > 0)
            {
                BP.setblueprint(BP.Blueprint);

                BP.Description.description = BP.Description.description + "\n" + message;
                window.RenderBlueprint();
            }


        }

        private void Color_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.openpaintpicker();
            PaintSelector.PaintColor = "#" + ((string)((dynamic)color_list.SelectedItem).Background.ToString()).Substring(3,6);
            if (MainWindow.paintSelector.IsLoaded)
                MainWindow.paintSelector.textbox_color.Text = PaintSelector.PaintColor;
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

        public override string ToString()
        {
            return Name;
        }
    }
}
