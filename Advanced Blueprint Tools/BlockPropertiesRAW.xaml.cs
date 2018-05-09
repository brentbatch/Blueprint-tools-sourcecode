using Newtonsoft.Json.Linq;
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
    /// Interaction logic for BlockPropertiesRAW.xaml
    /// </summary>
    public partial class BlockPropertiesRAW : Window
    {
        MainWindow mainwindow;
        HashSet<string> uuidsbackup = new HashSet<string>();
        dynamic blueprint;
        public BlockPropertiesRAW(MainWindow mainwindow)
        {
            this.mainwindow = mainwindow;
            InitializeComponent();
            Loadwindow w = new Loadwindow();
            w.Show();
            Update();

            w.Close();
        }
        private void button_help_Click(object sender, RoutedEventArgs e)
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
                filterupdate();
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
                    if (textbox_color.Name == "filter_color") filtercolor = textbox_color.Text.Substring(1, 6).ToLower();
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
            blueprint = BP.Blueprint;
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
            if (uuidsbackup != BP.GetUsedUuids())//fill the combobox once
            {
                filter_type.Items.Clear();
                filter_type.Items.Add(new Item("any", "*"));
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
        
        string filtercolor = null;

        Thread filter;
        private void filterupdate()
        {
            var l = new Loadwindow();
            l.Show();
            disableAll();
            filter_output.Items.Clear();
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
            this.mainwindow.setMarker2((x1 + x2 + 0.0f) / 2, (y1 + y2 + 0.0f) / 2, (z1 + z2 + 0.0f) / 2, (x2 - x1), (y2 - y1), (z2 - z1));
            string targetuuid = null;
            if (filter_type.SelectedIndex < 0) filter_type.SelectedIndex = 0;
            if (filter_type.SelectedIndex > 0)
                targetuuid = ((Item)filter_type.SelectedItem).UUID;


            //thread:
            if (filter != null && filter.IsAlive)
                filter.Abort();
            filter = new Thread(() =>
            {
                //filter correct blocks:
                int i = 0;
                foreach (dynamic body in blueprint.bodies)
                    foreach (dynamic child in body.childs)
                    {
                        if (child.color.ToString().StartsWith("#"))
                            child.color = child.color.ToString().Substring(1, 6).ToLower();
                        dynamic realpos = BP.getposandbounds(child);
                        if ((filtercolor == null || filtercolor == child.color.ToString()) &&
                            (targetuuid == null || targetuuid == child.shapeId.ToString()) &&
                            (x1 <= (int)realpos.pos.x && (int)realpos.pos.x + (int)realpos.bounds.x <= x2) &&
                            (y1 <= (int)realpos.pos.y && (int)realpos.pos.y + (int)realpos.bounds.y <= y2) &&
                            (z1 <= (int)realpos.pos.z && (int)realpos.pos.z + (int)realpos.bounds.z <= z2))
                        {
                            dynamic listitem = new JObject();
                            listitem.pos = new JObject();
                            listitem.pos.x = (int)realpos.pos.x;
                            listitem.pos.y = (int)realpos.pos.y;
                            listitem.pos.z = (int)realpos.pos.z;
                            listitem.bounds = new JObject();
                            listitem.bounds.x = (int)realpos.bounds.x;
                            listitem.bounds.y = (int)realpos.bounds.y;
                            listitem.bounds.z = (int)realpos.bounds.z;
                            listitem.blockname = "unnamed shape" + child.shapeId.ToString();
                            listitem.index = i;
                            if (Database.blocks.ContainsKey(child.shapeId.ToString()))
                                listitem.blockname = Database.blocks[child.shapeId.ToString()].Name;
                            listitem.color = child.color.ToString();
                            listitem.child = child.ToString();
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                filter_output.Items.Add(listitem);
                            }));
                        }
                        i++;
                    }
                if (blueprint.joints != null)
                    foreach (dynamic child in blueprint.joints)
                    {

                        if (child.color.ToString().StartsWith("#"))
                            child.color = child.color.ToString().Substring(1, 6).ToLower();
                        dynamic c = child;
                        c.pos = child.posA;
                        c.xaxis = child.xaxisA;
                        c.zaxis = child.zaxisA;
                        dynamic realpos = BP.getposandbounds(c);
                        realpos.pos = child.posA;
                        if (!(Convert.ToInt32(child.zaxis.ToString()) > 0 || !(realpos.bounds.x != 1 || realpos.bounds.y != 1 || realpos.bounds.z != 1)))
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
                            dynamic listitem = new JObject();
                            listitem.pos = new JObject();
                            listitem.pos.x = (int)realpos.pos.x;
                            listitem.pos.y = (int)realpos.pos.y;
                            listitem.pos.z = (int)realpos.pos.z;
                            listitem.bounds = new JObject();
                            listitem.bounds.x = (int)realpos.bounds.x;
                            listitem.bounds.y = (int)realpos.bounds.y;
                            listitem.bounds.z = (int)realpos.bounds.z;
                            listitem.blockname = "unnamed shape" + child.shapeId.ToString();
                            if (Database.blocks.ContainsKey(child.shapeId.ToString()))
                                listitem.blockname = Database.blocks[child.shapeId.ToString()].Name;
                            listitem.color = child.color.ToString();
                            listitem.child = child.ToString();
                            listitem.index = i;
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                filter_output.Items.Add(listitem);
                            }));
                        }
                        i++;
                    }
            });
            filter.IsBackground = true;
            filter.Start();
            l.Close();
        }
        private void filter_output_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(filter_output.SelectedIndex>=0)
            {
                dynamic selected = ((dynamic)filter_output.SelectedItem);
                string child = selected.child.ToString();
                
                this.mainwindow.setMarker(Convert.ToInt32(selected.pos.x) + (Convert.ToDouble(selected.bounds.x) / 2), Convert.ToInt32(selected.pos.y) + (Convert.ToDouble(selected.bounds.y) / 2), Convert.ToDouble(selected.pos.z) + (Convert.ToDouble(selected.bounds.z) / 2));

                Edit_child.Text = child;

                button_render.IsEnabled = true;
            }
        }
        private void button_render_Click(object sender, RoutedEventArgs e)
        {
            //string blueprintstr = blueprint.ToString();
            //string child = ((dynamic)filter_output.SelectedItem).child.ToString();
            Loadwindow w = new Loadwindow();
            try
            {
                w.Show();
                dynamic child = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(Edit_child.Text);
                int i = 0;
                int index = ((dynamic)filter_output.SelectedItem).index;
                for (int j=0; j<blueprint.bodies.Count; j++)
                    for(int k=0; k<blueprint.bodies[j].childs.Count; k++)
                    {
                        if (index == i)
                        {
                            blueprint.bodies[j].childs[k] = child;
                            BP.setblueprint(blueprint);
                            BP.Description.description = BP.Description.description + "\n++Applied RAW changes";
                            mainwindow.RenderBlueprint();
                            //Update(); 
                            w.Close();
                            MessageBox.Show("edit successful");

                            filterupdate();
                            return;
                        }
                        i++;
                    }

                for (int j = 0; j < blueprint.joints.Count; j++)
                {
                    if (index == i)
                    {
                        blueprint.joints[j] = child;
                        BP.setblueprint(blueprint);
                        BP.Description.description = BP.Description.description + "\n++Applied RAW changes";
                        mainwindow.RenderBlueprint();
                        //Update();
                        w.Close();
                        MessageBox.Show("edit successful");
                        filterupdate();
                        return;
                    }
                    i++;
                }

                w.Close();
                MessageBox.Show("edit UNsuccessfull :(");
            }
            catch (Exception exc)
            {
                w.Close();
                MessageBox.Show(exc.Message);
            }
        }
        private void disableAll()
        {
            Edit_child.Text = "";
            button_render.IsEnabled = false;
            this.mainwindow.Marker = null;
            mainwindow.Image_blueprint.DataContext = "";
            mainwindow.Image_blueprint.DataContext = mainwindow;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            this.mainwindow.Marker = null;
            this.mainwindow.Marker2 = null;
            mainwindow.Image_blueprint.DataContext = null;
            mainwindow.Image_blueprint.DataContext = mainwindow;
            this.mainwindow.helix.ResetCamera();
        }

    }
}
