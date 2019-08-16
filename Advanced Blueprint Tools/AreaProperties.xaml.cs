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
using static Advanced_Blueprint_Tools.Part;

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for AreaProperties.xaml
    /// </summary>
    public partial class AreaProperties : Window
    {
        MainWindow mainwindow; 
        HashSet<string> uuidsbackup = new HashSet<string>();
        public AreaProperties(MainWindow mainwindow)
        {
            this.mainwindow = mainwindow;
            InitializeComponent();
            Loadwindow w = new Loadwindow();
            w.Show();
            Update();

            w.Close();
        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/HXFqUqF"); 
        }

        private void Filter_pos_TextChanged(object sender, RoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            try
            {
                if (Convert.ToInt32(t.Text) >= 0) { }
            }
            catch
            {
                if (t.Text == "-")
                {
                    t.Text = "-0";
                    t.Select(1, 1);
                }
                else
                {
                    t.Text = "0";
                    t.Select(0, 1);
                }
            }
            if (this.IsLoaded)
            {
                int x1 = Convert.ToInt32(filter_x1.Text);
                int y1 = Convert.ToInt32(filter_y1.Text);
                int z1 = Convert.ToInt32(filter_z1.Text);
                int x2 = Convert.ToInt32(filter_x2.Text);
                int y2 = Convert.ToInt32(filter_y2.Text);
                int z2 = Convert.ToInt32(filter_z2.Text);
                this.mainwindow.setMarker2((x1 + x2 + 0.0f) / 2, (y1 + y2 + 0.0f) / 2, (z1 + z2 + 0.0f) / 2, (x2 - x1), (y2 - y1), (z2 - z1));
            }
        }
        private void filter_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
                filterupdate();
        }
        private void Filter_SET_Click(object sender, RoutedEventArgs e)
        {
            this.mainwindow.openpaintpicker();
            filter_color.Text = PaintSelector.PaintColor;
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
                    if (textbox_color.Name == "filter_color") filtercolor = textbox_color.Text;
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
                textbox_color.Background = (Brush)bc.ConvertFrom("#eeeeee");
                if (textbox_color.Name == "filter_color") filtercolor = null;
            }
            if (textbox_color.Name == "filter_color")
                if (this.IsLoaded)
                    filterupdate();
        }

        public void Update()//new bp, items replaced, ... whatever
        {
            disableAll();
            if (filter_x1.Text == "" || uuidsbackup != BP.GetUsedUuids())
            {
                dynamic bounds = BP.GetBounds();
                filter_x1.Text = bounds.minx.ToString();
                filter_y1.Text = bounds.miny.ToString();
                filter_z1.Text = bounds.minz.ToString();
                filter_x2.Text = bounds.maxx.ToString();
                filter_y2.Text = bounds.maxy.ToString();
                filter_z2.Text = bounds.maxz.ToString();
            }
            if (uuidsbackup != BP.GetUsedUuids() | true)//fill the combobox once
            {
                filter_type.Items.Clear();
                filter_type.Items.Add(new Item("any","*"));
                foreach (string uuid in BP.GetUsedUuids())
                {
                    if (Database.blocks.ContainsKey(uuid))
                        filter_type.Items.Add(new Item(Database.blocks[uuid].Name.ToString(), uuid));
                }
                uuidsbackup.Clear();
                uuidsbackup.UnionWith(BP.GetUsedUuids());
                filter_type.SelectedIndex = 0;
            }
            filterupdate();
        }

        private string filtercolor = null;
        private void filterupdate() // enable specific boxes
        {
            disableAll();
            if (filter_type.SelectedIndex>0)
            {
                string uuid = ((Item)filter_type.SelectedItem).UUID.ToString();
                if(Database.blocks.ContainsKey(uuid) && Database.blocks[uuid] is Part)
                {
                    properties property = ((Part)Database.blocks[uuid]).GetProperty();
                    if (property == properties.sensor)
                        Edit_sensor.Visibility = Visibility.Visible;
                    if (property == properties.spotlight)
                        Edit_lamp.Visibility = Visibility.Visible;
                    if (property == properties.logic)
                        Edit_gate.Visibility = Visibility.Visible;
                    if (property == properties.timer)
                        Edit_Timer.Visibility = Visibility.Visible;
                }

            }
            Edit_general.Visibility = Visibility.Visible;
            int x1 = Convert.ToInt32(filter_x1.Text);
            int y1 = Convert.ToInt32(filter_y1.Text);
            int z1 = Convert.ToInt32(filter_z1.Text);
            int x2 = Convert.ToInt32(filter_x2.Text);
            int y2 = Convert.ToInt32(filter_y2.Text);
            int z2 = Convert.ToInt32(filter_z2.Text);
            this.mainwindow.setMarker2((x1 + x2 + 0.0f) / 2, (y1 + y2 + 0.0f) / 2, (z1 + z2 + 0.0f) / 2, (x2 - x1), (y2 - y1), (z2 - z1));

            button_render.IsEnabled = true;
        }

        private void button_render_Click(object sender, RoutedEventArgs e)
        {
            //apply changes
            if (Convert.ToInt32(filter_x1.Text) > Convert.ToInt32(filter_x2.Text))
            {
                string h = filter_x1.Text;
                filter_x1.Text = filter_x2.Text;
                filter_x2.Text = h;
            }
            if (Convert.ToInt32(filter_y1.Text) > Convert.ToInt32(filter_y2.Text))
            {
                string h = filter_y1.Text;
                filter_y1.Text = filter_y2.Text;
                filter_y2.Text = h;
            }
            if (Convert.ToInt32(filter_z1.Text) > Convert.ToInt32(filter_z2.Text))
            {
                string h = filter_z1.Text;
                filter_z1.Text = filter_z2.Text;
                filter_z2.Text = h;
            }
            int x1 = Convert.ToInt32(filter_x1.Text);
            int y1 = Convert.ToInt32(filter_y1.Text);
            int z1 = Convert.ToInt32(filter_z1.Text);
            int x2 = Convert.ToInt32(filter_x2.Text);
            int y2 = Convert.ToInt32(filter_y2.Text);
            int z2 = Convert.ToInt32(filter_z2.Text);
            string targetuuid = null;
            if (filter_type.SelectedIndex < 0) filter_type.SelectedIndex = 0;
            if (filter_type.SelectedIndex > 0)
                targetuuid = ((Item)filter_type.SelectedItem).UUID;


            dynamic blueprint = BP.Blueprint;
            if (filtercolor != null&& filtercolor.StartsWith("#"))
                filtercolor = filtercolor.Substring(1, 6).ToLower();

            foreach(dynamic body in blueprint.bodies)
                foreach(dynamic child in body.childs)
                {
                    if (child.color.ToString().StartsWith("#"))
                        child.color = child.color.ToString().Substring(1, 6).ToLower();
                    dynamic realpos = BP.getposandbounds(child);
                    if((filtercolor == null || filtercolor == child.color.ToString()) &&
                        (targetuuid == null || targetuuid == child.shapeId.ToString()) &&
                        (x1 <= (int)realpos.pos.x && (int)realpos.pos.x + (int)realpos.bounds.x <= x2) &&
                        (y1 <= (int)realpos.pos.y && (int)realpos.pos.y + (int)realpos.bounds.y <= y2) &&
                        (z1 <= (int)realpos.pos.z && (int)realpos.pos.z + (int)realpos.bounds.z <= z2) )
                    {
                        child.pos.x = child.pos.x + Convert.ToInt32(new_x.Text);
                        child.pos.y = child.pos.y + Convert.ToInt32(new_y.Text);
                        child.pos.z = child.pos.z + Convert.ToInt32(new_z.Text);

                        if (new_color.Text != "")
                            child.color = new_color.Text.ToString().Substring(1, 6);
                        if (Edit_gate.IsVisible && new_gatemode.SelectedIndex > 0)
                            child.controller.mode = new_gatemode.SelectedIndex - 1;
                        if(Edit_lamp.IsVisible)
                        {
                            if (new_luminance.Text != "") child.controller.luminance = Convert.ToInt32(new_luminance.Text);
                            if (new_coneangle.Text != "") child.controller.coneAngle = Convert.ToInt32(new_coneangle.Text);
                            if (new_lampcolor.Text != "") child.controller.color = new_lampcolor.Text;
                        }
                        if(Edit_sensor.IsVisible && (new_sensorrange.Text != "" || new_sensorcolormode.IsChecked==true))
                        {
                            if (new_sensorrange.Text != "")
                                child.controller.range = Convert.ToInt32(new_sensorrange.Text);

                            child.controller.colorMode = new_sensorcolormode.IsChecked==true?true:false;
                            child.controller.color = new_sensorcolor.Text;
                        }
                        if(Edit_Timer.IsVisible && new_timerseconds.Text != null)
                        {
                            child.controller.seconds = Convert.ToInt32(new_timerseconds.Text);
                            child.controller.ticks = Convert.ToInt32(new_timerticks.Text);
                        }

                    }
                }
            if(blueprint.joints != null)
            foreach (dynamic child in blueprint.joints)
            {

                if (child.color.ToString().StartsWith("#"))
                    child.color = child.color.ToString().Substring(1, 6);
                dynamic c = child;
                c.pos = child.posA;
                c.xaxis = child.xaxisA;
                c.zaxis = child.zaxisA;
                dynamic realpos = BP.getposandbounds(c);
                realpos.pos = child.posA;
                if (!(Convert.ToInt32(child.zaxis.ToString())>0 || !(realpos.bounds.x != 1 || realpos.bounds.y != 1 || realpos.bounds.z != 1)))
                {
                    int zaxis = Convert.ToInt32(child.zaxis.ToString());
                    if (zaxis == -1)
                        realpos.pos.x -= realpos.bounds.x - 1;
                    if (zaxis == -2)
                        realpos.pos.y -= realpos.bounds.y - 1;
                    if (zaxis == -3)
                        realpos.pos.z -= realpos.bounds.z - 1;
                }

                if ((filtercolor == null || filtercolor == child.color.ToString()) &&
                    (targetuuid == null || targetuuid == child.shapeId.ToString()) &&
                    (x1 <= (int)realpos.pos.x && (int)realpos.pos.x + (int)realpos.bounds.x <= x2) &&
                    (y1 <= (int)realpos.pos.y && (int)realpos.pos.y + (int)realpos.bounds.y <= y2) &&
                    (z1 <= (int)realpos.pos.z && (int)realpos.pos.z + (int)realpos.bounds.z <= z2))
                {
                    child.posA.x = child.posA.x + Convert.ToInt32(new_x.Text);
                    child.posA.y = child.posA.y + Convert.ToInt32(new_y.Text);
                    child.posA.z = child.posA.z + Convert.ToInt32(new_z.Text);
                    if (new_color.Text != "")
                        child.color = new_color.Text.ToString().Substring(1, 6);


                }
            }


            Loadwindow w = new Loadwindow();
            w.Show();
            BP.Description.description = BP.Description.description += "++ Applied some area property changes ";
            BP.setblueprint(BP.Blueprint);
            this.mainwindow.RenderBlueprint();
            //Update();
            filterupdate();
            w.Close();
        }



        private void SET_Copy_Click(object sender, RoutedEventArgs e)
        {

            this.mainwindow.openpaintpicker();
            new_sensorcolor.Text = PaintSelector.PaintColor;

        }
        private void SET_Copy1_Click(object sender, RoutedEventArgs e)
        {
            this.mainwindow.openpaintpicker();
            new_lampcolor.Text = PaintSelector.PaintColor;
        }
        private void SET_Copy2_Click(object sender, RoutedEventArgs e)
        {
            this.mainwindow.openpaintpicker();
            new_color.Text = PaintSelector.PaintColor;
        }

        private void disableAll()
        {
            Edit_gate.Visibility = Visibility.Collapsed;
            Edit_general.Visibility = Visibility.Collapsed;
            Edit_lamp.Visibility = Visibility.Collapsed;
            Edit_sensor.Visibility = Visibility.Collapsed;
            Edit_Timer.Visibility = Visibility.Collapsed;
            button_render.IsEnabled = false;
            this.mainwindow.Marker = null;
            mainwindow.Image_blueprint.DataContext = "";
            mainwindow.Image_blueprint.DataContext = mainwindow;

        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            this.mainwindow.Marker = null;
            this.mainwindow.setMarker2(0, 0, 0, 0, 0, 0);
        }
    }
}
