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
        HashSet<string> uuidsbackup;
        dynamic backuplist;
        public BlockProperties(MainWindow mainwindow)
        {
            this.mainwindow = mainwindow;
            InitializeComponent();
            //new Loadwindow(this).Show();
            Loadwindow w = new Loadwindow();
            w.Show();

            Update();

            w.Close();

        }

        public void Update()
        {

            if (uuidsbackup != BP.GetUsedUuids())
            {
                usedblocks = new List<Item>
                {
                    new Item("any", "*")
                };
                foreach (string uuid in BP.GetUsedUuids())
                {
                    if (Database.blocks.ContainsKey(uuid))
                    {
                        usedblocks.Add(new Item(Database.blocks[uuid].Name.ToString(), uuid)); // fill the combobox!   only runs once!
                    }
                }
                filter_type.Items.Clear();
                foreach (Item useditem in usedblocks)
                        filter_type.Items.Add(useditem);

            }
            uuidsbackup = BP.GetUsedUuids(); 
            filter_type.SelectedIndex = 0;
            int i = 0;
            {
                backuplist = new JObject(); //fill 'backup'list with all childs!

                foreach (dynamic body in BP.Blueprint.bodies)
                    foreach (dynamic child in body.childs)
                    {
                        if (child.color.ToString().StartsWith("#"))
                            child.color = child.color.ToString().Substring(1);
                        child.blueprintIndex = i;
                        child.blockname = Database.blocks[child.shapeId.ToString()].Name;
                        dynamic realpos = BP.getposandbounds(child);

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
            dynamic bounds = BP.GetBounds();
            filter_x1.Text = bounds.minx.ToString();
            filter_y1.Text = bounds.miny.ToString();
            filter_z1.Text = bounds.minz.ToString();
            filter_x2.Text = bounds.maxx.ToString();
            filter_y2.Text = bounds.maxy.ToString();
            filter_z2.Text = bounds.maxz.ToString();

            filterupdate();
        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("find help here: \nhttp://youtube.com/c/brentbatch");
        }

        private void Filter_SET_Click(object sender, RoutedEventArgs e)
        {
            this.mainwindow.openpaintpicker();
            filter_color.Text = PaintSelector.PaintColor;
        }
        private void Filter_pos_TextChanged(object sender, TextChangedEventArgs e)
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
            // if (filter != null && filter.IsAlive) filter.Abort();
            // filter = new Thread(new ThreadStart(filter_output_update));
            // filter.SetApartmentState(ApartmentState.STA);
            // filter.Start();
            Loadwindow l = new Loadwindow();
            l.Show();
            filter_output_update();
            l.Close();
        }
        //Thread filter;

        private void filter_output_update()
        {

            {

                dynamic bounds = BP.GetBounds();
                int x1 = bounds.minx, y1 = bounds.maxx, z1 = bounds.miny, x2 = bounds.maxy, y2 = bounds.minz, z2 = bounds.maxz;
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
                        type = ((Item) filter_type.SelectedItem).UUID;
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

                try
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                        disableAll();
                    }));
                }
                catch { }
            }
        }
        int selectedchildindex=-1;
        private void filter_output_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            disableAll();
            if (filter_output.SelectedIndex != -1)
            {

                var bc = new BrushConverter();
                string selectedcolor = ((dynamic)filter_output.SelectedItem).color;
                string color = "#FF" + (selectedcolor.StartsWith("#")==true? selectedcolor.Substring(1): selectedcolor);
                //filter_output_color.Fill = (Brush)bc.ConvertFrom(color);
                //clear other options, enable/disable several

                dynamic selectedblock = ((dynamic)filter_output.SelectedItem);

                dynamic realpos = BP.getposandbounds(selectedblock);
                this.mainwindow.setMarker(Convert.ToInt32(realpos.pos.x) + (Convert.ToDouble(realpos.bounds.x)/2), Convert.ToInt32(realpos.pos.y) + (Convert.ToDouble(realpos.bounds.y) / 2), Convert.ToDouble(realpos.pos.z) + (Convert.ToDouble(realpos.bounds.z) / 2));

                selectedchildindex = selectedblock.blueprintIndex;
                Edit_general.Visibility = Visibility.Visible;
                new_x.Text = selectedblock.pos.x;
                new_y.Text = selectedblock.pos.y;
                new_z.Text = selectedblock.pos.z;
                new_color.Text = "#" + selectedblock.color;
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
                        new_sensorcolor.Text = "#"+selectedblock.controller.color;
                        if (selectedblock.controller.color == null) new_sensorcolor.Text = "#eeeeee";
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
                        new_timerticks.Text = selectedblock.controller.ticks;
                        Edit_Timer.Visibility = Visibility.Visible;
                    }

                    if (selectedblock.controller.timePerFrame != null)//controller!
                    {
                        //playMode timePerFrame joints
                        new_controllerloopmode.IsChecked = selectedblock.controller.playMode;
                        new_controllertimeperframe.Text = selectedblock.controller.timePerFrame;
                        new_controllercontrolls.Items.Clear();
                        if (selectedblock.controller.joints != null)
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
                        /*
                        if (selectedblock.controller.controllers != null)
                            foreach (dynamic joint in selectedblock.controller.frames)
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

                            }*/

                        Edit_controller.Visibility = Visibility.Visible;
                    }
                }
                button_render.IsEnabled = true;



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


        private void new_controllercontrolls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new_selectedcontroller.DataContext = (dynamic)new_controllercontrolls.SelectedItem;
            if (new_controllercontrolls.SelectedItem != null && ((dynamic)new_controllercontrolls.SelectedItem).reverse == true)
                new_controllerreverse.IsChecked = true;
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
            System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Watch this video on how to use: \nhttps://www.youtube.com/c/brentbatch", "Tutorial?", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
            if (messageBoxResult.ToString() == "Yes")
            { System.Diagnostics.Process.Start("https://youtu.be/glLgQemUS2I?t=799"); }
        }

        private void button_render_Click(object sender, RoutedEventArgs e)
        {
            //blueprintIndex
            foreach(dynamic body in BP.Blueprint.bodies)
                foreach(dynamic child in body.childs)
                    if (Convert.ToInt32(child.blueprintIndex) == selectedchildindex)
                    {

                        if (Edit_controller.IsVisible)
                        {
                            child.controller.joints.Clear();
                            foreach (dynamic item in (dynamic)new_controllercontrolls.Items)
                            {
                                item.startAngle = Convert.ToInt32(item.startAngle);
                                item.frames[0].targetAngle = Convert.ToInt32(item.controller0);
                                item.frames[1].targetAngle = Convert.ToInt32(item.controller1);
                                item.frames[2].targetAngle = Convert.ToInt32(item.controller2);
                                item.frames[3].targetAngle = Convert.ToInt32(item.controller3);
                                item.frames[4].targetAngle = Convert.ToInt32(item.controller4);
                                item.frames[5].targetAngle = Convert.ToInt32(item.controller5);
                                item.frames[6].targetAngle = Convert.ToInt32(item.controller6);
                                item.frames[7].targetAngle = Convert.ToInt32(item.controller7);
                                item.frames[8].targetAngle = Convert.ToInt32(item.controller8);
                                item.frames[9].targetAngle = Convert.ToInt32(item.controller9);
                                child.controller.joints.Add(item);
                            }
                        }
                        if(Edit_gate.IsVisible)
                        {
                            child.controller.mode = new_gatemode.SelectedIndex;
                        }
                        if(Edit_general.IsVisible)
                        {
                            child.pos.x = Convert.ToInt32(new_x.Text);
                            child.pos.y = Convert.ToInt32(new_y.Text);
                            child.pos.z = Convert.ToInt32(new_z.Text);
                            child.color = new_color.Text;
                            child.xaxis = Convert.ToInt32(new_xaxis.Text);
                            child.zaxis = Convert.ToInt32(new_zaxis.Text);

                            dynamic selectedblock = ((dynamic)filter_output.SelectedItem);
                            selectedblock.pos.x = child.pos.x;
                            selectedblock.pos.y = child.pos.y;
                            selectedblock.pos.z = child.pos.z;
                            selectedblock.xaxis = child.xaxis;
                            selectedblock.zaxis = child.zaxis;
                        }
                        if(Edit_sensor.IsVisible)
                        {
                            child.controller.colorMode = new_sensorcolormode.IsChecked;
                            child.controller.range = Convert.ToInt32(new_sensorrange.Text);
                            child.controller.color = new_sensorcolor.Text;
                        }
                        if(Edit_lamp.IsVisible)
                        {
                            child.controller.coneAngle = Convert.ToInt32(new_coneangle.Text);
                            child.controller.luminance = Convert.ToInt32(new_luminance.Text);
                        }
                        if(Edit_Timer.IsVisible)
                        {
                            child.controller.seconds = Convert.ToInt32(new_timerseconds.Text);
                            child.controller.ticks = Convert.ToInt32(new_timerticks.Text);
                        }
                }


            Loadwindow w = new Loadwindow();
            w.Show();

            BP.Description.description = BP.Description.description += "++ Applied some block property changes ";
            BP.setblueprint(BP.Blueprint);
            this.mainwindow.RenderBlueprint();
            //Update();
            dynamic bounds = BP.GetBounds();
            int x1 = bounds.minx, y1 = bounds.maxx, z1 = bounds.miny, x2 = bounds.maxy, y2 = bounds.minz, z2 = bounds.maxz;
            this.Dispatcher.Invoke((Action)(() =>
            {//this refer to form in WPF application 
                if (filter_x1.Text != "") x1 = Convert.ToInt32(filter_x1.Text);
                if (filter_y1.Text != "") y1 = Convert.ToInt32(filter_y1.Text);
                if (filter_z1.Text != "") z1 = Convert.ToInt32(filter_z1.Text);
                if (filter_x2.Text != "") x2 = Convert.ToInt32(filter_x2.Text);
                if (filter_y2.Text != "") y2 = Convert.ToInt32(filter_y2.Text);
                if (filter_z2.Text != "") z2 = Convert.ToInt32(filter_z2.Text);//0.1! = any
                this.mainwindow.setMarker2((x1 + x2 + 0.0f) / 2, (y1 + y2 + 0.0f) / 2, (z1 + z2 + 0.0f) / 2, (x2 - x1), (y2 - y1), (z2 - z1));
            }));
            w.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.mainwindow.Marker = null;
            this.mainwindow.setMarker2(0, 0, 0, 0, 0, 0);
            this.mainwindow.Marker2 = new System.Windows.Media.Media3D.Model3DGroup();
        }

    }
}
