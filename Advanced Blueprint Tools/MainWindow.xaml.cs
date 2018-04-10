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

        OpenWindow openwindow;
        //public cuz need to update(); when new creation loaded: 
        public AdvancedConnections advancedconnections;
        public AdvancedColor advancedcolorwindow;
        public SwapBlocksWindow swapblockswindow;
        public BlockProperties blockProperties;
        Ellipsoid_Generator ellpisoid_generator;
        Cuboid_Generator cuboid_Generator;


        public MainWindow()
        {

            InitializeComponent();
            //LOAD RESOURCES:
            Database.findPaths();
            new Thread(new ThreadStart(() =>
            {
                Database.LoadAllBlocks();
            })).Start();
            new Thread(new ThreadStart(() =>
            {
                Database.LoadAllBlueprints();
            })).Start();

           
            
            
        }
        
        public void UpdateOpenedBlueprint()
        {
            TextBox_Name.Text = OpenedBlueprint.description.name;
            TextBox_Description.Text = OpenedBlueprint.description.description;

            //update 3D view
            var modelGroup = new Model3DGroup();
            var glass = new Model3DGroup();
            
            int centerx = this.OpenedBlueprint.centerx;
            int centery = this.OpenedBlueprint.centery;
            int centerz = this.OpenedBlueprint.centerz;

            bool missingmods = false;
            
            foreach (dynamic x in this.OpenedBlueprint.blocksxyz)
                foreach (dynamic y in this.OpenedBlueprint.blocksxyz[x.Name])
                    foreach (dynamic z in this.OpenedBlueprint.blocksxyz[x.Name][y.Name])
                        foreach (dynamic child in this.OpenedBlueprint.blocksxyz[x.Name][y.Name][z.Name].blocks)
                        {
                            int posx = Convert.ToInt32(x.Name.ToString());
                            int posy = Convert.ToInt32(y.Name.ToString());
                            int posz = Convert.ToInt32(z.Name.ToString());

                            if (Database.blocks.ContainsKey(child.shapeId.ToString()))
                            {
                                Blockobject blockobject = Database.blocks[child.shapeId.ToString()];
                                if(blockobject is Block)
                                {
                                    Model3D model = (blockobject as Block).Render(posx - centerx, posy - centery, posz - centerz, child.bounds, child.color.ToString(), Convert.ToInt32(child.xaxis), Convert.ToInt32(child.zaxis));
                                    if(blockobject.glass==true)
                                        glass.Children.Add(model);
                                    else
                                        modelGroup.Children.Add(model);
                                }
                                else//part
                                {
                                    Model3D model = (blockobject as Part).Render(posx - centerx, posy - centery, posz - centerz, child.color.ToString(), Convert.ToInt32(child.xaxis), Convert.ToInt32(child.zaxis));
                                    if (blockobject.glass == true)
                                        glass.Children.Add(model);
                                    else
                                        modelGroup.Children.Add(model);
                                }
                            }
                            else//not in database
                            {
                                missingmods = true;

                                dynamic bounds = new JObject();
                                if (child.bounds == null)
                                {
                                    bounds.x = 1;
                                    bounds.y = 1;
                                    bounds.z = 1;
                                }
                                else
                                    bounds = child.bounds;
                        
                                Block block = new Block(null, "unknown", "unknown", null, null);
                                Model3D model = block.Render(posx - centerx, posy - centery, posz - centerz, bounds, child.color.ToString(), Convert.ToInt32(child.xaxis), Convert.ToInt32(child.zaxis));

                                modelGroup.Children.Add(model);

                            }
                        }
            if (missingmods)
                MessageBox.Show("Missing mods for this blueprint.\nReplaced missing blocks with cuboids");


            try
            {
                this.Model = modelGroup;
                this.Glass = glass;
                this.Marker = null;
                this.Marker2 = null;

                //Image_blueprint.DataContext = new MainViewModel();//'refresh'
                Image_blueprint.DataContext = null;
                Image_blueprint.DataContext = this;
                helix.ResetCamera();
                //helix.SetView(new Point3D(0, 0, 0), new Vector3D(90, 0, 0), new Vector3D(10, 10, 10), 50);
                //Model.Children[5].Transform = new ScaleTransform3D(2,2,2);

                if (connections.IsEnabled == false)
                {
                    connections.IsEnabled = true;
                    blockproperties.IsEnabled = true;
                    paint.IsEnabled = true;
                    fixcreation.IsEnabled = true;

                    //areaproperties.IsEnabled = false;
                    swapblocks.IsEnabled = true;
                    paintpicker.IsEnabled = true;
                    mirrorcreation.IsEnabled = true;
                    requiredmods.IsEnabled = true;
                    
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Failed to import into rendering box!");
            }
        }

        public void setMarker(double x, double y, double z)
        {//cross marker
            int centerx = this.OpenedBlueprint.centerx;
            int centery = this.OpenedBlueprint.centery;
            int centerz = this.OpenedBlueprint.centerz;
            Model3DGroup marker = new Model3DGroup();
            Material material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(100, 51, 204, 51)));
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(x - centerx, y - centery, z - centerz), 0.3, 0.3, 100);
            meshBuilder.AddBox(new Point3D(x - centerx, y - centery, z - centerz), 0.3, 100, 0.3);
            meshBuilder.AddBox(new Point3D(x - centerx, y - centery, z - centerz), 100, 0.3, 0.3);
            // Create a mesh from the builder (and freeze it)
            var Mesh = meshBuilder.ToMesh(true);
            marker.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });
            this.Marker = marker;
            Image_blueprint.DataContext = "";
            Image_blueprint.DataContext = this;
        }
        public void setMarker2(double x, double y, double z, double boundsx, double boundsy, double boundsz)
        {//cuboid marker
            int centerx = this.OpenedBlueprint.centerx;
            int centery = this.OpenedBlueprint.centery;
            int centerz = this.OpenedBlueprint.centerz;
            Model3DGroup marker = new Model3DGroup();
            Material material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(100, 20, 50, 50)));
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(x - centerx, y - centery, z - centerz), boundsx+0.1, boundsy+0.1, boundsz+0.1);
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
            if (OpenedBlueprint != null) e.CanExecute = true;
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
            if (OpenedBlueprint != null)
            {
                newtumbnail();
                OpenedBlueprint.description.name = TextBox_Name.Text;
                OpenedBlueprint.description.description = TextBox_Description.Text;
                OpenedBlueprint.Save();
            }
        }
        private void button_save_click(object sender, RoutedEventArgs e)//save as
        {
            if (OpenedBlueprint != null)
            {
                newtumbnail();
                OpenedBlueprint.description.name = TextBox_Name.Text;
                OpenedBlueprint.description.description = TextBox_Description.Text;
                Random r = new Random(); //GENERATE NEW UUID AND CHANGE DESCRIPTION LOCALID --todo
                OpenedBlueprint.SaveAs("Blueprint" + r.Next() + "-" + r.Next());
            }
        }
        
        private void newtumbnail()
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap(500, 500, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(helix);
            Clipboard.SetImage(bmp);
            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bmp));
            OpenedBlueprint.icon = png;
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
            //
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

            this.OpenedBlueprint.blueprint.joints = null;
            foreach (dynamic body in this.OpenedBlueprint.blueprint.bodies)
            {
                //remove bearings/springs
                foreach (dynamic child in body.childs)
                {
                    if (child.joints != null)
                        child.joints = null;
                    if (child.controller != null)
                        if (child.controller.joints != null)
                            child.controller.joints = null;
                    blueprint.bodies[0].childs.Add(child);
                }
            }
            this.OpenedBlueprint.description.description = this.OpenedBlueprint.description.description + "\n++ removed joints & glitchwelded";
            this.OpenedBlueprint.setblueprint(this.OpenedBlueprint.blueprint);
            this.UpdateOpenedBlueprint();
        }
        
        private void Click_swapblocks(object sender, RoutedEventArgs e)
        {
            if (swapblockswindow != null) swapblockswindow.Close();
            swapblockswindow = new SwapBlocksWindow(this);
            swapblockswindow.Owner = this;
            swapblockswindow.Show();
        }


        public PaintSelector paintSelector;
        private void Click_paintpicker(object sender, RoutedEventArgs e) //paint picker
        {
            if(paintSelector != null)
            {
                PaintSelector p = new PaintSelector();
                p.Owner = this;
                p.Show();
            }
            else
            {
                paintSelector = new PaintSelector();
                paintSelector.Owner = this;
                paintSelector.Show();
                //openpainpicker();
            }
        }
        public void openpaintpicker()//opens the paintpicker if mainwindow doesn't have one open
        {
            if (paintSelector == null || !paintSelector.IsLoaded)
            {
                paintSelector = new PaintSelector();
                paintSelector.Owner = this;
                paintSelector.Show();
            }
        }
        

        private void Click_mirrormode(object sender, RoutedEventArgs e)//needs work
          {
            dynamic blueprint = this.OpenedBlueprint.blueprint;
            if (blueprint.joints == null)
            {
                foreach (dynamic body in blueprint.bodies)
                    foreach (dynamic block in body.childs)
                    {

                        if (block.bounds == null)
                        {
                            //block.xaxis = -block.xaxis;
                            block.pos.x = -block.pos.x; 
                            throw new Exception(); //every block should have gotten bounds
                        }


                        {
                            dynamic realpos = this.OpenedBlueprint.getposandbounds(block);


                            realpos.xaxis = -Convert.ToInt32(block.xaxis);
                            realpos.zaxis = Convert.ToInt32(block.zaxis);
                            //Bounds bounds = Blockobject.BoundsByRotation(new Bounds(realpos.bounds),1,3);
                            realpos.pos.x = -Convert.ToInt32(block.pos.x) - Convert.ToInt32(realpos.bounds.x);
                            //realpos.pos.y = Convert.ToInt32(block.pos.y) - Convert.ToInt32(block.bounds.y);

                            block.pos = this.OpenedBlueprint.calcbppos(realpos).pos;
                            block.xaxis = this.OpenedBlueprint.calcbppos(realpos).xaxis;
                            block.zaxis = this.OpenedBlueprint.calcbppos(realpos).zaxis;

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
                this.OpenedBlueprint.setblueprint(blueprint);
                this.UpdateOpenedBlueprint();
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
        
        private void Click_cuboidgenerator(object sender, RoutedEventArgs e)
        {
            if (cuboid_Generator != null) cuboid_Generator.Close();
            cuboid_Generator = new Cuboid_Generator(this);
            cuboid_Generator.Owner = this;
            cuboid_Generator.Show();
        }

        private void Click_requiredmods(object sender, RoutedEventArgs e)
        {
            List<string> useduuids = this.OpenedBlueprint.useduuids;
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://scrapmechanic.xesau.eu/uuidservice/get_mods");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string postData = "uuid[]=" + useduuids[0].ToString();
            for (int i = 1; i < useduuids.Count(); i++)
                postData += "&uuid[]=" + useduuids[i].ToString();

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
        
        private void MenuItem_Click_Settings(object sender, RoutedEventArgs e)
        {

        }
    }
}
