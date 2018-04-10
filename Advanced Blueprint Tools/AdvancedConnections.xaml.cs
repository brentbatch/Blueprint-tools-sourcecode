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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for AdvancedConnections.xaml
    /// </summary>
    public partial class AdvancedConnections : Window
    {
        
        MainWindow window;
        private dynamic uuidsbackup;

        List<connectable> connectable_UUIDS = new List<connectable>();
        //List<Item> ItemList;
        public AdvancedConnections(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            this.update();
        }

        public void update()
        {
            if(uuidsbackup != this.window.OpenedBlueprint.useduuids)
            {
                connectable_UUIDS.Clear();
                foreach (string uuid in this.window.OpenedBlueprint.useduuids)
                {
                    if (Database.blocks.ContainsKey(uuid) && Database.blocks[uuid] is Part && (Database.blocks[uuid] as Part).IsConnectable)
                    {
                        connectable_UUIDS.Add(new connectable(uuid.ToString(), ((Part)Database.blocks[uuid]).GetBounds(), Database.blocks[uuid].Name));

                    }
                }
                this.Dispatcher.Invoke((Action)(() =>
                {//this refer to form in WPF application 
                    comboBox_items1.Items.Clear();
                    comboBox_items2.Items.Clear();
                }));
                foreach (connectable i in connectable_UUIDS)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                        comboBox_items1.Items.Add(i.name);
                        comboBox_items2.Items.Add(i.name);
                    }));
                }
            }
            uuidsbackup = this.window.OpenedBlueprint.useduuids;
        }

        

        private void textBox_color_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.update();
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
            textBox_color1.Text = PaintSelector.PaintColor;
        }

        private void button3_Copy_Click(object sender, RoutedEventArgs e)
        {
            window.openpaintpicker();
            textBox_color2.Text = PaintSelector.PaintColor;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.update();
            PaintSelector p = new PaintSelector();
            p.Owner = this.window;
            p.Show();
        }

        //Wire it!
        private void button1_Click(object sender, RoutedEventArgs e)
        {//WIRE IT!
            if(comboBox_items1.SelectedIndex!=-1 && comboBox_items2.SelectedIndex != -1 && window.OpenedBlueprint != null)
            {
                string sourcecolor = textBox_color1.Text;
                string destinationcolor = textBox_color2.Text;
                connectable sourceblock = connectable_UUIDS[comboBox_items1.SelectedIndex];
                connectable destinationblock = connectable_UUIDS[comboBox_items2.SelectedIndex];

                dynamic backupbp = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(window.OpenedBlueprint.blueprint.ToString());
                //list all destionationblocks and their ID's
                dynamic destids = new JObject();
                /*
                destids["-1"] = new JObject();
                destids["-1"]["1"] = new JObject();
                destids["-1"]["1"]["5"] = new JObject();
                destids["-1"]["1"]["5"].id = 5;
                */
                int minx = 10000, maxx = -10000, miny = 10000, maxy = -10000, minz = 10000, maxz = -10000;
                //loop over all blocks:
                foreach (dynamic body in window.OpenedBlueprint.blueprint.bodies)
                {
                    foreach(dynamic child in body.childs)
                    {
                        if(child.shapeId.Value.ToLower() == destinationblock.UUID.ToLower() /*&& (child.color.Value.ToLower() == destinationcolor.ToLower() || "#"+child.color.Value.ToLower() == destinationcolor.ToLower() || destinationcolor == "" || destinationcolor == "#")*/)
                        {
                            dynamic dest = getposandbounds(child);//outputs corrected child (default rotation, correct position)

                            string x = dest.pos.x.ToString();
                            string y = dest.pos.y.ToString();
                            string z = dest.pos.z.ToString();

                            if (destids[x] == null)
                                destids[x] = new JObject();
                            if (destids[x][y] == null)
                                destids[x][y] = new JObject();
                            if (destids[x][y][z] == null)
                                destids[x][y][z] = new JObject();
                            if (destids[x][y][z].ids == null)
                                destids[x][y][z].ids = new JArray();
                            dynamic id = new JObject();
                            id.id = child.controller.id;
                            destids[x][y][z].ids.Add(id);
                            destids[x][y][z].color = child.color;

                            //get whole creation bounds:
                            if (dest.pos.x < minx) minx = dest.pos.x;
                            if (dest.pos.x > maxx) maxx = dest.pos.x;
                            if (dest.pos.y < miny) miny = dest.pos.y;
                            if (dest.pos.y > maxy) maxy = dest.pos.y;
                            if (dest.pos.z < minz) minz = dest.pos.z;
                            if (dest.pos.z > maxz) maxz = dest.pos.z;
                           /* {//test
                                if (destids[x + "." + y + "." + z] == null) destids[x + "." + y + "." + z] = new JArray();
                                dynamic blockproperties = new JObject();
                                blockproperties.id = new JObject();
                                blockproperties.id.id = child.controller.id;
                                blockproperties.uuid = child.shapeId;
                                if (child.color.ToString().StartsWith("#")) child.color = child.color.ToString().subString(1);
                                blockproperties.color = child.color;
                                destids[x + "." + y + "." + z].Add(blockproperties);
                            }*/
                        }
                    }
                }
                if(window.OpenedBlueprint.blueprint.joints != null)
                foreach(dynamic child in window.OpenedBlueprint.blueprint.joints)
                {
                    if (child.shapeId.Value.ToLower() == destinationblock.UUID.ToLower() /*&& (child.color.Value.ToLower() == destinationcolor.ToLower() || "#"+child.color.Value.ToLower() == destinationcolor.ToLower() || destinationcolor == "" || destinationcolor == "#")*/)
                    {
                        dynamic dest = (child);//outputs corrected child (default rotation, correct position)

                        string x = dest.pos.x.ToString();
                        string y = dest.pos.y.ToString();
                        string z = dest.pos.z.ToString();

                        if (destids[x] == null)
                            destids[x] = new JObject();
                        if (destids[x][y] == null)
                            destids[x][y] = new JObject();
                        if (destids[x][y][z] == null)
                            destids[x][y][z] = new JObject();
                        if (destids[x][y][z].ids == null)
                            destids[x][y][z].ids = new JArray();
                        dynamic id = new JObject();
                        id.id = child.controller.id;
                        destids[x][y][z].ids.Add(id);
                        destids[x][y][z].color = child.color;

                        //get whole creation bounds:
                        if (dest.pos.x < minx) minx = dest.pos.x;
                        if (dest.pos.x > maxx) maxx = dest.pos.x;
                        if (dest.pos.y < miny) miny = dest.pos.y;
                        if (dest.pos.y > maxy) maxy = dest.pos.y;
                        if (dest.pos.z < minz) minz = dest.pos.z;
                        if (dest.pos.z > maxz) maxz = dest.pos.z;
                    }

                }

                int amountwired=0;
                try
                {
                    //can be improved!

                    foreach(dynamic body in window.OpenedBlueprint.blueprint.bodies)
                        foreach(dynamic child in body.childs)
                            if (child.shapeId.Value.ToLower() == sourceblock.UUID.ToLower() && (child.color.Value.ToLower() == sourcecolor.ToLower() || "#" + child.color.Value.ToLower() == sourcecolor.ToLower() || sourcecolor == "" || sourcecolor == "#"))
                            {//COLOR AND UUID CORRECT, FURTHER WIRING PROCESS:
                                dynamic source = getposandbounds(child);//outputs corrected child (default rotation, correct position)

                                string x = source.pos.x.ToString();
                                string y = source.pos.y.ToString();
                                string z = source.pos.z.ToString();

                                if (checkBox_X.IsChecked == false)
                                {
                                    minx = source.pos.x;
                                    maxx = source.pos.x;//bounds?
                                }
                                if (checkBox_Y.IsChecked == false)
                                {
                                    miny = source.pos.y;
                                    maxy = source.pos.y;
                                }
                                if (checkBox_Z.IsChecked == false)
                                {
                                    minz = source.pos.z;
                                    maxz = source.pos.z;
                                }


                                for (int i = minx; i <= maxx; i++)
                                    if (destids[i.ToString()] != null)
                                        for (int j = miny; j <= maxy; j++)
                                            if (destids[i.ToString()][j.ToString()] != null)
                                                for (int k = minz; k <= maxz; k++)
                                                    if (destids[i.ToString()][j.ToString()][k.ToString()] != null)
                                                        if (destids[i.ToString()][j.ToString()][k.ToString()].color.ToString().ToLower() == destinationcolor.ToLower() || "#" + destids[i.ToString()][j.ToString()][k.ToString()].color.ToString().ToLower() == destinationcolor.ToLower() || destinationcolor == "" || destinationcolor == "#")
                                                            if (destids[(i+ Convert.ToInt32(textBox_X.Text)).ToString()] !=null && destids[(i + Convert.ToInt32(textBox_X.Text)).ToString()][(j + Convert.ToInt32(textBox_Y.Text)).ToString()] != null && destids[(i + Convert.ToInt32(textBox_X.Text)).ToString()][(j + Convert.ToInt32(textBox_Y.Text)).ToString()][(k + Convert.ToInt32(textBox_Z.Text)).ToString()] != null)
                                                                foreach (dynamic id in destids[(i + Convert.ToInt32(textBox_X.Text)).ToString()][(j + Convert.ToInt32(textBox_Y.Text)).ToString()][(k + Convert.ToInt32(textBox_Z.Text)).ToString()].ids)
                                                                    if (!(checkBox_0.IsChecked == false && child.controller.id == id.id))
                                                                    {
                                                                        if (child.controller.controllers == null)
                                                                            child.controller.controllers = new JArray();
                                                                        child.controller.controllers.Add(id);
                                                                        amountwired++;
                                                                    }

                                /*
                                for (int i = minx; i <= maxx; i++)
                                    for (int j = miny; j <= maxy; j++)
                                        for (int k = minz; k <= maxz; k++)
                                            if (
                                                destids[i.ToString() + "." + j.ToString() + "." + k.ToString()] != null &&
                                                destids[(i + Convert.ToInt32(textBox_X.Text)).ToString() + "." +
                                                        (j + Convert.ToInt32(textBox_Y.Text)).ToString() + "." +
                                                        (k + Convert.ToInt32(textBox_Z.Text)).ToString()] != null
                                                )
                                                foreach ( dynamic destblocknooffset in destids[i.ToString() + "." + j.ToString() + "." + k.ToString()])
                                                    if((destblocknooffset.color.ToLower() == destinationcolor.ToLower() || destinationcolor == "" || destinationcolor == "#"))
                                                        foreach(dynamic blockoffset in destids[(i + Convert.ToInt32(textBox_X.Text)).ToString() + "." + (j + Convert.ToInt32(textBox_Y.Text)).ToString() + "." + (k + Convert.ToInt32(textBox_Z.Text)).ToString()])
                                                            if(!(checkBox_0.IsChecked == false && child.controller.id == blockoffset.id.id))
                                                            {
                                                                if (child.controller.controllers == null)
                                                                    child.controller.controllers = new JArray();
                                                                child.controller.controllers.Add(blockoffset.id);
                                                                amountwired++;
                                                            }
                                */

                            }

                    new System.Threading.Thread(new System.Threading.ThreadStart(() => { MessageBox.Show("Successfully made " + amountwired + " connections! :D");})).Start();
                    
                    //window.UpdateOpenedBlueprint();
                    if (amountwired>0)
                        window.OpenedBlueprint.description.description = window.OpenedBlueprint.description.description + "\n--> " + amountwired + " connections made between " + sourcecolor + " " + sourceblock.name + " and " + destinationcolor + " " + destinationblock.name;
                   
                }
                catch(Exception exception)
                {
                    MessageBox.Show(exception.Message+"\n\n Blueprint connections abrupted\nrestoring blueprint","ERROR");
                    window.OpenedBlueprint.blueprint = backupbp;
                }
            }
            else
            {
                MessageBox.Show("Something went wrong!\nno harm done");
            }




        }



        //get bounds from 
        private dynamic getbounds(dynamic part)
        {
            dynamic bounds = new Newtonsoft.Json.Linq.JObject();
            if (part.box != null)
            {
                return part.box;
            }
            if (part.hull != null)
            {
                bounds.x = part.hull.x;
                bounds.y = part.hull.y;
                bounds.z = part.hull.z;
                return bounds;
            }
            if (part.cylinder != null)
            {
                if (part.cylinder.axis.ToString().ToLower() == "x")
                {
                    bounds.x = part.cylinder.depth;
                    bounds.y = part.cylinder.diameter * 2;
                    bounds.z = part.cylinder.diameter * 2;
                    return bounds;

                }
                else
                if (part.cylinder.axis.ToString().ToLower() == "y")
                {
                    bounds.x = part.cylinder.diameter * 2;
                    bounds.y = part.cylinder.depth;
                    bounds.z = part.cylinder.diameter * 2;
                    return bounds;

                }
                else
                if (part.cylinder.axis.ToString().ToLower() == "z")
                {
                    bounds.x = part.cylinder.diameter * 2;
                    bounds.y = part.cylinder.diameter * 2;
                    bounds.z = part.cylinder.depth;
                    return bounds;

                }
            }
            bounds.X = 1;
            bounds.Y = 1;
            bounds.Z = 1;
            MessageBox.Show("no bounds\n\nImpossible senario. please report");
            return bounds;
        }
        //Reorganize bounds according to rotation:
        private dynamic flip(dynamic child, string neworder)
        {
            int x = child.bounds.x;
            int y = child.bounds.y;
            int z = child.bounds.z;
            if (neworder == "zxy") //shift
            {
                child.bounds.x = z;
                child.bounds.y = x;
                child.bounds.z = y;
            }
            if (neworder == "yzx") //"
            {
                child.bounds.x = y;
                child.bounds.y = z;
                child.bounds.z = x;
            }
            if (neworder == "xzy") //switch
            {
                child.bounds.x = x;
                child.bounds.y = z;
                child.bounds.z = y;
            }
            if (neworder == "zyx")
            {
                child.bounds.x = z;
                child.bounds.y = y;
                child.bounds.z = x;
            }
            if (neworder == "yxz")
            {
                child.bounds.x = y;
                child.bounds.y = x;
                child.bounds.z = z;
            }
            return child;
        }
        //get new pos and correct ingame bounds for child
        private dynamic getposandbounds(dynamic whatever)
        {
            dynamic child = Newtonsoft.Json.JsonConvert.DeserializeObject(Convert.ToString(whatever));
            string uuid = child.shapeId;

            if (child.bounds == null) //add bounds to parts (blocks do not get affected)
            {

                foreach(connectable c in connectable_UUIDS)
                {
                    if (c.UUID.ToLower() == child.shapeId.Value)
                    {
                        child.bounds = c.bounds;
                    }
                }
                //switch bounds here
                if (Math.Abs(Convert.ToInt32(child.xaxis)) == 3)
                {
                    if (Math.Abs(Convert.ToInt32(child.zaxis)) == 2)
                    {
                        flip(child, "yzx");
                    }
                    if (Math.Abs(Convert.ToInt32(child.zaxis)) == 1)
                    {
                        flip(child, "zyx");
                    }
                }
                if (Math.Abs(Convert.ToInt32(child.xaxis)) == 2)
                {
                    if (Math.Abs(Convert.ToInt32(child.zaxis)) == 3)
                    {
                        flip(child, "yxz");
                    }
                    if (Math.Abs(Convert.ToInt32(child.zaxis)) == 1)
                    {
                        flip(child, "zxy");
                    }
                }
                if (Math.Abs(Convert.ToInt32(child.xaxis)) == 1)
                {
                    if (Math.Abs(Convert.ToInt32(child.zaxis)) == 2)
                    {
                        flip(child, "xzy");
                    }
                }
            }
            //this updating pos only applies to parts, blocks do not get affected as they always have xaxis 1 zaxis 3
            if (child.xaxis == -1 | child.zaxis == -1 | (child.xaxis == 2 && child.zaxis == 3) | (child.xaxis == 3 && child.zaxis == -2) | (child.xaxis == -2 && child.zaxis == -3) | (child.xaxis == -3 && child.zaxis == 2))
                child.pos.x -= child.bounds.x;
            if (child.xaxis == -2 | child.zaxis == -2 | (child.xaxis == -1 && child.zaxis == 3) | (child.xaxis == -3 && child.zaxis == -1) | (child.xaxis == 1 && child.zaxis == -3) | (child.xaxis == 3 && child.zaxis == 1))
                child.pos.y -= child.bounds.y;
            if (child.xaxis == -3 | child.zaxis == -3 | (child.xaxis == -2 && child.zaxis == 1) | (child.xaxis == -1 && child.zaxis == -2) | (child.xaxis == 1 && child.zaxis == 2) | (child.xaxis == 2 && child.zaxis == -1))
                child.pos.z -= child.bounds.z;
            return child;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://youtu.be/glLgQemUS2I?t=270");
        }//help

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            try
            {
                if (Convert.ToInt32(t.Text) >= 0 || Convert.ToInt32(t.Text) <= 0) { }
            }
            catch
            {
                t.Text = "0";
            }
        }


        //
    }
    public class connectable
    {
        public string name = "unnamed block";
        public string UUID;
        public dynamic bounds;

        public connectable(string UUID, dynamic bounds)
        {
            this.UUID = UUID;
            this.bounds = bounds;
        }
        public connectable(string UUID, dynamic bounds, string name)
        {
            this.UUID = UUID;
            this.bounds = bounds;
            this.name = name;
        }
    }
    

}
