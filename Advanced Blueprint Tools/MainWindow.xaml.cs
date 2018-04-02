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

        public static dynamic gameblocks = new Newtonsoft.Json.Linq.JObject();

        public dynamic getgameblocks()
        {
            return gameblocks;
        }
        

        public List<string> blueprints = new List<string>();
        public BP OpenedBlueprint { get; set; }
        public string PaintColor { get; set; } = "#eeeeee";
        public Model3DGroup Model { get; set; }
        public Model3DGroup Marker { get; set; }
        public Model3DGroup Marker2 { get; set; }
        public Model3DGroup Glass { get; set; }
        dynamic mods = new JObject();

        OpenWindow openwindow;
        public AdvancedConnections advancedconnections;
        public AdvancedColor advancedcolorwindow;
        public SwapBlocksWindow swapblockswindow;
        public BlockProperties blockProperties;


        public MainWindow()
        {

            //LOAD RESOURCES:

            new Thread(new ThreadStart(loadblueprints)).Start();//load recources
            new Thread(new ThreadStart(loadallblocks)).Start();

            // if (openwindow != null) openwindow.Close();
            //openwindow = new OpenWindow(this);
            //openwindow.Show();
            InitializeComponent();


            
            var modelGroup = new Model3DGroup();
            var importer = new AssimpImporter();
                                                                                                                        //aiProcess_GenNormals | aiProcess_FindInstances | aiProcess_CalcTangentSpace | cls 
            //importer.ConvertFromFileToFile("D:\\Program Files (x86)\\Steam\\SteamApps\\common\\Scrap Mechanic\\Data\\Objects\\Mesh\\lights\\obj_light_headlight_off.fbx", "light.obj", "obj");
               //foreach (Mesh m in a.Meshes)
                if(false)
                {
                Scene a = importer.ImportFile("D:\\Program Files (x86)\\Steam\\SteamApps\\common\\Scrap Mechanic\\Data\\Objects\\Mesh\\lights\\obj_light_headlight_off.fbx",PostProcessSteps.GenerateNormals|PostProcessSteps.FindInstances|PostProcessSteps.CalculateTangentSpace|PostProcessSteps.Triangulate);//obj_decor_cone.mesh

                Mesh m = a.Meshes[0];
                    IList<Point3D> vertices = new List<Point3D>();
                    var meshBuilder = new MeshBuilder(false, false);
                    var sdf = m.Faces;
                    foreach(Face face in m.Faces)
                    {
                        foreach(uint i in face.Indices)
                        {
                            vertices.Add(new Point3D(m.Vertices[i].X, m.Vertices[i].Y, m.Vertices[i].Z));
                        }
                        meshBuilder.AddPolygon(vertices);
                        vertices = new List<Point3D>();
                    }

                    Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                    Geometry3D Mesh = meshBuilder.ToMesh(true);
                    modelGroup.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });

                    this.Model = modelGroup;
                    Image_blueprint.DataContext = "";
                    Image_blueprint.DataContext = this;
                }
            
        }

        
        public void UpdateOpenedBlueprint()
        {
            //try
            {
                TextBox_Name.Text = OpenedBlueprint.description.name;
                TextBox_Description.Text = OpenedBlueprint.description.description;
                //Image_blueprint.Source = new BitmapImage(new Uri(OpenedBlueprint.icon));
            
                //update 3D view
                var modelGroup = new Model3DGroup();
                var glass = new Model3DGroup();



                bool noobj =false;
                bool haderror = false;
                foreach (dynamic x in this.OpenedBlueprint.blocksxyz)
                    foreach (dynamic y in this.OpenedBlueprint.blocksxyz[x.Name])
                        foreach (dynamic z in this.OpenedBlueprint.blocksxyz[x.Name][y.Name])
                            foreach (dynamic child in this.OpenedBlueprint.blocksxyz[x.Name][y.Name][z.Name].blocks)
                            {
                                string uuid = child.shapeId.ToString();

                                string mesh = "";
                                //The Importer to load .obj files
                                ModelImporter importer = new ModelImporter();
                                Model3D loadedblock = null;
                                //try
                                {

                                    if (gameblocks[uuid] != null)//missing mod?
                                    {
                                        if (gameblocks[uuid].mesh.ToString() != "")//its a a part
                                        {
                                            mesh = gameblocks[uuid].mesh;
                                            if (System.IO.Path.GetExtension(mesh) != ".obj")
                                            {
                                                try
                                                {

                                                    Scene a = new AssimpImporter().ImportFile(mesh);                                                                                    
                                                    //foreach (Mesh m in a.Meshes)
                                                    {
                                                        Mesh m = a.Meshes[0];
                                                        IList<Point3D> vertices = new List<Point3D>();
                                                        var meshBuilder = new MeshBuilder(false, false);
                                                        foreach (Face face in m.Faces)
                                                        {
                                                            foreach (uint i in face.Indices)
                                                            {
                                                                vertices.Add(new Point3D(m.Vertices[i].X, m.Vertices[i].Y, m.Vertices[i].Z));
                                                            }
                                                            meshBuilder.AddPolygon(vertices);
                                                            vertices = new List<Point3D>();
                                                        }
                                                    
                                                        Color color = new Color();
                                                        string colorstr = child.color.ToString();
                                                        if ((child.color.ToString()[0]) == '#')
                                                            colorstr.Substring(1);
                                                        color = Color.FromRgb(Convert.ToByte(colorstr.Substring(0, 2), 16), Convert.ToByte(colorstr.Substring(2, 2), 16), Convert.ToByte(colorstr.Substring(4, 2), 16));

                                                        Material material = new DiffuseMaterial(new SolidColorBrush(color));
                                                        if (gameblocks[uuid] != null && gameblocks[uuid].glass == true)
                                                        {
                                                            color.A = 120;
                                                            material = new DiffuseMaterial(new SolidColorBrush(color));
                                                        }

                                                        Geometry3D Mesh = meshBuilder.ToMesh(true);

                                                        //modelGroup.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });

                                                        //importer.DefaultMaterial = material;

                                                        //loadedblock = importer.Load(mesh);
                                                        loadedblock = new GeometryModel3D { Geometry = Mesh, Material = material };

                                                        #region Rotation and Translation

                                                        loadedblock.Transform = new MatrixTransform3D(new Matrix3D());
                                                        Transform3DGroup baseLinkMove = new Transform3DGroup();
                                                        TranslateTransform3D baseLinkTranslate = new TranslateTransform3D();
                                                        RotateTransform3D baseLinkRotatex = new RotateTransform3D();
                                                        RotateTransform3D baseLinkRotatez = new RotateTransform3D();
                                                        bool xpos = child.xaxis > 0;
                                                        bool zpos = child.zaxis > 0;
                                                        switch (Math.Abs(Convert.ToInt32(child.xaxis.ToString())))
                                                        {
                                                            case 1:
                                                                baseLinkRotatex.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, xpos ? 0 : 1), 180);
                                                                baseLinkMove.Children.Add(baseLinkRotatex);
                                                                switch (Math.Abs(Convert.ToInt32(child.zaxis.ToString())))
                                                                {
                                                                    case 1:
                                                                        MessageBox.Show("Incorrect rotationset found !");
                                                                        break;
                                                                    case 2:
                                                                        baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(zpos ? -1 : 1, 0, 0), 90);
                                                                        break;
                                                                    case 3:
                                                                        baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(zpos ? 0 : 1, 0, 0), 180);
                                                                        break;
                                                                }
                                                                baseLinkMove.Children.Add(baseLinkRotatez);
                                                                break;
                                                            case 2:
                                                                baseLinkRotatex.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, xpos ? 1 : -1), 90);
                                                                baseLinkMove.Children.Add(baseLinkRotatex);
                                                                switch (Math.Abs(Convert.ToInt32(child.zaxis.ToString())))
                                                                {
                                                                    case 1:
                                                                        baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(0, zpos ? 1 : -1, 0), 90);
                                                                        break;
                                                                    case 2:
                                                                        MessageBox.Show("Incorrect rotationset found !");
                                                                        break;
                                                                    case 3:
                                                                        baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(0, zpos ? 0 : 1, 0), 180);
                                                                        break;
                                                                }
                                                                baseLinkMove.Children.Add(baseLinkRotatez);
                                                                break;
                                                            case 3:
                                                                baseLinkRotatex.Rotation = new AxisAngleRotation3D(new Vector3D(0, xpos ? -1 : 1, 0), 90);
                                                                baseLinkMove.Children.Add(baseLinkRotatex);
                                                                switch (Math.Abs(Convert.ToInt32(child.zaxis.ToString())))
                                                                {
                                                                    case 1:
                                                                        baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, (zpos == xpos) ? 1 : 0), 180);
                                                                        break;
                                                                    case 2:
                                                                        baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, (zpos == xpos) ? -1 : 1), 90);
                                                                        break;
                                                                    case 3:
                                                                        MessageBox.Show("Incorrect rotationset found !");
                                                                        break;
                                                                }
                                                                baseLinkMove.Children.Add(baseLinkRotatez);
                                                                break;
                                                        } //rotations translate!

                                                        int centerx = (this.OpenedBlueprint.maxx + this.OpenedBlueprint.minx) / 2;
                                                        int centery = (this.OpenedBlueprint.maxy + this.OpenedBlueprint.miny) / 2;
                                                        int centerz = (this.OpenedBlueprint.maxz + this.OpenedBlueprint.minz) / 2;


                                                        baseLinkTranslate.OffsetX = Convert.ToInt32(x.Name) - centerx + (float)(child.bounds.x) / 2;
                                                        baseLinkTranslate.OffsetY = Convert.ToInt32(y.Name) - centery + (float)(child.bounds.y) / 2;
                                                        baseLinkTranslate.OffsetZ = Convert.ToInt32(z.Name) - centerz + (float)(child.bounds.z) / 2;

                                                        baseLinkMove.Children.Add(baseLinkTranslate);
                                                        loadedblock.Transform = baseLinkMove;
                                                        #endregion
                                                        if (gameblocks[uuid].glass == true)
                                                        {
                                                            glass.Children.Add(loadedblock);
                                                        }
                                                        else
                                                            modelGroup.Children.Add(loadedblock);
                                                    }

                                                }
                                                catch
                                                {

                                                    //check for same mesh but with extension .obj
                                                    string Directory = System.IO.Path.GetDirectoryName(mesh);
                                                    string filename = System.IO.Path.GetFileNameWithoutExtension(mesh);
                                                    if(File.Exists(Directory+"\\"+filename + ".obj"))
                                                    {
                                                        mesh = Directory + "\\" + filename + ".obj";
                                                    }
                                                    else
                                                    {//no .obj file for this block
                                                        mesh = null;
                                                        if (noobj == false) MessageBox.Show("couldn't load one or more meshes in this creation, ask the mod maker to add .obj files to the mod meshes or wait for release 2 of this tool");
                                                        noobj = true;
                                                    }
                                                }

                                            }

                                            if (System.IO.Path.GetExtension(mesh) == ".obj")
                                            {
                                                try
                                                {

                                                    var bc = new BrushConverter();
                                                    //string color = "#FF" + textbox_color.Text.Substring(1);
                                                    //textbox_color.Background = (Brush)bc.ConvertFrom(color);
                                                    Color color = new Color();
                                                    string colorstr = child.color.ToString();

                                                    if ((child.color.ToString()[0]) == '#')
                                                        color = Color.FromRgb(Convert.ToByte(colorstr.Substring(1, 2), 16), Convert.ToByte(colorstr.Substring(3, 2), 16), Convert.ToByte(colorstr.Substring(5, 2), 16));
                                                    else
                                                        color = Color.FromRgb(Convert.ToByte(colorstr.Substring(0, 2), 16), Convert.ToByte(colorstr.Substring(2, 2), 16), Convert.ToByte(colorstr.Substring(4, 2), 16));

                                                    Material material = new DiffuseMaterial(new SolidColorBrush(color));

                                                    if (gameblocks[uuid] != null && gameblocks[uuid].glass == true)
                                                    {
                                                        color.A = 120;
                                                        material = new DiffuseMaterial(new SolidColorBrush(color));
                                                    }
                                                    
                                                    //if (System.IO.Path.GetExtension(gameblocks[uuid].textures[0].ToString())!=".tga")
                                                      //  material = MaterialHelper.CreateImageMaterial(gameblocks[uuid].textures[0].ToString());

                                                    importer.DefaultMaterial = material;

                                                    loadedblock = importer.Load(mesh);
                                                    

                                                    #region Rotation and Translation

                                                    loadedblock.Transform = new MatrixTransform3D(new Matrix3D());
                                                    Transform3DGroup baseLinkMove = new Transform3DGroup();
                                                    TranslateTransform3D baseLinkTranslate = new TranslateTransform3D();
                                                    RotateTransform3D baseLinkRotatex = new RotateTransform3D();
                                                    RotateTransform3D baseLinkRotatez = new RotateTransform3D();
                                                    bool xpos = child.xaxis > 0;
                                                    bool zpos = child.zaxis > 0;
                                                    switch (Math.Abs(Convert.ToInt32(child.xaxis.ToString())))
                                                    {
                                                        case 1:
                                                            baseLinkRotatex.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, xpos ? 0 : 1), 180);
                                                            baseLinkMove.Children.Add(baseLinkRotatex);
                                                            switch (Math.Abs(Convert.ToInt32(child.zaxis.ToString())))
                                                            {
                                                                case 1:
                                                                    MessageBox.Show("Incorrect rotationset found !");
                                                                    break;
                                                                case 2:
                                                                    baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(zpos ? -1 : 1, 0, 0), 90);
                                                                    break;
                                                                case 3:
                                                                    baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(zpos ? 0 : 1, 0, 0), 180);
                                                                    break;
                                                            }
                                                            baseLinkMove.Children.Add(baseLinkRotatez);
                                                            break;
                                                        case 2:
                                                            baseLinkRotatex.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, xpos ? 1 : -1), 90);
                                                            baseLinkMove.Children.Add(baseLinkRotatex);
                                                            switch (Math.Abs(Convert.ToInt32(child.zaxis.ToString())))
                                                            {
                                                                case 1:
                                                                    baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(0, zpos ? 1 : -1, 0), 90);
                                                                    break;
                                                                case 2:
                                                                    MessageBox.Show("Incorrect rotationset found !");
                                                                    break;
                                                                case 3:
                                                                    baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(0, zpos ? 0 : 1, 0), 180);
                                                                    break;
                                                            }
                                                            baseLinkMove.Children.Add(baseLinkRotatez);
                                                            break;
                                                        case 3:
                                                            baseLinkRotatex.Rotation = new AxisAngleRotation3D(new Vector3D(0, xpos ? -1 : 1, 0), 90);
                                                            baseLinkMove.Children.Add(baseLinkRotatex);
                                                            switch (Math.Abs(Convert.ToInt32(child.zaxis.ToString())))
                                                            {
                                                                case 1:
                                                                    baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, (zpos == xpos) ? 1 : 0), 180);
                                                                    break;
                                                                case 2:
                                                                    baseLinkRotatez.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, (zpos == xpos) ? -1 : 1), 90);
                                                                    break;
                                                                case 3:
                                                                    MessageBox.Show("Incorrect rotationset found !");
                                                                    break;
                                                            }
                                                            baseLinkMove.Children.Add(baseLinkRotatez);
                                                            break;
                                                    } //rotations translate!

                                                    int centerx = (this.OpenedBlueprint.maxx + this.OpenedBlueprint.minx) / 2;
                                                    int centery = (this.OpenedBlueprint.maxy + this.OpenedBlueprint.miny) / 2;
                                                    int centerz = (this.OpenedBlueprint.maxz + this.OpenedBlueprint.minz) / 2;


                                                    baseLinkTranslate.OffsetX = Convert.ToInt32( x.Name) - centerx + (float)(child.bounds.x)/2;
                                                    baseLinkTranslate.OffsetY = Convert.ToInt32( y.Name) - centery + (float)(child.bounds.y)/2;
                                                    baseLinkTranslate.OffsetZ = Convert.ToInt32( z.Name) - centerz + (float)(child.bounds.z)/2;

                                                    baseLinkMove.Children.Add(baseLinkTranslate);
                                                    loadedblock.Transform = baseLinkMove;
                                                    #endregion
                                                    if(gameblocks[uuid].glass == true)
                                                    {
                                                        glass.Children.Add(loadedblock);
                                                    }
                                                    else
                                                        modelGroup.Children.Add(loadedblock);
                                                }
                                                catch(Exception e)
                                                {//failed to load the .obj
                                                    MessageBox.Show(e.Message + "\n\nMesh: " + mesh, "Failed to load mesh!");
                                                    mesh = null;
                                                }
                                            }
                                        }
                                        else //its a block
                                        {
                                            mesh = null;
                                        }
                                    }
                                    else
                                    {
                                        if(!haderror) MessageBox.Show("missing UUID: "+uuid);
                                        haderror = true;
                                        mesh = null;
                                        //missing mod!
                                    }
                                }
                                //catch(Exception ex)
                                {
                                    //MessageBox.Show(ex.Message+"\n\nMesh: "+ mesh, "unknown error!");
                                }

                                try
                                {

                                    if (child.bounds != null && mesh == null)//bp.cs assigns bounds to child , it gets it from gameblocks[]
                                    {//previous shit failed but it's a block or whatever, attempt to build it!

                                        dynamic bounds = child.bounds;

                                        //var bc = new BrushConverter();
                                        //string color = "#FF" + textbox_color.Text.Substring(1);
                                        //textbox_color.Background = (Brush)bc.ConvertFrom(color);
                                        Color color = new Color();
                                        string colorstr = child.color.ToString();

                                        if ((child.color.ToString()[0]) == '#')
                                            color = Color.FromRgb(Convert.ToByte(colorstr.Substring(1, 2), 16), Convert.ToByte(colorstr.Substring(3, 2), 16), Convert.ToByte(colorstr.Substring(5, 2), 16));
                                        else
                                            color = Color.FromRgb(Convert.ToByte(colorstr.Substring(0, 2), 16), Convert.ToByte(colorstr.Substring(2, 2), 16), Convert.ToByte(colorstr.Substring(4, 2), 16));

                                        Material material = new DiffuseMaterial(new SolidColorBrush(color));

                                        //if (System.IO.Path.GetExtension(gameblocks[uuid].textures[0].ToString()) != ".tga")
                                        //  material = MaterialHelper.CreateImageMaterial(gameblocks[uuid].textures[0].ToString());


                                        int centerx = this.OpenedBlueprint.centerx;
                                        int centery = this.OpenedBlueprint.centery;
                                        int centerz = this.OpenedBlueprint.centerz;
                                        // Create a mesh builder and add a box to it


                                        var meshBuilder = new MeshBuilder(false, false);
                                        meshBuilder.AddBox(new Point3D(Convert.ToInt32(x.Name) - centerx + (float)(bounds.x) / 2,
                                            Convert.ToInt32(y.Name) - centery + (float)(bounds.y) / 2,
                                            Convert.ToInt32(z.Name) - centerz + (float)(bounds.z) / 2),
                                            Convert.ToInt32(bounds.x), Convert.ToInt32(bounds.y), Convert.ToInt32(bounds.z));
                                        // Create a mesh from the builder (and freeze it)
                                        var Mesh = meshBuilder.ToMesh(true);

                                        if (gameblocks[uuid] != null && gameblocks[uuid].glass == true)
                                        {
                                            color.A = 150;
                                            material = new DiffuseMaterial(new SolidColorBrush(color));
                                            glass.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });
                                            break;
                                        }
                                        else
                                            modelGroup.Children.Add(new GeometryModel3D { Geometry = Mesh, Material = material });

                                    }

                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message+"\n\n", "Failed to Create new mesh!");
                                }

                                //string mesh = MainWindow.gameblocks[uuid].renderable.lodList[0].mesh; //game_data/whatever

                                //string meshpath;


                                //mesh = @"D:\Program Files (x86)\Steam\SteamApps\common\Scrap Mechanic\Data\Objects\Mesh\industrial\obj_industrial_satellite02.fbx";
                                //The Importer to load .obj files
                                //ModelImporter importer = new ModelImporter();
                                //The Material (Color) that is applyed to the importet objects
                                //Material material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
                                //importer.DefaultMaterial = material;
                                //Model3D block = importer.Load(mesh);
                                //modelGroup.Children.Add(block);
                            }//end of foreach



                
                


                try
                {
                    
                    this.Model = modelGroup;
                    this.Glass = glass;
                    this.Marker = null;
                    this.Marker2 = null;

                    //Image_blueprint.DataContext = new MainViewModel();//'refresh'
                    Image_blueprint.DataContext = "";
                    Image_blueprint.DataContext = this;
                    helix.ResetCamera();
                    //Model.Children[5].Transform = new ScaleTransform3D(2,2,2);

                    if (menuitem1.IsEnabled==false)
                    {
                        menuitem1.IsEnabled = true;
                        menuitem2.IsEnabled = true;
                        menuitem3.IsEnabled = true;
                        menuitem4.IsEnabled = true;
                        /*
                        menuitem5.IsEnabled = true;
                        menuitem6.IsEnabled = true;
                        menuitem7.IsEnabled = true;
                        menuitem8.IsEnabled = true;
                        menuitem9.IsEnabled = true;*/
                        menuitem10.IsEnabled = true;
                        menuitem11.IsEnabled = true;
                        menuitem12.IsEnabled = true;
                        menuitem14.IsEnabled = true;
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message, "Failed to import into rendering box!");
                }



            }
            //catch (Exception e)
            {
              //  MessageBox.Show(e.Message);
            }
        }
        public void setMarker(double x, double y, double z)
        {
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
        {
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
            if(((JObject)gameblocks).Count>0)
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
            else
            {
                MessageBox.Show("Game-file resources are not fully loaded yet!\nplease take a moment\nthis won't take long");
            }
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
            MessageBox.Show("saved as new creation,\nif game is running: will need to be restarted to be able to see the blueprints, complain to the devs about this, not me");
        }
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }


        private void button_overwrite_click(object sender, RoutedEventArgs e)//save/overwrite
        {
            if (OpenedBlueprint != null)
            {
                save();
                OpenedBlueprint.Save();
            }
        }
        private void button_save_click(object sender, RoutedEventArgs e)//save as
        {
            if (OpenedBlueprint != null)
            {
                save();
                Random r = new Random(); //GENERATE NEW UUID AND CHANGE DESCRIPTION LOCALID
                OpenedBlueprint.SaveAs("Blueprint" + r.Next() + "-" + r.Next());
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //connection
            if (advancedconnections != null) advancedconnections.Close();
            advancedconnections = new AdvancedConnections(this);
            advancedconnections.Owner = this;
            advancedconnections.Show();
        }//advancedwirefeature

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (blockProperties != null) blockProperties.Close();
            blockProperties = new BlockProperties(this);
            blockProperties.Owner = this;
            blockProperties.Show();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {//paint tool
            if (advancedcolorwindow != null) advancedcolorwindow.Close();
            advancedcolorwindow = new AdvancedColor(this);
            advancedcolorwindow.Owner = this;
            advancedcolorwindow.Show();
        }//advancedcolorfeature
        

        private void menuitem_Click_4(object sender, RoutedEventArgs e) //remove joints & weld
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

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {

            if (swapblockswindow != null) swapblockswindow.Close();
            swapblockswindow = new SwapBlocksWindow(this);
            swapblockswindow.Owner = this;
            swapblockswindow.Show();
        }

        public PaintSelector paintSelector;
        private void MenuItem_Click_10(object sender, RoutedEventArgs e) //paint picker
        {
            if(paintSelector != null)
            {
                PaintSelector p = new PaintSelector(this);
                p.Owner = this;
                p.Show();
            }
            else
            {
                paintSelector = new PaintSelector(this);
                paintSelector.Owner = this;
                paintSelector.Show();
            }
        }
        public void openpaintpicker()
        {
            if (paintSelector == null || !paintSelector.IsLoaded)
            {
                paintSelector = new PaintSelector(this);
                paintSelector.Owner = this;
                paintSelector.Show();
            }
        }

        private void save()//textbox to BP()
        {
            OpenedBlueprint.description.name = TextBox_Name.Text;
            OpenedBlueprint.description.description = TextBox_Description.Text;
            MessageBox.Show("Blueprint Saved!");
        }

        //PRE LOAD BLUEPRINTS AND CRAP

        public string blueprintdir = "";
        public void loadblueprints()
        {//Find the correct blueprint dir:
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (Directory.Exists(appdata + @"\Axolot Games\Scrap Mechanic\User"))
            {
                string scrapdir = appdata + "\\Axolot Games\\Scrap Mechanic\\User";
                DateTime lasthigh = new DateTime(1900, 1, 1);
                string dir = "";
                foreach (string subdir in Directory.GetDirectories(scrapdir)) //get user_numbers folder that is last used
                {
                    DirectoryInfo fi1 = new DirectoryInfo(subdir + @"\blueprints");
                    DateTime created = fi1.LastWriteTime;
                    fi1 = new DirectoryInfo(subdir);

                    if (created > lasthigh)
                    {
                        dir = subdir;
                        lasthigh = created;
                    }
                }
                blueprintdir = dir + "\\blueprints";
            }

            foreach (string blueprint in Directory.GetDirectories(blueprintdir))
            {
                if (File.Exists(blueprint + @"\blueprint.json") && File.Exists(blueprint + @"\icon.png") && File.Exists(blueprint + @"\description.json"))
                {
                    this.blueprints.Add(blueprint);
                }
            }

            try
            {
                while (!Directory.Exists(steamapps ) || steamapps == null || steamapps == "") ;//don't continue if directory not found
            }
            catch
            { }


            try
            {
                foreach (string blueprint in Directory.GetDirectories(steamapps + @"\workshop\content\387990"))
                {
                    if (File.Exists(blueprint + @"\blueprint.json") && File.Exists(blueprint + @"\icon.png") && File.Exists(blueprint + @"\description.json"))
                    {
                        this.blueprints.Add(blueprint);
                    }
                }
            }
            catch { }

            while (true)
            {
                if (Directory.GetLastWriteTime(blueprintdir) > DateTime.Now.AddSeconds(-2))
                {
                    this.blueprints = new List<string>();
                    // a blueprint is added/removed!
                    foreach (string blueprint in Directory.GetDirectories(blueprintdir))
                    {
                        if (File.Exists(blueprint + @"\blueprint.json") && File.Exists(blueprint + @"\icon.png") && File.Exists(blueprint + @"\description.json"))
                        {
                            this.blueprints.Add(blueprint);
                        }
                    }
                }

                try
                {

                    if (Directory.GetLastWriteTime(steamapps + @"\workshop\content\387990") > DateTime.Now.AddSeconds(-2))
                        foreach (string blueprint in Directory.GetDirectories(steamapps + @"\workshop\content\387990"))
                        {
                            if (File.Exists(blueprint + @"\blueprint.json") && File.Exists(blueprint + @"\icon.png") && File.Exists(blueprint + @"\description.json"))
                            {
                                this.blueprints.Add(blueprint);
                            }
                        }
                }
                catch { }
                
                Thread.Sleep(1500);
                try
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                     //check if window still active
                    }));
                }
                catch
                {
                    break;
                    this.Close();
                } //end thread if window closed
            }
        }
        string steamapps;

        public void loadallblocks()
        {
            string ScrapData = "";
            string ModDatabase = "";
            {
                dynamic file = new JObject();
                file.steamapps = "";
                file.times = 0;
                if (File.Exists("steamapps"))
                {
                    try
                    {
                        file = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("steamapps"));
                    }
                    catch
                    {

                    }
                    
                    if(file.steamapps != null)
                        steamapps = file.steamapps.ToString();
                    if(file.times != null && Convert.ToInt32(file.times) == 3)
                    {
                        System.Diagnostics.Process.Start("http://www.youtube.com/c/brentbatch?sub_confirmation=1");
                    }

                }
                if (steamapps == null)
                {
                    steamapps = "";
                }

                if (System.IO.Directory.Exists(Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Steam\SteamApps\common\Scrap Mechanic\Data\Objects\Database\ShapeSets"))
                {
                    steamapps = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\\steam\\SteamApps";
                }
                else if (System.IO.Directory.Exists(@"C:\Program Files (x86)\Steam\SteamApps\common\Scrap Mechanic\Data\Objects\Database\ShapeSets"))
                {
                    steamapps = @"C:\Program Files (x86)\Steam\SteamApps";
                }
                else if (System.IO.Directory.Exists(@"D:\Program Files (x86)\Steam\SteamApps\common\Scrap Mechanic\Data\Objects\Database\ShapeSets"))
                {
                    steamapps = @"D:\Program Files (x86)\Steam\SteamApps";
                }
                else if (System.IO.Directory.Exists(steamapps + @"\common\Scrap Mechanic\Data\Objects\Database\ShapeSets"))
                {
                    //i already know steamapps!
                }
                else
                {   
                    while (!System.IO.Directory.Exists(ScrapData))
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {//this refer to form in WPF application 
                            MessageBoxResult result = MessageBox.Show("could not find the gamefiles folder which is needed to get the block properties\nPlease select the Scrap Mechanic folder, It contains: Cache,Data,Logs,Release,...\n\nSteam Library > Right click scrap mechanic > properties > Local Files > browse local files > Copy path from the folder", "Unusual folder location!",MessageBoxButton.OKCancel);
                            if(result != MessageBoxResult.OK)
                            {
                                this.Close();
                                MessageBox.Show("continuing without loading resources.\nWorking processes: \n- Loading blueprints -although a bit broken\n- sphere generator\n- mirror mode");
                            }

                            System.Windows.Forms.FolderBrowserDialog fbD = new System.Windows.Forms.FolderBrowserDialog();
                            fbD.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                            
                            fbD.ShowDialog();
                            string scrap = fbD.SelectedPath;
                            steamapps = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(scrap));
                            ScrapData = scrap+"\\Data";
                        }));
                    }
                }

                if(ScrapData=="")
                    ScrapData = steamapps + @"\common\Scrap Mechanic\Data";
                ModDatabase = steamapps + @"\workshop\content\387990";

                file.times = Convert.ToInt32(file.times) + 1;
                file.steamapps = steamapps;
                //Properties.Resources.steamapps = steamapps;
                File.WriteAllText("steamapps", file.ToString());

            }//get paths

            dynamic blocks = new Newtonsoft.Json.Linq.JObject();
            int amountloaded=0;

            //VANILLA BLOCKS: 
            dynamic blockz = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(ScrapData + @"\Objects\Database\basicmaterials.json"));
            foreach (dynamic prop in blockz)
            {
                foreach (dynamic part in blockz[prop.Name]) //added cuz i need textures
                {
                    try
                    {
                        blocks[part.uuid.ToString()] = part;
                        blocks[part.uuid.ToString()].Name = "unnamed shape " + part.uuid;
                        blocks[part.uuid.ToString()].Mod = "vanilla";
                        blocks[part.uuid.ToString()].textures = new Newtonsoft.Json.Linq.JArray();
                        blocks[part.uuid.ToString()].textures.Add(ScrapData + @"\Objects\Textures\Blocks\" + System.IO.Path.GetFileName(part.dif.ToString()));
                        blocks[part.uuid.ToString()].textures.Add(ScrapData + @"\Objects\Textures\Blocks\" + System.IO.Path.GetFileName(part.asg.ToString()));
                        blocks[part.uuid.ToString()].textures.Add(ScrapData + @"\Objects\Textures\Blocks\" + System.IO.Path.GetFileName(part.nor.ToString()));
                        blocks[part.uuid.ToString()].mesh = "";
                        amountloaded++;
                    }
                    catch
                    { }
                    
                }
            }
            //VANILLA PARTS:
            string ScrapShapeSets = ScrapData + @"\Objects\Database\ShapeSets";
            foreach (string file in System.IO.Directory.GetFiles(ScrapShapeSets))
            {
                dynamic parts = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(file));
                foreach (dynamic prop in parts)
                {
                    foreach (dynamic part in parts[prop.Name])
                    {
                        try
                        {
                            blocks[part.uuid.ToString()] = part;
                            blocks[part.uuid.ToString()].Name = "unnamed shape " + part.uuid;
                            blocks[part.uuid.ToString()].Mod = "vanilla";
                            blocks[part.uuid.ToString()].textures = new Newtonsoft.Json.Linq.JArray();
                            if(part.renderable.lodList[0].subMeshList!=null)
                            {
                                foreach(dynamic submesh in part.renderable.lodList[0].subMeshList)
                                    if(submesh.material.ToString() == "Glass")
                                        blocks[part.uuid.ToString()].glass = true;


                                 foreach (string texture in part.renderable.lodList[0].subMeshList[0].textureList)
                                    if(texture != "")
                                        blocks[part.uuid.ToString()].textures.Add(ScrapData + @"\Objects\Textures\" + System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(texture))+ "\\" + System.IO.Path.GetFileName(texture));
                            }
                            else
                            {
                                foreach (string texture in part.renderable.lodList[0].subMeshMap.Shark.textureList)//TEMPORARY SOLUTION, WILL CRASH IF NEW OBJECT ADDED
                                    if (texture != "")
                                        blocks[part.uuid.ToString()].textures.Add(ScrapData + @"\Objects\Textures\" + System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(texture)) + "\\" + System.IO.Path.GetFileName(texture));

                            }
                            blocks[part.uuid.ToString()].mesh = ScrapData + @"\Objects\Mesh\" + System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.renderable.lodList[0].mesh.ToString())) + "\\" + System.IO.Path.GetFileName(part.renderable.lodList[0].mesh.ToString());
                            amountloaded++;
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show("found unusual file in vanilla gamefiles:\nPlease remove old mods from the game files!!\n\n" + file,e.Message);
                            blocks[part.uuid.ToString()].mesh = "";
                        }
                    }
                }
            }
            //name the vanilla blocks:
            string basicmat = ScrapData + @"\Gui\Language\English\InventoryItemDescriptions.json";
            dynamic inventoryitemdesc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(basicmat));
            foreach (dynamic block in inventoryitemdesc)
            {
                if(blocks[block.Name.ToString()] != null) blocks[block.Name.ToString()].Name = block.Value.title.ToString();
            }
            

            List<String> workshoppages = new List<string>();
            //MODDED BLOCKS:
            if(System.IO.Directory.Exists(ModDatabase))
            foreach (string folder in System.IO.Directory.GetDirectories(ModDatabase))
            {
                int conflictusemod = 0; //(1 use old, 2 use new
                //modded blocks:
                if (System.IO.Directory.Exists(folder + @"\Objects\Database\ShapeSets"))
                {
                    foreach (string file in System.IO.Directory.GetFiles(folder + @"\Objects\Database\ShapeSets"))
                        if (System.IO.Path.GetExtension(file).ToLower() == ".json")
                        {
                            dynamic parts = null;
                            try
                            {
                                parts = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(file));
                            }
                            catch(Exception e)
                            {
                                //MessageBox.Show(e.Message + "\n\n" + file);
                                workshoppages.Add("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(file));
                                parts = null;
                            }
                            if(parts !=null)
                            foreach (dynamic prop in parts)
                            {
                                foreach (dynamic part in parts[prop.Name])
                                {
                                    try
                                    {
                                        dynamic moddesc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(folder + @"\description.json"));

                                        if (moddesc.name != null)
                                        {
                                            if (conflictusemod == 0 && blocks[part.uuid.ToString()] != null && blocks[part.uuid.ToString()].Modfolder != folder)
                                            {
                                                conflictusemod = 1;//keep mod1

                                                string uuid = part.uuid.ToString();
                                                string mod1 = blocks[part.uuid.ToString()].Mod;
                                                string mod2 = moddesc.name;

                                                if(mod1.ToString() == "vanilla")
                                                {
                                                    conflictusemod = 2;//overwrite!
                                                }
                                                else
                                                {
                                                    MessageBoxResult messageBoxResult = MessageBox.Show
                                                        ("\"" + mod1 + "\" is using some(or all) uuid's of mod \"" + mod2 + "\"\n\nOverwrite blocks from \"" + mod1 + "\" with the blocks from \"" + mod2 + "\" ?\n\n"+file, "Conflicting mods!!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                                    if (messageBoxResult == MessageBoxResult.Yes)
                                                    {
                                                        conflictusemod = 2;//overwrite!
                                                    }
                                                }

                                                    //System.Diagnostics.Process.Start(blocks[part.uuid.ToString()].Modfolder.ToString());
                                                    //System.Diagnostics.Process.Start(folder);
                                                }

                                            if (conflictusemod != 1)//OVERWRITE if 2 , 0=no conflict
                                            {
                                                string p = part.ToString();
                                                blocks[part.uuid.ToString()] = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(p);
                                                blocks[part.uuid.ToString()].Name = "unnamed shape " + part.uuid;
                                                blocks[part.uuid.ToString()].Modfolder = folder;
                                                this.mods[System.IO.Path.GetFileName(folder).ToString()] = new JObject();
                                                blocks[part.uuid.ToString()].Mod = moddesc.name;
                                                blocks[part.uuid.ToString()].textures = new Newtonsoft.Json.Linq.JArray();
                                                if (part.renderable != null)
                                                {

                                                    foreach (dynamic submesh in part.renderable.lodList[0].subMeshList)
                                                        if (submesh.material.ToString() == "Glass")
                                                                blocks[part.uuid.ToString()].glass = true;
                                                    foreach (string texture in part.renderable.lodList[0].subMeshList[0].textureList)
                                                    if (texture != "")
                                                        blocks[part.uuid.ToString()].textures.Add(folder + @"\Objects\Textures\" + (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(texture))=="Textures"?"": System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(texture))+"\\") + System.IO.Path.GetFileName(texture));
                                                    blocks[part.uuid.ToString()].mesh = folder + @"\Objects\Mesh\" + ((System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.renderable.lodList[0].mesh.ToString())) == "Mesh" )? "" : (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.renderable.lodList[0].mesh.ToString())) + "\\")) + System.IO.Path.GetFileName(part.renderable.lodList[0].mesh.ToString());
                                                    string test = blocks[part.uuid.ToString()].mesh;
                                                    if (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.renderable.lodList[0].mesh.ToString())) != "Mesh")
                                                    {
                                                    }
                                                }
                                                else
                                                {//its a block!
                                                    blocks[part.uuid.ToString()].textures.Add(folder + @"\Objects\Textures\" +(( System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.dif.ToString())) == "Textures") ? "" : (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.dif.ToString())) + "\\")) + System.IO.Path.GetFileName(part.dif.ToString()));
                                                    blocks[part.uuid.ToString()].textures.Add(folder + @"\Objects\Textures\" +(( System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.asg.ToString())) == "Textures") ? "" : (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.asg.ToString())) + "\\")) + System.IO.Path.GetFileName(part.asg.ToString()));
                                                    blocks[part.uuid.ToString()].textures.Add(folder + @"\Objects\Textures\" +(( System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.nor.ToString())) == "Textures") ? "" : (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.nor.ToString())) + "\\")) + System.IO.Path.GetFileName(part.nor.ToString()));
                                                    blocks[part.uuid.ToString()].mesh = "";
                                                    dynamic t = blocks[part.uuid.ToString()].textures.ToString();
                                                    t = null;
                                                }
                                                amountloaded++;
                                            }
                                        }
                                        else
                                        {

                                            MessageBox.Show("you have a broken mod!\nFile: " + file);
                                        }

                                    }
                                    catch
                                    {
                                        //MessageBox.Show(e.Message);
                                        workshoppages.Add("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder));
                                    }
                                }
                            }
                        }   
                }

                //names:
                if (System.IO.Directory.Exists(folder + @"\Gui\Language\English"))
                {
                    foreach(string file in Directory.GetFiles(folder + @"\Gui\Language\English\"))
                    if(System.IO.File.Exists(folder + @"\Gui\Language\English\inventoryDescriptions.json"))
                    {
                        try
                        {
                            
                            inventoryitemdesc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(folder + @"\Gui\Language\English\inventoryDescriptions.json"));
                            if (inventoryitemdesc != null)
                                foreach (dynamic block in inventoryitemdesc)
                                    if (conflictusemod != 1 && blocks[block.Name.ToString()] != null)
                                    {
                                        string test = block.Value.title;
                                        blocks[block.Name.ToString()].Name = test;
                                    }

                        }
                        catch
                        {
                            workshoppages.Add("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder));
                        }
                    }
                    //else MessageBox.Show("Mod Found with incorrect capitalized letter for 'inventoryDescriptions.json'");
                }
            }

            // APPDATA MODS:

            try
            {
                string appdatamods = Directory.GetParent(blueprintdir) + "/Mods";

                foreach (string folder in System.IO.Directory.GetDirectories(appdatamods))
                {
                    int conflictusemod = 0; //(1 use old, 2 use new
                                            //modded blocks:
                    if (System.IO.Directory.Exists(folder + @"\Objects\Database\ShapeSets"))
                    {
                        foreach (string file in System.IO.Directory.GetFiles(folder + @"\Objects\Database\ShapeSets"))
                            if (System.IO.Path.GetExtension(file).ToLower() == ".json")
                            {
                                dynamic parts = null;
                                try
                                {
                                    parts = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(file));
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message + "\n\n" + file, "broken appdata mod!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                    parts = null;
                                }
                                if (parts != null)
                                    foreach (dynamic prop in parts)
                                    {
                                        foreach (dynamic part in parts[prop.Name])
                                        {
                                            try
                                            {
                                                dynamic moddesc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(folder + @"\description.json"));

                                                if (moddesc.name != null)
                                                {
                                                    string p = part.ToString();
                                                    blocks[part.uuid.ToString()] = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(p);
                                                    blocks[part.uuid.ToString()].Name = "unnamed shape " + part.uuid;
                                                    blocks[part.uuid.ToString()].Modfolder = folder;
                                                    blocks[part.uuid.ToString()].Mod = moddesc.name;
                                                    blocks[part.uuid.ToString()].textures = new Newtonsoft.Json.Linq.JArray();
                                                    if (part.renderable != null)
                                                    {

                                                        foreach (dynamic submesh in part.renderable.lodList[0].subMeshList)
                                                            if (submesh.material.ToString() == "Glass")
                                                                blocks[part.uuid.ToString()].glass = true;
                                                        foreach (string texture in part.renderable.lodList[0].subMeshList[0].textureList)
                                                            if (texture != "")
                                                                blocks[part.uuid.ToString()].textures.Add(folder + @"\Objects\Textures\" + (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(texture)) == "Textures" ? "" : System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(texture)) + "\\") + System.IO.Path.GetFileName(texture));
                                                        blocks[part.uuid.ToString()].mesh = folder + @"\Objects\Mesh\" + ((System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.renderable.lodList[0].mesh.ToString())) == "Mesh") ? "" : (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.renderable.lodList[0].mesh.ToString())) + "\\")) + System.IO.Path.GetFileName(part.renderable.lodList[0].mesh.ToString());
                                                        string test = blocks[part.uuid.ToString()].mesh;
                                                        if (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.renderable.lodList[0].mesh.ToString())) != "Mesh")
                                                        {
                                                        }
                                                    }
                                                    else
                                                    {//its a block!
                                                        blocks[part.uuid.ToString()].textures.Add(folder + @"\Objects\Textures\" + ((System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.dif.ToString())) == "Textures") ? "" : (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.dif.ToString())) + "\\")) + System.IO.Path.GetFileName(part.dif.ToString()));
                                                        blocks[part.uuid.ToString()].textures.Add(folder + @"\Objects\Textures\" + ((System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.asg.ToString())) == "Textures") ? "" : (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.asg.ToString())) + "\\")) + System.IO.Path.GetFileName(part.asg.ToString()));
                                                        blocks[part.uuid.ToString()].textures.Add(folder + @"\Objects\Textures\" + ((System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.nor.ToString())) == "Textures") ? "" : (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(part.nor.ToString())) + "\\")) + System.IO.Path.GetFileName(part.nor.ToString()));
                                                        blocks[part.uuid.ToString()].mesh = "";
                                                        dynamic t = blocks[part.uuid.ToString()].textures.ToString();
                                                        t = null;
                                                    }
                                                    amountloaded++;
                                                }
                                                else
                                                {

                                                    //MessageBox.Show("you have a broken mod!\nFile: " + file,"", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                                }

                                            }
                                            catch (Exception e)
                                            {
                                                MessageBox.Show(e.Message + "\n"+file, "Something wrong with a self-made mod", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                            }
                                        }
                                    }
                            }
                    }

                    //names:
                    if (System.IO.Directory.Exists(folder + @"\Gui\Language\English"))
                    {
                        foreach (string file in Directory.GetFiles(folder + @"\Gui\Language\English\"))
                            if (System.IO.File.Exists(folder + @"\Gui\Language\English\inventoryDescriptions.json"))
                            {
                                try
                                {

                                    inventoryitemdesc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(folder + @"\Gui\Language\English\inventoryDescriptions.json"));
                                    if (inventoryitemdesc != null)
                                        foreach (dynamic block in inventoryitemdesc)
                                            if (conflictusemod != 1 && blocks[block.Name.ToString()] != null)
                                            {
                                                string test = block.Value.title;
                                                blocks[block.Name.ToString()].Name = test;
                                            }

                                }
                                catch
                                {
                                    //workshoppages.Add("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder));
                                }
                            }
                        //else MessageBox.Show("Mod Found with incorrect capitalized letter for 'inventoryDescriptions.json'");
                    }
                }


            }
            catch (Exception e)
            { MessageBox.Show(e.Message,"Error while loading mods", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly); }


            if (workshoppages.Count !=0)
            {
                System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Modded blocks loaded: "+ amountloaded +"\n\nFound mods that weren't able to be loaded\nEither broken or made by a lazy dev\n\nRemove or Unsub\n\nOpen Workshop pages? (No=continue without this/these mod(s))", "Found unloadable mods!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    foreach (string page in workshoppages)
                    {
                        System.Diagnostics.Process.Start(page);
                    }
                }
            }
            this.Dispatcher.Invoke((Action)(() =>
            {//this refer to form in WPF application 
                gameblocks = blocks;
            }));
        }

        private void MenuItem_Click_11(object sender, RoutedEventArgs e)//mirror mode
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

                            realpos.pos.x = -Convert.ToInt32(block.pos.x) - Convert.ToInt32(block.bounds.x);
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

        Sphere_generator sphere_Generator;
        private void MenuItem_Click_12(object sender, RoutedEventArgs e) // sphere generator
        {
            if (sphere_Generator != null) sphere_Generator.Close();
            sphere_Generator = new Sphere_generator(this);
            sphere_Generator.Owner = this;
            sphere_Generator.Show();

        }

        private void menuitem_Click14(object sender, RoutedEventArgs e)//cuboid
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
                if (gameblocks[missingmod.ToString()] != null)
                {
                    dynamic block = gameblocks[missingmod.ToString()];
                    string modname = block.Mod;
                    string modid = System.IO.Path.GetFileName(block.Modfolder.ToString());
                    dynamic mod = new JObject();
                    mod.name = modname;
                    mod.ok = "✔";
                    mod.author = "Unavailable, service couldn't find it";
                    mod.url = "http://steamcommunity.com/workshop/filedetails/?id=" + modid;

                    Mods.mods.Add(mod);
                }
                else
                {
                    MessageBox.Show("couldn't find required mod for: " + missingmod.ToString());
                }
            }
            
            foreach (dynamic mod in Mods.mods)
            {
                string test = mod.url.ToString().Substring(51);
                if (this.mods[mod.url.ToString().Substring(51)] != null)
                    mod.ok = "✔";
                else
                    if(mod.ok == null)
                        mod.ok = "X";
            }

            new required_mods(Mods).Show();

            stream.Dispose();
            reader.Dispose();
        }


        Cuboid_Generator cuboid_Generator;
        private void MenuItem_Click_15(object sender, RoutedEventArgs e)
        {
            if (cuboid_Generator != null) cuboid_Generator.Close();
            cuboid_Generator = new Cuboid_Generator(this);
            cuboid_Generator.Owner = this;
            cuboid_Generator.Show();
        }

    }
}
