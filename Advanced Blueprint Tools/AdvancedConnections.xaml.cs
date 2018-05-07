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

        HashSet<Connectable> connectable_UUIDS = new HashSet<Connectable>();
        //List<Item> ItemList;
        public AdvancedConnections(MainWindow window)
        {
            this.mainwindow = window;
            InitializeComponent();
            this.Update();
        }

        public void Update()
        {
            if(uuidsbackup != BP.GetUsedUuids())
            {
                connectable_UUIDS.Clear();
                foreach (string uuid in BP.GetUsedUuids())
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
                        comboBox_items1.Items.Add(i);
                        comboBox_items2.Items.Add(i);
                    }));
                }
            }
            uuidsbackup = BP.GetUsedUuids(); 
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
            this.mainwindow.openpaintpicker();
            textBox_color1.Text = PaintSelector.PaintColor;
        }

        private void Button3_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.mainwindow.openpaintpicker();
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
        
        public struct Controller
        {
            public int id;
            public int x;
            public int y;
            public int z;
            public string color;
            public string uuid;
            public List<int> connections;
        }

        private void WireIt(dynamic a, string sourcecolor, string destinationcolor, string sourcetype, string destinationtype, int offsetx, int offsety, int offsetz, string offsetcolor )
        {
            
            Dictionary<int,Controller> controllers = new Dictionary<int, Controller>();
            bool dirx = checkBox_X.IsChecked == true;
            bool diry = checkBox_Y.IsChecked == true;
            bool dirz = checkBox_Z.IsChecked == true;
            bool self = checkBox_0.IsChecked == true;
            bool matchoffset = MatchOffset.IsChecked == true; //safemode enable?

            Dictionary<string, List<int>> filtercrap = new Dictionary<string, List<int>>();//can be glitchwelded
            //x.y.z = int

            foreach (dynamic body in BP.Blueprint.bodies)
                foreach (dynamic child in body.childs)
                    if (child.controller != null)
                    {
                        dynamic rpos = BP.getposandbounds(child);
                        int x = Convert.ToInt32(rpos.pos.x.ToString());
                        int y = Convert.ToInt32(rpos.pos.y.ToString());
                        int z = Convert.ToInt32(rpos.pos.z.ToString());
                        string color = child.color.ToString().ToLower();
                        if (color.StartsWith("#"))
                            color = color.Substring(1, 6);
                        int id = Convert.ToInt32(child.controller.id.ToString());
                        controllers.Add(id,new Controller {id= id, x = x, y = y, z = z, color = color, uuid = child.shapeId.ToString(), connections = new List<int>() });

                        string key = x.ToString() + "." + y.ToString() + "." + z.ToString();
                        if (!filtercrap.ContainsKey(key))
                            filtercrap.Add(key, new List<int> { id });
                        else
                            filtercrap[key].Add(id);
                    }

            if(matchoffset)
            {//filter only the block with color,type,offset from source
                foreach (Controller source in controllers.Values)
                    if ((sourcecolor == null || sourcecolor == source.color) && (sourcetype == source.uuid))
                        foreach (Controller destination in controllers.Values)
                            if((destinationcolor == null || destinationcolor == destination.color) && ( destinationtype == null || destinationtype == destination.uuid) && 
                                (self || !source.Equals(destination)) &&
                                (dirx || source.x+offsetx == destination.x) && (diry || source.y + offsety == destination.y) && (dirz || source.z + offsetz == destination.z))
                            {
                                source.connections.Add(destination.id);
                            }
            }
            else//filter all blocks no offset with col&type, then get the ones offset from them (any color, any type)
                foreach (Controller source in controllers.Values)
                    if ((sourcecolor == null || sourcecolor == source.color) && (sourcetype == source.uuid))
                        foreach (Controller destination in controllers.Values)
                            if ((destinationcolor == null || destinationcolor == destination.color) && (destinationtype == null || destinationtype == destination.uuid) &&
                                (self || !source.Equals(destination)) &&
                                (dirx || source.x == destination.x) && (diry || source.y == destination.y) && (dirz || source.z == destination.z))
                            {
                                string key = (offsetx+ destination.x).ToString() + "." + (offsety + destination.y).ToString() + "." + (offsetz + destination.z).ToString();
                                //get block offset from destination 
                                if (filtercrap.ContainsKey(key))
                                    source.connections.AddRange(filtercrap[key]);
                                //source.connections.Add(destination.id);
                            }

            //apply controllers to bp:

            int amountwired = 0;
            foreach (dynamic body in BP.Blueprint.bodies)
                foreach (dynamic child in body.childs)
                    if (child.controller != null && controllers.ContainsKey(Convert.ToInt32(child.controller.id.ToString())))
                    {
                        int id = Convert.ToInt32(child.controller.id.ToString());
                        if (child.controller.controllers == null)
                            child.controller.controllers = new JArray();
                        if(controllers[id].connections != null)
                            foreach (int destid in controllers[id].connections)
                            {
                                child.controller.controllers.Add(new JObject { ["id"] = destid });
                                amountwired++;
                            }
                    }

            new System.Threading.Thread(new System.Threading.ThreadStart(() => {
                    MessageBox.Show("Successfully made " + amountwired + " connections! :D");
            })).Start();

            if (amountwired > 0)
            {
                mainwindow.TextBox_Description.Text += "\n--> " + amountwired + " connections made\n";
                BP.Description.description = BP.Description.description + "\n--> " + amountwired + " connections made\n";
            }

            mainwindow.RenderWires();
            #region somecode
            /*
            dynamic controllersold = new JObject();

            dynamic sources = new JObject();

            foreach (dynamic body in blueprint.bodies)
                foreach (dynamic child in body.childs)
                {
                    if (child.controller != null)
                    {
                        dynamic rpos = BP.getposandbounds(child);
                        string x = rpos.pos.x.ToString();
                        string y = rpos.pos.y.ToString();
                        string z = rpos.pos.z.ToString();
                        string color = child.color.ToString();
                        if (color.StartsWith("#"))
                            color = color.Substring(1, 6);
                        //child.shapeId.ToString()
                        // Convert.ToInt32(child.controller.id.ToString())
                        dynamic controller = new JObject { [x] = new JObject { [y] = new JObject { [z] = new JObject { [child.color.ToString()] = new JObject { [child.shapeId.ToString()] = new JObject { [color] = Convert.ToInt32(child.controller.id.ToString()) } } } } } };
                        controllersold.Merge(controller);
                        if ((sourcecolor == null || sourcecolor == color) && (sourcetype == null || sourcetype == child.shapeId.ToString()))
                            sources.Merge(controller);
                    }
                }


            dynamic result = new JObject();//result.DeepClone();
            //Newtonsoft.Json.Linq.JProperty // keypair

            if (matchoffset) //ON
            {//filter only the block with color,type,offset from source
                dynamic searchfrom = controllersold.DeepClone();
                if (dirx)
                {
                    result = searchfrom;
                }
                else
                {
                    foreach(JProperty Jprop in sources) //Jprop.Name = "X"
                    {
                        string coord = (Convert.ToInt32(Jprop.Name) + offsetx).ToString();
                        if (searchfrom[coord] != null)
                        {
                            result.Merge(new JObject { [coord] = searchfrom[coord] });
                        }
                    }
                }

                searchfrom = result.DeepClone();
                result = new JObject();
                if (diry)
                {
                    result = searchfrom;
                }
                else
                {
                    foreach (JProperty JpropSource in sources) 
                    {
                        string coord = (Convert.ToInt32(JpropSource.Name) + offsetx).ToString();
                        if (searchfrom[coord] != null)
                        {
                            result.Merge(new JObject { [coord] = searchfrom[coord] });
                        }
                    }
                }
            }
            else
            {//filter all blocks no offset with col&type, then get the ones offset from them (any color, any type)

            }


            foreach (dynamic body in blueprint.bodies)
                foreach(dynamic child in body.childs)
                {
                    if(child.controller != null && (sourcetype == null || sourcetype == child.shapeId.ToString()))
                    {
                        string color = child.color.ToString(); if (color.StartsWith("#")) color = color.Substring(1, 6);
                        if(sourcecolor == null || color == sourcecolor)
                        {
                            dynamic rpos = BP.getposandbounds(child);
                            int x = Convert.ToInt32(rpos.pos.x.ToString());
                            int y = Convert.ToInt32(rpos.pos.y.ToString());
                            int z = Convert.ToInt32(rpos.pos.z.ToString());



                        }
                    }
                }




            if (blueprint == "")//false
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.scrapshitConnectionString))
            {
                //conn.ConnectionString = "MyDB";c
                int i = 0;
                conn.Open();
                using (SqlCommand insertCommand = new SqlCommand("DELETE FROM Connections", conn))
                {
                    insertCommand.ExecuteNonQuery();
                }
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



                //SqlCommand cmd = new SqlCommand("SELECT * FROM Connections WHERE login = @login", conn);

                //find all sources  "SELECT * AS 'Sources' FROM Connections" + "WHERE color = @sourcecolor AND uuid = @sourceuuid"
                //find all dests?


                //string cmd = "SELECT * FROM Description"
                //cmd+= "Where uuid = @uuid"
                //cmd+= @"AND color = @color"

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

            */
            #endregion
        }



        //Wire it!
        private void Button1_Click(object sender, RoutedEventArgs e)
        {//WIRE IT!
            if(comboBox_items1.SelectedIndex!=-1 && comboBox_items2.SelectedIndex != -1 && BP.Blueprint != null)
            {
                dynamic backupbp = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(BP.Blueprint.ToString());
                string sourcecolor = textBox_color1.Text.ToLower();
                string destinationcolor = textBox_color2.Text.ToLower();
                Connectable sourceblock = (Connectable) comboBox_items1.SelectedItem;
                Connectable destinationblock = (Connectable) comboBox_items2.SelectedItem;

                int offsetx = Convert.ToInt32(textBox_X.Text);
                int offsety = Convert.ToInt32(textBox_Y.Text);
                int offsetz = Convert.ToInt32(textBox_Z.Text);

                if (sourcecolor.Length == 7)
                    sourcecolor = sourcecolor.Substring(1, 6);
                else
                    sourcecolor = null;
                if (destinationcolor.Length == 7)
                    destinationcolor = destinationcolor.Substring(1, 6);
                else
                    destinationcolor = null;
                //try
                {
                    WireIt(null, sourcecolor, destinationcolor, sourceblock.UUID, destinationblock.UUID, offsetx, offsety, offsetz, destinationcolor);
                }
                //catch (Exception ex)
                {
                  //  MessageBox.Show(ex.Message + "\n\n Blueprint connections abrupted\nrestoring blueprint", "ERROR");
                    //BP.setblueprint(backupbp);
                }
                
                if(false)
                {

                #region oldcode
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
                    #endregion

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

        public override string ToString()
        {
            return name;
        }
    }
    

}
