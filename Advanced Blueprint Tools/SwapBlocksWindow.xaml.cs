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
            Update();
        }


        public void Update()
        {
            if (uuidsbackup != BP.GetUsedUuids())
            {
                blockstoreplace = new JArray();
                replacebyblocks = new JArray();
                foreach (string uuid in BP.GetUsedUuids())
                {
                    if (Database.blocks.ContainsKey(uuid))
                    {
                        dynamic blok = new JObject();
                        blok.name = Database.blocks[uuid].Name;
                        blok.bounds = (Database.blocks[uuid] is Part)? ((Part)Database.blocks[uuid]).GetBoundsDynamic() : null;
                        blok.uuid = uuid;
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
            uuidsbackup = BP.GetUsedUuids();

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

                var keys = Database.blocks.Keys;
                try
                {
                    foreach (string uuid in keys)
                    {
                        dynamic bounds = null;
                        if (Database.blocks[uuid] is Part)
                        {
                            bounds = ((Part)Database.blocks[uuid]).GetBoundsDynamic();
                        }

                        //if (bounds == null || (selectedblok.bounds != null && bounds == selectedblok.bounds))
                        {
                            dynamic block = new JObject();
                            block.name = Database.blocks[uuid].Name;
                            block.bounds = bounds;
                            block.uuid = uuid;
                            replacebyblocks.Add(block);
                            comboBox_new.Items.Add(block.name);
                        }

                    }
                }
                catch { }

            }
        }
        private void comboBox_new_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button_set_Click(object sender, RoutedEventArgs e)
        {
            this.window.openpaintpicker();
            textBox_color1.Text = PaintSelector.PaintColor;
        }

        private void button_swap_Click(object sender, RoutedEventArgs e)
        {
            if(comboBox_new.SelectedIndex>-1&& comboBox_old.SelectedIndex > -1)
            {

                dynamic blocktoreplace = blockstoreplace[comboBox_old.SelectedIndex];
                dynamic replacebyblock = replacebyblocks[comboBox_new.SelectedIndex];
                int amountchanged = 0;
                dynamic blueprint = BP.Blueprint;
                foreach (dynamic body in blueprint.bodies)
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
                    BP.setblueprint(blueprint);
                    BP.Description.description = BP.Description.description + "\n" + message;
                    window.RenderBlueprint();
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
