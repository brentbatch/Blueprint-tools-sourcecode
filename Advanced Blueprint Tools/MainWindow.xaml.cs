using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Assimp;
using Assimp.Unmanaged;


namespace Advanced_Blueprint_Tools
{
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using HelixToolkit.Wpf;
    using Assimp.Configs;
    using System.Net.Http;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public BP OpenedBlueprint { get; set; }
        public Model3DGroup Model { get; set; }
        public Model3DGroup Marker { get; set; }
        public Model3DGroup Marker2 { get; set; }
        public Model3DGroup Glass { get; set; }

        public Model3DGroup Wires { get; set; }

        OpenWindow openwindow;
        //public cuz need to update(); when new creation loaded: 
        public AdvancedConnections advancedconnections;
        public AdvancedColor advancedcolorwindow;
        public SwapBlocksWindow swapblockswindow;
        public BlockProperties blockProperties;
        public AreaProperties areaProperties;
        public BlockPropertiesRAW blockPropertiesRAW;

        Circle_generator circle_generator;
        Ellipsoid_Generator ellpisoid_generator;
        Cuboid_Generator cuboid_Generator;

        NotificationWindow notificationWindow;
        Settings settings;

        public MainWindow()
        {

            new Thread(() =>
            {
                MessageBox.Show("This version of the tool should work with scrap mechanic 0.4.5, if for any reason this breaks or you found a bug: contact Brent Batch#9261");
            }).Start();
            InitializeComponent();
            //LOAD RESOURCES:
            Database.findPaths();
            new Thread(() =>
            {
                try
                {
                    Database.LoadAllBlocks();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error: Could not load parts. Further program functions may fail.\n" + e.Message);
                }
            }).Start();
            new Thread(() =>
            {
                try
                { 
                    Database.LoadAllBlueprints();
                }
                    catch (Exception e)
                {
                    MessageBox.Show("Error: Could not load Blueprints. Further program functions may fail.\n" + e.Message);
                }
            }).Start();
            new Task(() =>
            {
                try
                { 
                    new Updater().CheckForUpdates();
                }
                    catch (Exception e)
                {
                    MessageBox.Show("Error: could not check for an updated version.\n" + e.Message);
                }
            }).Start();
            new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            if (Database.Notifications.Count > 0)
                            {
                                Notifications.Visibility = Visibility.Visible;
                                Notifications.Content = Database.Notifications.Count;
                            }
                            else
                            {
                                Notifications.Visibility = Visibility.Collapsed;
                            }
                        }));
                        Thread.Sleep(1500);
                    }
                }
                catch { }

            }).Start();
            helix.Camera = helix_wires.Camera;

        } 

        public void RenderBlueprint()
        {
            
            try
            {
                TextBox_Name.Text = BP.Description.name;
                TextBox_Description.Text = BP.Description.description;

                Tuple<Model3DGroup, Model3DGroup> renders = BP.RenderBlocks();//backgroundtask ?
                this.Model = renders.Item1;
                this.Glass = renders.Item2;
                this.Marker = null;
                CenterMass.Content = null;
                this.Marker2 = new Model3DGroup();

                RenderWires();
                if (listBox_parts.Visibility == Visibility.Visible)
                    fillparts();
                Image_blueprint.DataContext = null;
                Image_blueprint.DataContext = this;
                helix.ResetCamera();
                helix_wires.Camera = helix.Camera;
                //Model.Children[5].Transform = new ScaleTransform3D(2,2,2);

                if (connections.IsEnabled == false)
                {
                    connections.IsEnabled = true;
                    blockproperties.IsEnabled = true;
                    paint.IsEnabled = true;
                    fixcreation.IsEnabled = true;
                    areaproperties.IsEnabled = true;
                    swapblocks.IsEnabled = true;
                    paintpicker.IsEnabled = true;
                    mirrorcreation.IsEnabled = true;
                    requiredmods.IsEnabled = true;
                }
                if (!Properties.Settings.Default.safemode)
                    blockpropertiesRAW.IsEnabled = true;
                else
                    blockpropertiesRAW.IsEnabled = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\nin most cases the creation should be fine, something just went wrong while rendering", "failed to render");
            }
            
        }

        public void RenderWires()
        {
            this.Wires = new Model3DGroup();
            if (Properties.Settings.Default.wires)
            {
                var wires = BP.GetWires();
                foreach (dynamic wire in wires.Values)
                {
                    Addblob(wire.pos, wire.color.ToString());
                    if (wire.connections != null)
                        foreach (dynamic connid in wire.connections)
                            if (wires.ContainsKey(Convert.ToInt32(connid.id.ToString())))
                            {
                                Addwire(wire.pos, wires[Convert.ToInt32(connid.id.ToString())].pos, wire.color.ToString());
                            }
                }
                Image_blueprint.DataContext = null;
                Image_blueprint.DataContext = this;
                }

        }
        private void ToggleWires_Click(object sender, RoutedEventArgs e)
        {
            if (BP.Blueprint != null)
            {
                Properties.Settings.Default.wires = !Properties.Settings.Default.wires;
                if (Properties.Settings.Default.wires)
                    RenderWires();
                else
                {
                    this.Wires = null;
                    Image_blueprint.DataContext = null;
                    Image_blueprint.DataContext = this;
                }

            }

        }


        private void CenterMass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CenterMass.Content == null)
                {
                    Point3D point = BP.GetCenterOffMass().Item1;

                    Material material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255,230,30,30)));
                    var meshBuilder = new MeshBuilder(false, false);

                    meshBuilder.AddSphere(point, 0.3);
                    var Mesh = meshBuilder.ToMesh(true);
                    CenterMass.Content = new GeometryModel3D { Geometry = Mesh, Material = material };
                }
                else
                    CenterMass.Content = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Addblob(dynamic pos, string color)
        {
            try
            {
                if (!Properties.Settings.Default.colorwires)
                    color = Properties.Settings.Default.blobcolor.Substring(1,6);

                Material material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(Properties.Settings.Default.coloropacity,
                            Convert.ToByte(color.Substring(0, 2), 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16))));
                var meshBuilder = new MeshBuilder(false, false);

                meshBuilder.AddSphere(new Point3D(Convert.ToDouble(pos.x.ToString()) ,
                                                Convert.ToDouble(pos.y.ToString()) ,
                                                Convert.ToDouble(pos.z.ToString()) ), 0.25);
                var Mesh = meshBuilder.ToMesh(true);
                this.Wires.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "wire render failed");
            }
        }

        public void Addwire(dynamic pos1, dynamic pos2, string color)
        {
            try
            {
                if (!Properties.Settings.Default.colorwires)
                    color = Properties.Settings.Default.wirecolor.Substring(1, 6);
                Material material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(Properties.Settings.Default.coloropacity,
                        Convert.ToByte(color.Substring(0, 2), 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16))));

                var meshBuilder = new MeshBuilder(false, false);

                meshBuilder.AddArrow(new Point3D(Convert.ToDouble(pos1.x.ToString()),
                                                Convert.ToDouble(pos1.y.ToString()) ,
                                                Convert.ToDouble(pos1.z.ToString()) ),
                                    new Point3D(Convert.ToDouble(pos2.x.ToString()) ,
                                                Convert.ToDouble(pos2.y.ToString()) ,
                                                Convert.ToDouble(pos2.z.ToString()) ),
                                    0.15, 4);
                var Mesh = meshBuilder.ToMesh(true);
                this.Wires.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "wire render failed");
            }
        }

        public void setMarker(double x, double y, double z)
        {//cross marker
            Model3DGroup marker = new Model3DGroup();
            Material material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(100, 51, 204, 51)));
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(x, y, z), 0.2, 0.2, 1000);
            meshBuilder.AddBox(new Point3D(x, y, z), 0.2, 1000, 0.2);
            meshBuilder.AddBox(new Point3D(x, y, z), 1000, 0.2, 0.2);
            // Create a mesh from the builder (and freeze it)
            var Mesh = meshBuilder.ToMesh(true);
            
            marker.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });
            this.Marker = marker;
            
          
            Image_blueprint.DataContext = "";
            Image_blueprint.DataContext = this;
            helix_wires.Camera.LookAt(new Point3D(x, y, z), 1000);
            //helix.Camera = helix_wires.Camera;
        }
        public void setMarker2(double x, double y, double z, double boundsx, double boundsy, double boundsz)
        {//cuboid marker
            Model3DGroup marker = new Model3DGroup();
            Material material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(50, 20, 50, 50)));
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(x, y, z), boundsx+0.1, boundsy+0.1, boundsz+0.1);
            // Create a mesh from the builder (and freeze it)
            var Mesh = meshBuilder.ToMesh(true);
            marker.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });
            this.Marker2 = marker;
            Image_blueprint.DataContext = "";
            Image_blueprint.DataContext = this;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (openwindow != null && openwindow.IsLoaded)
            {
                openwindow.Focus();
            }
            else
            {
                openwindow = new OpenWindow(this);
                openwindow.Owner = this;
                openwindow.Show();
            }
            openwindow.TextBox_Search.Focus();
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)//possible to save/saveas?
        {
            e.CanExecute = false;
            if (BP.Blueprint != null) e.CanExecute = true;
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            button_overwrite_click(sender, e);
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            button_save_click(sender, e);
        }


        private void button_overwrite_click(object sender, RoutedEventArgs e)//save/overwrite
        {
            if (BP.Blueprint != null)
            {
                newtumbnail();
                BP.Description.name = TextBox_Name.Text;
                BP.Description.description = TextBox_Description.Text;
                BP.Save();
            }
        }
        private void button_save_click(object sender, RoutedEventArgs e)//save as
        {
            if (BP.Blueprint != null)
            {
                newtumbnail();
                BP.Description.name = TextBox_Name.Text;
                BP.Description.description = TextBox_Description.Text;
                Random r = new Random(); //GENERATE NEW UUID AND CHANGE DESCRIPTION LOCALID --todo
                BP.SaveAs("Blueprint" + r.Next() + "-" + r.Next());
            }
        }
        
        private void newtumbnail()
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap(500, 500, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(helix);
            Clipboard.SetImage(bmp);
            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bmp));
            BP.Icon = png;
        }


        private void Click_advancedwiring(object sender, RoutedEventArgs e)
        {
            if (advancedconnections != null) advancedconnections.Close();
            advancedconnections = new AdvancedConnections(this);
            advancedconnections.Owner = this;
            advancedconnections.Show();
        }

        private void Click_blockproperties(object sender, RoutedEventArgs e)
        {
            if (blockProperties != null) blockProperties.Close();
            blockProperties = new BlockProperties(this);
            blockProperties.Owner = this;
            blockProperties.Show();
        }

        private void Click_areaproperties(object sender, RoutedEventArgs e)
        {
            if (areaProperties != null) areaProperties.Close();
            areaProperties = new AreaProperties(this);
            areaProperties.Owner = this;
            areaProperties.Show();
        }
        private void Click_blockpropertiesRAW(object sender, RoutedEventArgs e)
        {
            if (blockPropertiesRAW != null) blockPropertiesRAW.Close();
            blockPropertiesRAW = new BlockPropertiesRAW(this);
            blockPropertiesRAW.Owner = this;
            blockPropertiesRAW.Show();
        }

        private void Click_advancedcolor(object sender, RoutedEventArgs e)
        {//paint tool
            if (advancedcolorwindow != null) advancedcolorwindow.Close();
            advancedcolorwindow = new AdvancedColor(this);
            advancedcolorwindow.Owner = this;
            advancedcolorwindow.Show();
        }
        
        private void Click_fixcreation(object sender, RoutedEventArgs e)
        {
            dynamic blueprint = new JObject();

            blueprint.bodies = new JArray();
            blueprint.version = 1;
            blueprint.bodies.Add(new JObject());
            blueprint.bodies[0].childs = new JArray();

            //BP.Blueprint.joints = null;
            foreach (dynamic body in BP.Blueprint.bodies)
            {
                //remove bearings/springs/pistons
                foreach (dynamic child in body.childs)
                {
                    if (child.joints != null)
                        child.joints = null;
                    if (child.controller != null)
                    {
                        if (child.controller.joints != null)
                            child.controller.joints = null;
                        if (child.controller.controllers != null)
                            child.controller.controllers = null;
                    }
                    blueprint.bodies[0].childs.Add(child);
                }
            }
            BP.Description.description = BP.Description.description + "\n++ removed joints & glitchwelded";
            BP.setblueprint(blueprint);
            this.RenderBlueprint();
        }
        
        private void Click_swapblocks(object sender, RoutedEventArgs e)
        {
            if (swapblockswindow != null) swapblockswindow.Close();
            swapblockswindow = new SwapBlocksWindow(this);
            swapblockswindow.Owner = this;
            swapblockswindow.Show();
        }


        public static PaintSelector paintSelector;
        private void Click_paintpicker(object sender, RoutedEventArgs e) //paint picker
        {
            {
                paintSelector = new PaintSelector();
                paintSelector.Owner = this;
                paintSelector.Show();
                //MainWindow.openpainpicker();
            }
        }
        public void openpaintpicker()//opens the paintpicker if mainwindow doesn't have one open
        {
            
            if (paintSelector == null || !(paintSelector.IsActive || paintSelector.IsFocused || paintSelector.IsVisible))
            {
                paintSelector = new PaintSelector();
                paintSelector.Owner = this; //static function :(
                paintSelector.Show();
            }
        }
        

        private void Click_mirrormode(object sender, RoutedEventArgs e)//needs work
          {
            dynamic blueprint = BP.Blueprint;
            if (blueprint.joints == null)
            {
                foreach (dynamic body in blueprint.bodies)
                    foreach (dynamic block in body.childs)
                    {
                        {
                            dynamic realpos = BP.getposandbounds(block);
                            
                            int xaxis = Convert.ToInt32(block.xaxis);
                            int zaxis = Convert.ToInt32(block.zaxis);
                            if (!((xaxis == 1 && zaxis == -2) || (Math.Abs(xaxis) == 1 && Math.Abs(zaxis) == 3) || (xaxis == -1 && zaxis == 2)))
                            {
                                realpos.xaxis = -xaxis;
                                realpos.zaxis = Math.Abs( zaxis)==1? -zaxis : zaxis;

                            }
                            //Bounds bounds = Blockobject.BoundsByRotation(new Bounds(realpos.bounds),1,3);
                            realpos.pos.x = -Convert.ToInt32(block.pos.x) - ((realpos.pos.x == block.pos.x)? Convert.ToInt32(realpos.bounds.x) :0);
                            //realpos.pos.y = Convert.ToInt32(block.pos.y) - Convert.ToInt32(block.bounds.y);

                            block.pos = BP.calcbppos(realpos).pos;
                            block.xaxis = BP.calcbppos(realpos).xaxis;
                            block.zaxis = BP.calcbppos(realpos).zaxis;

                            //block.pos.x = -Convert.ToInt32(block.pos.x);
                            //works thus far for blocks, not parts tho

                            /*
                            if(Math.Abs(Convert.ToInt32(block.zaxis)) == 3 || Math.Abs(Convert.ToInt32(block.zaxis)) == 2)
                            {
                                block.xaxis = -Convert.ToInt32(block.xaxis);
                            }
                            if(Math.Abs(Convert.ToInt32(block.zaxis)) == 1)
                            {
                                block.zaxis = -Convert.ToInt32(block.zaxis);
                            }*/
                            /*block.pos.x = -Convert.ToInt32(block.pos.x);
                            if(Convert.ToInt32(block.xaxis) == 1 || Convert.ToInt32(block.xaxis) == -1)
                                block.xaxis = -Convert.ToInt32(block.xaxis);
                            else
                                block.zaxis = -Convert.ToInt32(block.zaxis);*/
                        }
                    }
                BP.setblueprint(blueprint);
                this.RenderBlueprint();
            }
            else
                MessageBox.Show("Mirror mode can't mirror blueprints with joints inside yet!");
            MessageBox.Show("Mirror mode did it's best to mirror things though there may be some parts that didn't turn out great.");

        }

        private void Click_ellipsoidgenerator(object sender, RoutedEventArgs e)
        {
            if (ellpisoid_generator != null) ellpisoid_generator.Close();
            ellpisoid_generator = new Ellipsoid_Generator(this);
            ellpisoid_generator.Owner = this;
            ellpisoid_generator.Show();

        }

        private void circlegenerator_Click(object sender, RoutedEventArgs e)
        {
            if (circle_generator != null) circle_generator.Close();
            circle_generator = new Circle_generator(this);
            circle_generator.Owner = this;
            circle_generator.Show();
        }

        private void Click_cuboidgenerator(object sender, RoutedEventArgs e)
        {
            if (cuboid_Generator != null) cuboid_Generator.Close();
            cuboid_Generator = new Cuboid_Generator(this);
            cuboid_Generator.Owner = this;
            cuboid_Generator.Show();
        }

        private void Click_requiredmods(object sender, RoutedEventArgs e)
        {
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://scrapmechanic.xesau.eu/uuidservice/get_mods");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string postData = "";
            foreach(string uuid in BP.GetUsedUuids())
            {
                postData+="&uuid[]=" + uuid;
            }
            postData = postData.Substring(1);

            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = bytes.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            var result = reader.ReadToEnd();
            dynamic Mods = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);

            foreach (dynamic missingmod in Mods.missing)
            {
                if (Database.blocks.ContainsKey(missingmod.ToString()))
                {
                    string modid = System.IO.Path.GetFileName(Database.blocks[missingmod.ToString()].id.ToString());
                    dynamic mod = new JObject();
                    mod.name = Database.blocks[missingmod.ToString()].Mod;
                    mod.ok = "✔";
                    mod.author = "Unavailable, service couldn't find it";
                    mod.url = "http://steamcommunity.com/workshop/filedetails/?id=" + modid;

                    if(!((JArray)Mods.mods).Contains(mod))
                        Mods.mods.Add(mod);
                }
                else
                {
                    MessageBox.Show("couldn't find required mod for: " + missingmod.ToString());
                }
            }
            
            foreach (dynamic mod in Mods.mods)
            {
                string fileId = mod.url.ToString().Substring(51);
                if (Database.usedmods.ContainsKey(fileId))
                    mod.ok = "✔";
                else
                    if(mod.ok == null)
                        mod.ok = "X";
            }

            new required_mods(Mods).Show();

            stream.Dispose();
            reader.Dispose();
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            if (notificationWindow != null) notificationWindow.Close();
            notificationWindow = new NotificationWindow();
            notificationWindow.Owner = this;
            notificationWindow.Show();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (settings != null) settings.Close();
            settings = new Settings(this);
            settings.Owner = this;
            settings.Show();

        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Need help?","",MessageBoxButton.YesNo);   
            if(result == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start("https://discord.gg/HXFqUqF");
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("A tool that provides lots of awesome features for you to make blueprint editing fun and easy!\n" +
                "\nMade by Brent Batch (youtube.com/c.brentbatch)\n'Required Mods'-feature: help by Xesau/Aaron\n\n" +
                "And a BIG thanks to all the testers: @GamerGuy, @iceboundGlaceon, @lmaster, @the_killerbanana, @ThePiGuy24Gaming, @Remynem, @xXTBR and @zOmbie1919nl.\n\n" +
                "Version: " + Properties.Settings.Default.version,"About");
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(TextBox_Description.Text == "Easter Egg")
            MessageBox.Show(@"                 /  \" + "\n" +
@"               / _ ^\" + "\n" +
@"              |  /  \  |" + "\n" +
@"              ||      ||  _______" + "\n" +
@"              ||      ||  |\          \" + "\n" +
@"              ||      ||  ||\          \" + "\n" +
@"              ||      ||  ||  \        |" + "\n" +
@"              ||      ||  ||    \__/" + "\n" +
@"              ||      ||  ||      ||" + "\n" +
@"                \\_/  \_/  \_//" + "\n" +
@"              /     _         _     \" + "\n" +
@"            /                           \" + "\n" +
@"            |       O         O      |" + "\n" +
@"            |      \    ___    /      |" + "\n" +
@"          /          \  \_/  /         \" + "\n" +
@"        /      -----    |    --\        \" + "\n" +
@"        |           \__/|\__/  \       |" + "\n" +
@"        \              |_|_|              /" + "\n" +
@"          \_____              _____/" + "\n" +
@"                      \          /" + "\n" +
@"                      |          |","Easter Egg!");
        }

        private void ObjToBlueprint_Click(object sender, RoutedEventArgs e)
        {
            new ObjToBlueprint(this) { Owner = this }.Show();
        }

        private void PixelArt_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("coming soon!","create 2d/3d pixelart, be able to import png");
        }

        private void Enable_Mode_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("coming soon!","Enable Mods, make certain mods invisible in features");
        }

        private void logicgenerator_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("coming soon!","Logic Diagram to Blueprint convertor");
        }

        private void midiconvertor_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("coming soon!", "import MIDI track, convert to blueprint");
        }

        private void mergecreation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("coming soon!", "hold creation in ram, import new, merge");
        }

        private void gif3d_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("coming soon!", "import multiple blueprints, craft a 3dgif with them");
        }

        private void dupecreation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("coming soon!", "stack/dupe creation in x/y/z");
        }

        private void listBox_parts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void label_desc_Click(object sender, RoutedEventArgs e)
        {

            var bc = new BrushConverter();
            label_desc.Background = (Brush)bc.ConvertFrom("#FFFBD300");
            label_parts.Background = (Brush)bc.ConvertFrom("#FF61737C");
            TextBox_Description.Visibility = Visibility.Visible;
            listBox_parts.Visibility = Visibility.Hidden;
        }

        private void label_parts_Click(object sender, RoutedEventArgs e)
        {
            //thread:
            var bc = new BrushConverter();
            label_desc.Background = (Brush)bc.ConvertFrom("#FF61737C");
            label_parts.Background = (Brush)bc.ConvertFrom("#FFFBD300");
            TextBox_Description.Visibility = Visibility.Hidden;
            listBox_parts.Visibility = Visibility.Visible;
            fillparts();
        }
        Dictionary<string, Tuple<string, BitmapSource, int>> parts;
        public void fillparts()
        {
            var parts = BP.GetUsedPartsList();
            if (parts != null)
                if (!parts.Equals(this.parts) || true)
                {
                    new Thread(() =>
                    {
                        List<Part_listitem> part_Listitems = new List<Part_listitem>();
                        foreach (var uuid in parts.Keys)
                        {
                            part_Listitems.Add(new Part_listitem()
                            { name = parts[uuid].Item1, amount = parts[uuid].Item3.ToString(), uuid = uuid});
                        }
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            //listBox_parts.ItemsSource = null;
                            listBox_parts.ItemsSource = part_Listitems;
                        }));
                        ImageToBitmapSourceConverter converter = new ImageToBitmapSourceConverter();
                        foreach (Part_listitem item in part_Listitems)
                        {
                            var bmp = Database.blocks[item.uuid].GetIcon(item.uuid);
                            if(bmp != null)
                            {
                                item.icon = (BitmapSource)converter.Convert(bmp.Clone(),null, typeof(BitmapSource),null);
                                item.emptysurface = 0;
                                item.iconheight = 100;
                            }
                        }
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            listBox_parts.ItemsSource = null;
                            listBox_parts.ItemsSource = part_Listitems;
                        }));

                    }
                    ).Start();
                    this.parts = parts;
                }
        }

    }
    public class Part_listitem
    {
        public int emptysurface { get; set; } = 100;
        public int iconheight { get; set; } = 0;
        public BitmapSource icon { get; set; } = null;
        public string uuid { get; set; }
        public string name { get; set; } = "unnamed";
        public string amount { get; set; }
        public override string ToString()
        {
            return name + "\n" + amount;
        }
    }
}
