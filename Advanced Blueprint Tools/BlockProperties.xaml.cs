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
using System.ComponentModel;

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class BlockProperties : Window
    {
        MainWindow mainwindow;
        List<Item> usedblocks;
        List<string> uuidsbackup;
        dynamic backuplist;
        public BlockProperties(MainWindow mainwindow)
        {
            this.mainwindow = mainwindow;
            InitializeComponent();
            //new Loadwindow(this).Show();
            Loadwindow w = new Loadwindow();
            w.Show();

            update();

            w.Close();
            //update();

        }

        public void update()
        {
            usedblocks = new List<Item>();
            usedblocks.Add(new Item("any", "*"));

            if (uuidsbackup != this.mainwindow.OpenedBlueprint.useduuids)
            {
                foreach (string uuid in this.mainwindow.OpenedBlueprint.useduuids)
                {
                    if (this.mainwindow.getgameblocks()[uuid] != null)
                    {

                        dynamic part = this.mainwindow.getgameblocks()[uuid];
                            usedblocks.Add(new Item(part.Name.ToString(), part.uuid.ToString())); // fill the combobox!   only runs once!
                    }
                }
                filter_type.Items.Clear();
                foreach (Item useditem in usedblocks)
                        filter_type.Items.Add(useditem.Name);

            }
            uuidsbackup = this.mainwindow.OpenedBlueprint.useduuids;

            int i = 0;
            {
                backuplist = new JObject(); //fill 'backup'list with all childs!

                foreach (dynamic body in this.mainwindow.OpenedBlueprint.blueprint.bodies)
                    foreach (dynamic child in body.childs)
                    {
                        child.blueprintIndex = i;
                        dynamic blocks = this.mainwindow.getgameblocks();
                        child.blockname = blocks[child.shapeId.ToString()].Name;
                        dynamic realpos = this.mainwindow.OpenedBlueprint.getposandbounds(child);

                        if (backuplist[realpos.pos.x.ToString()] == null) backuplist[realpos.pos.x.ToString()] = new JObject();
                        if (backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()] == null) backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()] = new JObject();
                        if (backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()][realpos.pos.z.ToString()] == null) backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()][realpos.pos.z.ToString()] = new JObject();
                        if (backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()][realpos.pos.z.ToString()][realpos.color.ToString().ToLower()] == null)
                            backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()][realpos.pos.z.ToString()][realpos.color.ToString().ToLower()] = new JObject();
                        if (backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()][realpos.pos.z.ToString()][realpos.color.ToString().ToLower()][realpos.shapeId.ToString().ToLower()] == null)
                            backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()][realpos.pos.z.ToString()][realpos.color.ToString().ToLower()][realpos.shapeId.ToString().ToLower()] = new JObject();
                        backuplist[realpos.pos.x.ToString()][realpos.pos.y.ToString()][realpos.pos.z.ToString()][realpos.color.ToString().ToLower()][realpos.shapeId.ToString().ToLower()][i.ToString()] = child;

                        i++;
                    }
            }

            //fill xyz:
            int x1 = this.mainwindow.OpenedBlueprint.minx, y1 = this.mainwindow.OpenedBlueprint.miny, z1 = this.mainwindow.OpenedBlueprint.minz, x2 = this.mainwindow.OpenedBlueprint.maxx + 1, y2 = this.mainwindow.OpenedBlueprint.maxy + 1, z2 = this.mainwindow.OpenedBlueprint.maxz + 1;
            filter_x1.Text = x1.ToString();
            filter_y1.Text = y1.ToString();
            filter_z1.Text = z1.ToString();
            filter_x2.Text = x2.ToString();
            filter_y2.Text = y2.ToString();
            filter_z2.Text = z2.ToString();

            filterupdate();
        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("find help here: \nhttp://youtube.com/c/brentbatch");
        }

        private void filter_SET_Click(object sender, RoutedEventArgs e)
        {
            mainwindow.openpaintpicker();
            filter_color.Text = this.mainwindow.PaintColor;
        }
        private void Filter_pos_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            try
            {
                if (Convert.ToInt32(t.Text) >= 0 || Convert.ToInt32(t.Text) <= 0) { }
            }
            catch
            {
                t.Text = "";
            }
            if (this.IsLoaded)
                filterupdate();
        }
        private void filter_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
                filterupdate();
        }
        private string filtercolor = null;
        private void filter_color_TextChanged(object sender, TextChangedEventArgs e)
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

            if (this.IsLoaded)
                filterupdate();
        }
        private void color_TextChanged(Object sender , TextChangedEventArgs e)
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
                textbox_color.Background = (Brush)bc.ConvertFrom("#eeeeee");
            }

        }


        private void filterupdate()
        {
            if (filter != null && filter.IsAlive) filter.Abort();
            filter = new Thread(new ThreadStart(filter_output_update));
            filter.SetApartmentState(ApartmentState.STA);
            filter.Start();
        }
        Thread filter;

        private void filter_output_update()
        {
            Loadwindow l = new Loadwindow();
            l.Show();

            { 
                int x1 = this.mainwindow.OpenedBlueprint.minx-2, y1 = this.mainwindow.OpenedBlueprint.miny-2, z1 = this.mainwindow.OpenedBlueprint.minz-2, x2 = this.mainwindow.OpenedBlueprint.maxx+2, y2 = this.mainwindow.OpenedBlueprint.maxy+2, z2 = this.mainwindow.OpenedBlueprint.maxz+2;
                string type = "*";//nullable! = any

                this.Dispatcher.Invoke((Action)(() =>
                {//this refer to form in WPF application 

                    if (filter_output.Items.Count > 0) filter_output.Items.Clear();
                    if (filter_x1.Text != "") x1 = Convert.ToInt32(filter_x1.Text);
                    if (filter_y1.Text != "") y1 = Convert.ToInt32(filter_y1.Text);
                    if (filter_z1.Text != "") z1 = Convert.ToInt32(filter_z1.Text);
                    if (filter_x2.Text != "") x2 = Convert.ToInt32(filter_x2.Text);
                    if (filter_y2.Text != "") y2 = Convert.ToInt32(filter_y2.Text);
                    if (filter_z2.Text != "") z2 = Convert.ToInt32(filter_z2.Text);//0.1! = any
                    if (filter_type.SelectedIndex < 0) filter_type.SelectedIndex = 0;
                    if (filter_type.SelectedIndex >= 0)
                        type = usedblocks[filter_type.SelectedIndex].UUID.ToLower();
                    this.mainwindow.setMarker2((x1 + x2+0.0f) / 2, (y1 + y2 + 0.0f) / 2, (z1 + z2 + 0.0f) / 2, (x2 - x1), (y2 - y1), (z2 - z1));
                }));
                string color = filtercolor;//nullable! = any


                for (int x = x1; x <= x2; x++)
                    if (backuplist[x.ToString()] != null)
                        for (int y = y1; y < y2; y++)
                            if (backuplist[x.ToString()][y.ToString()] != null)
                                for (int z = z1; z < z2; z++)
                                    if (backuplist[x.ToString()][y.ToString()][z.ToString()] != null)
                                    {
                                        if (color == null)
                                        {
                                            foreach (dynamic xyzcolor in backuplist[x.ToString()][y.ToString()][z.ToString()])
                                                if (type == "*")
                                                {
                                                    foreach (dynamic childuuid in xyzcolor.Value)
                                                    {
                                                        foreach (dynamic child in childuuid.Value)
                                                            this.Dispatcher.Invoke((Action)(() =>
                                                            {//this refer to form in WPF application 
                                                                filter_output.Items.Add(child.Value);  //any color any type
                                                            }));
                                                    }
                                                }
                                                else
                                                {
                                                    if (xyzcolor.Value[type.ToLower()] != null)
                                                        foreach (dynamic child in xyzcolor.Value[type.ToLower()])
                                                            this.Dispatcher.Invoke((Action)(() =>
                                                            {//this refer to form in WPF application 
                                                                filter_output.Items.Add(child.Value); //any color , one type
                                                            }));
                                                }
                                        }
                                        else
                                        {
                                            if (color.StartsWith("#"))
                                                color = color.Substring(1).ToLower();
                                            if (backuplist[x.ToString()][y.ToString()][z.ToString()][color] != null)
                                            {
                                                if (type == "*")
                                                {
                                                    foreach (dynamic childuuid in backuplist[x.ToString()][y.ToString()][z.ToString()][color])
                                                    {
                                                        foreach (dynamic child in childuuid.Value)
                                                            this.Dispatcher.Invoke((Action)(() =>
                                                            {//this refer to form in WPF application 
                                                                filter_output.Items.Add(child.Value); //one color , any type
                                                            }));
                                                    }
                                                }
                                                else
                                                {
                                                    if (backuplist[x.ToString()][y.ToString()][z.ToString()][color][type] != null)
                                                        foreach (dynamic child in backuplist[x.ToString()][y.ToString()][z.ToString()][color][type])
                                                            this.Dispatcher.Invoke((Action)(() =>
                                                            {//this refer to form in WPF application 
                                                                filter_output.Items.Add(child.Value);  //one color, one type
                                                            }));
                                                }
                                            }
                                        }
                                    }

                this.Dispatcher.Invoke((Action)(() =>
                {//this refer to form in WPF application 
                    disableAll();
                }));
                l.Close();
                
            }
        }

        private void filter_output_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            disableAll();
            if (filter_output.SelectedIndex != -1)
            {

                var bc = new BrushConverter();
                string selectedcolor = ((dynamic)filter_output.SelectedItem).color;
                string color = "#FF" + (selectedcolor.StartsWith("#")==true? selectedcolor.Substring(1): selectedcolor);
                filter_output_color.Fill = (Brush)bc.ConvertFrom(color);
                //clear other options, enable/disable several

                dynamic selectedblock = ((dynamic)filter_output.SelectedItem);

                dynamic realpos = this.mainwindow.OpenedBlueprint.getposandbounds(selectedblock);
                this.mainwindow.setMarker(Convert.ToInt32(realpos.pos.x) + (Convert.ToDouble(selectedblock.bounds.x)/2), Convert.ToInt32(realpos.pos.y) + (Convert.ToDouble(selectedblock.bounds.y) / 2), Convert.ToDouble(realpos.pos.z) + (Convert.ToDouble(selectedblock.bounds.z) / 2));

                button_render.Visibility = Visibility.Visible;
                Edit_general.Visibility = Visibility.Visible;
                new_x.Text = selectedblock.pos.x;
                new_y.Text = selectedblock.pos.y;
                new_z.Text = selectedblock.pos.z;
                new_xaxis.Text = selectedblock.xaxis;
                new_zaxis.Text = selectedblock.zaxis;
                if(selectedblock.controller != null) //
                {
                    if(selectedblock.controller.mode != null) //logic gate!
                    {
                        
                        new_gatemode.SelectedIndex = selectedblock.controller.mode;
                        Edit_gate.Visibility = Visibility.Visible;
                        //mode 0-5
                    }
                    if(selectedblock.controller.buttonMode != null)//sensor!
                    {
                        // colorMode & range & color
                        new_sensorcolormode.IsChecked = selectedblock.controller.colorMode;
                        if (selectedblock.controller.colorMode == null) new_sensorcolormode.IsChecked = false;
                        new_sensorrange.Text = selectedblock.controller.range;
                        new_sensorcolor.Text = selectedblock.controller.color;
                        if(selectedblock.controller.color==null)
                        Edit_sensor.Visibility = Visibility.Visible;
                    }
                    if (selectedblock.controller.luminance != null)//light!
                    {
                        //coneAngle  & luminance
                        new_coneangle.Text = selectedblock.controller.coneAngle;
                        new_luminance.Text = selectedblock.controller.luminance;
                        Edit_lamp.Visibility = Visibility.Visible;
                    }
                    if(selectedblock.controller.seconds != null)//timer
                    {
                        //seconds ticks
                        new_timerseconds.Text = selectedblock.controller.seconds;
                        new_luminance.Text = selectedblock.controller.ticks;
                        Edit_Timer.Visibility = Visibility.Visible;
                    }

                    if (selectedblock.controller.timePerFrame != null)//controller!
                    {
                        //playMode timePerFrame joints
                        new_controllerloopmode.IsChecked = selectedblock.controller.playMode;
                        new_controllertimeperframe.Text = selectedblock.controller.timePerFrame;
                        new_controllercontrolls.Items.Clear();
                        foreach (dynamic joint in selectedblock.controller.joints)
                        {
                            joint.controller0 = joint.frames[0].targetAngle.ToString();
                            joint.controller1 = joint.frames[1].targetAngle.ToString();
                            joint.controller2 = joint.frames[2].targetAngle.ToString();
                            joint.controller3 = joint.frames[3].targetAngle.ToString();
                            joint.controller4 = joint.frames[4].targetAngle.ToString();
                            joint.controller5 = joint.frames[5].targetAngle.ToString();
                            joint.controller6 = joint.frames[6].targetAngle.ToString();
                            joint.controller7 = joint.frames[7].targetAngle.ToString();
                            joint.controller8 = joint.frames[8].targetAngle.ToString();
                            joint.controller9 = joint.frames[9].targetAngle.ToString();
                            new_controllercontrolls.Items.Add(joint);

                        }

                        Edit_controller.Visibility = Visibility.Visible;
                    }
                }


                
            }
        }

        private void disableAll()
        {
            Edit_controller.Visibility = Visibility.Collapsed;
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

        private void SET_Copy_Click(object sender, RoutedEventArgs e)
        {

            mainwindow.openpaintpicker();
            new_sensorcolor.Text = this.mainwindow.PaintColor;

        }

        private void SET_Copy1_Click(object sender, RoutedEventArgs e)
        {
            mainwindow.openpaintpicker();
            new_lampcolor.Text = this.mainwindow.PaintColor;
        }


        private void new_controllercontrolls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new_selectedcontroller.DataContext = (dynamic)new_controllercontrolls.SelectedItem;
        }

        private void new_controller_joint_Changed(object sender, TextChangedEventArgs e)
        {
            dynamic test = (dynamic)new_selectedcontroller.DataContext;
        }

        private void new_controllerreverse_Click(object sender, RoutedEventArgs e)
        {
            new_controller_joint_Changed(null, null);
        }

        private void button_help_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("for help: youtube.com/c/brentbatch");
        }

        private void button_render_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.mainwindow.Marker = null;
            this.mainwindow.setMarker2(0, 0, 0, 0, 0, 0);
        }
    }
}
