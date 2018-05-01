using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        
        MainWindow mainwindow;
        private dynamic uuidsbackup;

        List<Connectable> connectable_UUIDS = new List<Connectable>();
        //List<Item> ItemList;
        public AdvancedConnections(MainWindow window)
        {
            this.mainwindow = window;
            InitializeComponent();
            this.Update();
        }

        public void Update()
        {
            if(uuidsbackup != BP.Useduuids)
            {
                connectable_UUIDS.Clear();
                foreach (string uuid in BP.Useduuids)
                {
                    if (Database.blocks.ContainsKey(uuid) && Database.blocks[uuid] is Part && (Database.blocks[uuid] as Part).IsConnectable)
                    {
                        connectable_UUIDS.Add(new Connectable(uuid.ToString(), ((Part)Database.blocks[uuid]).GetBoundsDynamic(), Database.blocks[uuid].Name));

                    }
                }
                this.Dispatcher.Invoke((Action)(() =>
                {//this refer to form in WPF application 
                    comboBox_items1.Items.Clear();
                    comboBox_items2.Items.Clear();
                }));
                foreach (Connectable i in connectable_UUIDS)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                        comboBox_items1.Items.Add(i.name);
                        comboBox_items2.Items.Add(i.name);
                    }));
                }
            }
            uuidsbackup = BP.Useduuids;
        }

        

        private void TextBox_color_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Update();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Update();
            PaintSelector p = new PaintSelector
            {
                Owner = this.mainwindow
            };
            p.Show();
        }
        

        private dynamic WireIt(dynamic blueprint, string sourcecolor, string destinationcolor, string sourcetype, string destinationtype, int offsetx, int offsety, int offsetz, string offsetcolor )
        {

            //dynamic block = new JObject { [x] = new JObject { [y] = new JObject { [z] = new JObject { [child.color.ToString()] = new JObject { [child.shapeId.ToString()] = new JObject { [child.color] = "test" } } } } } };
            //merge
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.scrapshitConnectionString))
            {
                //conn.ConnectionString = "MyDB";c
                int i = 0;
                conn.Open();
                foreach (dynamic body in blueprint.bodies)
                    foreach (dynamic child in body.childs)
                    {
                        if(child.controller != null)
                        {
                            dynamic rpos = BP.getposandbounds(child);
                            string x = rpos.pos.x.ToString();
                            string y = rpos.pos.y.ToString();
                            string z = rpos.pos.z.ToString();
                            string color = child.color.ToString();
                            if (color.StartsWith("#"))
                                color = color.Substring(1, 6);

                            using (SqlCommand insertCommand = new SqlCommand("INSERT INTO Connections (Controller_Id, posx, posy, posz, color, uuid) VALUES (@controllerid, @x, @y, @z, @color, @uuid)", conn))
                            {
                                insertCommand.Parameters.AddWithValue("@controllerid", Convert.ToInt32(child.controller.id.ToString()));
                                insertCommand.Parameters.AddWithValue("@x", x);
                                insertCommand.Parameters.AddWithValue("@y", y);
                                insertCommand.Parameters.AddWithValue("@z", z);
                                insertCommand.Parameters.AddWithValue("@color", color);
                                insertCommand.Parameters.AddWithValue("@uuid", child.shapeId.ToString());
                                try
                                {
                                    i+= insertCommand.ExecuteNonQuery();
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message);
                                }

                            }
                        }
                    }



                //SqlCommand cmd = new SqlCommand("SELECT * FROM Description WHERE login = @login", conn);


                //all with uuid x / other / all other (match offset block color)
                //all with color / all other  / all other (match offset block color)
                //all with x + offsetx / other(all in this dir) /x
                //all with y + offsety / other / y
                //all with z + offsetz /other / z
                //exclude self wire?
                //if match offset block color search offset from  x y z found blocks (from all, check all current found + offset with color x and type x)


            }



            //<x, y, z, color, shapeId>, controllerid
            Dictionary<Tuple<int, int, int, string, string>, int> dict = new Dictionary<Tuple<int, int, int, string, string>, int>();

            Dictionary<int, Dictionary<int, Dictionary<int, int>>> testdict = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
            //testdict[5][2][3] = 5;
            



            return blueprint;
        }



        //Wire it!
        private void Button1_Click(object sender, RoutedEventArgs e)
        {//WIRE IT!
            if(comboBox_items1.SelectedIndex!=-1 && comboBox_items2.SelectedIndex != -1 && BP.Blueprint != null)
            {
                string sourcecolor = textBox_color1.Text;
                string destinationcolor = textBox_color2.Text;
                Connectable sourceblock = connectable_UUIDS[comboBox_items1.SelectedIndex];
                Connectable destinationblock = connectable_UUIDS[comboBox_items2.SelectedIndex];

                int offsetx = Convert.ToInt32(textBox_X.Text);
                int offsety = Convert.ToInt32(textBox_Y.Text);
                int offsetz = Convert.ToInt32(textBox_Z.Text);
                
                dynamic result = WireIt(BP.Blueprint, sourcecolor, destinationcolor, sourceblock.UUID, destinationblock.UUID, offsetx, offsety, offsetz, destinationcolor);


                dynamic backupbp = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BP.Blueprint.ToString());
                //list all destionationblocks and their ID's
                dynamic destids = new JObject();
                int minx = 10000, maxx = -10000, miny = 10000, maxy = -10000, minz = 10000, maxz = -10000;
                //loop over all blocks:
                foreach (dynamic body in BP.Blueprint.bodies)
                {
                    foreach(dynamic child in body.childs)
                    {
                        if(child.shapeId.Value.ToLower() == destinationblock.UUID.ToLower() /*&& (child.color.Value.ToLower() == destinationcolor.ToLower() || "#"+child.color.Value.ToLower() == destinationcolor.ToLower() || destinationcolor == "" || destinationcolor == "#")*/)
                        {
                            dynamic dest = BP.getposandbounds(child);//outputs corrected child (default rotation, correct position)

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
                if(BP.Blueprint.joints != null)
                foreach(dynamic child in BP.Blueprint.joints)
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

                    foreach(dynamic body in BP.Blueprint.bodies)
                        foreach(dynamic child in body.childs)
                            if (child.shapeId.Value.ToLower() == sourceblock.UUID.ToLower() && (child.color.Value.ToLower() == sourcecolor.ToLower() || "#" + child.color.Value.ToLower() == sourcecolor.ToLower() || sourcecolor == "" || sourcecolor == "#"))
                            {//COLOR AND UUID CORRECT, FURTHER WIRING PROCESS:
                                dynamic source = BP.getposandbounds(child);//outputs corrected child (default rotation, correct position)

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

                                string color = child.color.ToString();
                                if (color.StartsWith("#"))
                                    color.Substring(1, 6);
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

                                mainwindow.Image_blueprint.DataContext = null;
                                mainwindow.Image_blueprint.DataContext = mainwindow;
                            }

                    new System.Threading.Thread(new System.Threading.ThreadStart(() => {
                        if(Properties.Settings.Default.wires)
                            MessageBox.Show("Successfully made " + amountwired + " connections! :D\n\n'Wires'-feature will render wires upon load from openwindow->needs work");
                        else
                            MessageBox.Show("Successfully made " + amountwired + " connections! :D");
                    })).Start();
                    
                    //window.UpdateOpenedBlueprint();
                    if (amountwired>0)
                        BP.Description.description = BP.Description.description + "\n--> " + amountwired + " connections made between " + sourcecolor + " " + sourceblock.name + " and " + destinationcolor + " " + destinationblock.name;
                   
                }
                catch(Exception exception)
                {
                    MessageBox.Show(exception.Message+"\n\n Blueprint connections abrupted\nrestoring blueprint","ERROR");
                    BP.setblueprint(backupbp);
                }
            }
            else
            {
                MessageBox.Show("Something went wrong!\nno harm done");
            }




        }

        

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://youtu.be/glLgQemUS2I?t=270");
        }//help

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
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
    public class Connectable
    {
        public string name = "unnamed block";
        public string UUID;
        public dynamic bounds;

        public Connectable(string UUID, dynamic bounds)
        {
            this.UUID = UUID;
            this.bounds = bounds;
        }
        public Connectable(string UUID, dynamic bounds, string name)
        {
            this.UUID = UUID;
            this.bounds = bounds;
            this.name = name;
        }
    }
    

}
