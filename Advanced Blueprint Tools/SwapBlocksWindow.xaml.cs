using Newtonsoft.Json.Linq;
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

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for SwapBlocksWindow.xaml
    /// </summary>
    public partial class SwapBlocksWindow : Window
    {
        MainWindow window;
        private dynamic uuidsbackup;
        private dynamic blockstoreplace;
        private dynamic replacebyblocks;
        public SwapBlocksWindow(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            update();
        }


        public void update()
        {
            if (uuidsbackup != this.window.OpenedBlueprint.useduuids)
            {
                blockstoreplace = new JArray();
                replacebyblocks = new JArray();
                foreach (string uuid in this.window.OpenedBlueprint.useduuids)
                {
                    if (this.window.getgameblocks()[uuid] != null)
                    {

                        dynamic part = this.window.getgameblocks()[uuid];
                        dynamic blok = new JObject();
                        blok.name = part.Name;
                        blok.bounds = getbounds(part);
                        blok.uuid = part.uuid;
                        blockstoreplace.Add(blok);
                    }
                }
                this.Dispatcher.Invoke((Action)(() =>
                {//this refer to form in WPF application 
                    comboBox_old.Items.Clear();
                    comboBox_new.Items.Clear();
                }));
                foreach (dynamic blok in blockstoreplace)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                        comboBox_old.Items.Add(blok.name);
                    }));
                }
            }
            uuidsbackup = this.window.OpenedBlueprint.useduuids;

        }

        private void textBox_color1_TextChanged(object sender, TextChangedEventArgs e)
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

        private void comboBox_old_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            replacebyblocks = new JArray();
            comboBox_new.Items.Clear();
            if(comboBox_old.SelectedIndex>=0)
            {

                dynamic selectedblok = blockstoreplace[comboBox_old.SelectedIndex];
                dynamic blocklist = this.window.getgameblocks();
                foreach (dynamic bindex in blocklist)
                {
                    dynamic part = blocklist[bindex.Name];
                    dynamic bounds = getbounds(part);
                    if ((selectedblok.bounds != null && bounds != null && bounds.x == selectedblok.bounds.x && bounds.y == selectedblok.bounds.y && bounds.z == selectedblok.bounds.z) || bounds == null)
                    {
                        dynamic b = new JObject();
                        b.name = part.Name;
                        b.bounds = getbounds(part);
                        b.uuid = part.uuid;
                        replacebyblocks.Add(b);
                        comboBox_new.Items.Add(part.Name);
                    }
                }

            }
        }

        private void button_set_Click(object sender, RoutedEventArgs e)
        {
            textBox_color1.Text = this.window.PaintColor;
        }

        private void button_swap_Click(object sender, RoutedEventArgs e)
        {
            if(comboBox_new.SelectedIndex>-1&& comboBox_old.SelectedIndex > -1)
            {

                dynamic blocktoreplace = blockstoreplace[comboBox_old.SelectedIndex];
                dynamic replacebyblock = replacebyblocks[comboBox_new.SelectedIndex];
                int amountchanged = 0;
                foreach (dynamic body in this.window.OpenedBlueprint.blueprint.bodies)
                    foreach (dynamic child in body.childs)
                    {
                        if (child.shapeId == blocktoreplace.uuid && (textBox_color1.Text.ToString().ToLower() == child.color.ToString().ToLower() || textBox_color1.Text.ToString().ToLower() == "#"+child.color.ToString().ToLower() || textBox_color1.Text == "" || textBox_color1.Text =="#"))
                        {
                            if (child.bounds == null)
                            {
                                child.bounds = blocktoreplace.bounds;
                            }
                            child.shapeId = replacebyblock.uuid;
                            amountchanged++;
                        }
                    }

                string message = "++ " + amountchanged + " " + blocktoreplace.name + " are now changed to " + replacebyblock.name;
                MessageBox.Show(message);
                if (amountchanged > 0)
                {
                    window.OpenedBlueprint.setblueprint(window.OpenedBlueprint.blueprint);

                    window.OpenedBlueprint.description.description = window.OpenedBlueprint.description.description + "\n" + message;
                    window.UpdateOpenedBlueprint();
                }

            }
        }

        //get bounds from 
        private dynamic getbounds(dynamic part)
        {
            dynamic bounds = null;
            if (part.box != null)
            {
                return part.box;
            }
            if (part.hull != null)
            {
                bounds = new JObject();
                bounds.x = part.hull.x;
                bounds.y = part.hull.y;
                bounds.z = part.hull.z;
                return bounds;
            }
            if (part.cylinder != null)
            {
                if (part.cylinder.axis.ToString().ToLower() == "x")
                {
                    bounds = new JObject();
                    bounds.x = part.cylinder.depth;
                    bounds.y = part.cylinder.diameter * 2;
                    bounds.z = part.cylinder.diameter * 2;
                    return bounds;

                }
                else
                if (part.cylinder.axis.ToString().ToLower() == "y")
                {
                    bounds = new JObject();
                    bounds.x = part.cylinder.diameter * 2;
                    bounds.y = part.cylinder.depth;
                    bounds.z = part.cylinder.diameter * 2;
                    return bounds;

                }
                else
                if (part.cylinder.axis.ToString().ToLower() == "z")
                {
                    bounds = new JObject();
                    bounds.x = part.cylinder.diameter * 2;
                    bounds.y = part.cylinder.diameter * 2;
                    bounds.z = part.cylinder.depth;
                    return bounds;

                }
            }
            return bounds;
        }
    }
}
