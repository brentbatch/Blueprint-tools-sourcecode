using Assimp;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Drawing;

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for ObjToBlueprint.xaml
    /// </summary>
    public partial class ObjToBlueprint : Window
    {
        MainWindow mainWindow;
        string file;
        string texture;
        Dictionary<Point3D, Point3D> coloredpoints;
        double scale=1;
        Assimp.Vector3D bounds = new Assimp.Vector3D(1, 1, 1);
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        ProgressWindow progressWindow;
        HashSet<Point3D> pointlist = new HashSet<Point3D>();//Tuple<Point3D,color>
        bool flipyz = false;
        bool flipxz = false;
        int flipz = 1;
        bool texturepossible = false;
        public ObjToBlueprint(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }
        

        private void objPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".obj";
            openFileDialog.Filter = "Obj files (*.obj)|*.obj|Mesh files (*.mesh)|*.mesh|Fbx Files(*.fbx)|*.fbx|stl files(*.stl)|*.stl|All Files(*.*)|*.*";
            openFileDialog.InitialDirectory = Database.ScrapData;

            openFileDialog.ShowDialog();

            string file = openFileDialog.FileName;
            if (file != null && file != "")
            {
                objPath.Text = file;
            }

        }
        private void objPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            string file = objPath.Text;
            if (file != null && file != "" && File.Exists(file))
            {
                this.file = file;
                //objPath.Text = file;

                new Thread(() =>
                {
                    try
                    {
                        this.texturepossible = false;
                        int minx = 1000000, maxx = -1000000, miny = 1000000, maxy = -1000000, minz = 1000000, maxz = -1000000;
                        Scene scene = new AssimpImporter().ImportFile(file);
                        foreach (Mesh m in scene.Meshes)
                        {
                            if (m.TextureCoordsChannelCount>0) this.texturepossible = true;
                            foreach (Assimp.Vector3D p in m.Vertices)
                            {
                                if (p.X < minx) minx = (int)p.X;
                                else if (p.X > maxx) maxx = (int)p.X;
                                if (p.Y < miny) miny = (int)p.Y;
                                else if (p.Y > maxy) maxy = (int)p.Y;
                                if (p.Z < minz) minz = (int)p.Z;
                                else if (p.Z > maxz) maxz = (int)p.Z;
                            }
                        }
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            BoundsBox.Content = @"(" + (maxx - minx) + "x" + (maxy - miny) + "x" + (maxz - minz) + ")";
                        }));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message, "Couldn't load mesh file");
                        this.file = null;
                    }
                }).Start();
            }
        }
        private void TexturePath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".png";
            openFileDialog.Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|All Files(*.*)|*.*";
            openFileDialog.InitialDirectory = Database.ScrapData;

            openFileDialog.ShowDialog();

            string file = openFileDialog.FileName;
            if (file != null && file != "")
            {
                TexturePath.Text = file;
            }
            

        }
        private void TexturePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            string file = TexturePath.Text;
            if (file != null && file != "" && File.Exists(file))
            {
                this.texture = file;
            }
            else this.texture = null;

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(this.file != null)
            {
                flipyz = flipYZ.IsChecked == true;
                flipxz = flipXZ.IsChecked == true;
                flipz = flipZ.IsChecked == true ? -1 : 1;

                backgroundWorker = new BackgroundWorker();
                backgroundWorker.WorkerSupportsCancellation = true;
                backgroundWorker.WorkerReportsProgress = true;
                progressWindow = new ProgressWindow(backgroundWorker, "Converting OBJ to Pointlist");
                progressWindow.Show();

                if(this.texture != null && this.texturepossible)
                    backgroundWorker.DoWork += new DoWorkEventHandler(Convertobjwtexture);
                else
                    backgroundWorker.DoWork += new DoWorkEventHandler(Convertobj);
                backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(conversioncompleted);
                backgroundWorker.ProgressChanged += BackgroundWorker1_ProgressChanged;
                    
                backgroundWorker.RunWorkerAsync();
                
            }
        }
        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressWindow.UpdateProgress(e.ProgressPercentage);
        }
        private void Convertobjwtexture(object sender, DoWorkEventArgs e)
        {
            try
            {
                Scene scene = new AssimpImporter().ImportFile(file, PostProcessSteps.Triangulate);

                coloredpoints = new Dictionary<Point3D, Point3D>();
                //List<Triangle> splittriangles = new List<Triangle>();
                //pointlist = new HashSet<Point3D>();


                //pointlist.ToDictionary<>
                int progress = 0;
                int total = 0;

                foreach (Mesh m in scene.Meshes)
                    foreach (Face face in m.Faces)
                        total++;
                foreach (Mesh m in scene.Meshes)
                {
                    var texturecoords = m.GetTextureCoords(0);

                    foreach (Face face in m.Faces)
                    {
                        
                        IList<Point3D> vertices = new List<Point3D>();
                        IList<Point3D> texturepoints = new List<Point3D>();
                        foreach (uint i in face.Indices)
                        {
                            double x = m.Vertices[i].X * scale;
                            double y = m.Vertices[i].Y * scale;
                            double z = m.Vertices[i].Z * scale;
                            Point3D point;
                            Assimp.Vector3D texturep = texturecoords[i];
                            Point3D texturepoint = new Point3D(texturep.X,1-texturep.Y,texturep.Z);
                            if (flipyz)
                            {
                                if (flipxz)
                                {
                                    point = new Point3D(y, z, flipz * x);
                                }
                                else
                                {
                                    point = new Point3D(x, z, flipz * y);
                                }
                            }
                            else
                            {
                                if (flipxz)
                                {
                                    point = new Point3D(z, y, flipz * x);
                                }
                                else
                                    point = new Point3D(x, y, flipz * z);
                            }
                            vertices.Add(point);
                            texturepoints.Add(texturepoint);
                            Point3D flooredpoint = new Point3D((int)Math.Floor(point.X), (int)Math.Floor(point.Y), (int)Math.Floor(point.Z));
                            if (!coloredpoints.ContainsKey(flooredpoint))
                                coloredpoints.Add(flooredpoint, texturepoint);
                        }
                        if (vertices.Count == 3)
                        {
                            ColorTriangle triangle = new ColorTriangle(vertices,texturepoints);
                            if (!triangle.BoundsSmallerThan(bounds))
                                Splitcolortriangles(triangle);
                        }
                        else
                        { }
                        progress++;
                        backgroundWorker.ReportProgress((progress * 100) / total);
                        if (backgroundWorker.CancellationPending)
                            e.Cancel = true;
                    }

                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                    System.Windows.MessageBox.Show(ex.Message, "Something went wrong converting the obj+texture");
                else
                    e.Cancel = true;
            }
            
            try
            {
                if (backgroundWorker.CancellationPending)
                    throw new OperationCanceledException();

                dynamic blueprint = new JObject();
                try
                {
                    Bitmap texturemap = new Bitmap(this.texture);
                    HashSet<Tuple<Point3D, string>> pointswithcolor = new HashSet<Tuple<Point3D, string>>();
                    int width = texturemap.Width;
                    int height = texturemap.Height;
                    foreach (var pair in coloredpoints)
                    {
                        int x = (int)(pair.Value.X * width);
                        int y = (int)(pair.Value.Y * height);
                        if (pair.Value.Y > 1)
                        {

                        }
                        while (y < 0)
                            y ++;
                        while (x < 0)
                            x ++;
                        if(Math.Abs(x)>width/2)
                        {

                        }
                        while (x >= width)
                            x -= width;
                        while (y>= height)
                            y -= height;
                        System.Drawing.Color color = texturemap.GetPixel(x , y);
                        string c = color.Name.Substring(2);
                        pointswithcolor.Add(new Tuple<Point3D, string>(pair.Key, c));
                    }
                    coloredpoints.Clear();
                    blueprint = BlueprintOptimizer.CreateBlueprintFromPointsAndColor(pointswithcolor);
                }
                catch (Exception bpex)
                {
                    System.Windows.MessageBox.Show(bpex.Message, "Something went wrong building the blueprint");
                }
                int amountgenerated = blueprint.bodies[0].childs.Count;
                string message = "converted obj to blueprint with " + amountgenerated + " blocks !";
                //MessageBox.Show(message+"\n\nOptimized to: "+count+" shapes");
                Random r = new Random();
                string blueprintpath = Database.User_ + "\\Blueprints\\Generatedblueprintobj-" + r.Next() + r.Next();
                dynamic description = new JObject();
                description.description = "generated obj blueprint with " + amountgenerated + " block";
                description.name = "generated blueprint" + r.Next();
                description.type = "Blueprint";
                description.version = 0;
                new Task(() =>
                {
                    System.Windows.MessageBox.Show(message + "\nPLEASE WAIT for the rendering to complete");
                }).Start();
                if (BP.Blueprintpath == null)
                {//no blueprint exists, initialize new one
                    new BP(blueprintpath, blueprint, description);
                }
                else
                {//overwrite current blueprint
                    BP.setblueprint(blueprint);
                    BP.Description.description += message;
                }
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show(exc.Message, "error");
            }


        }
        private void Splitcolortriangles(ColorTriangle triangle)
        {
            if (backgroundWorker.CancellationPending)
                throw new OperationCanceledException();
            //split triangle in 4 triangles:
            Point3D mid1 = Average(triangle.vertices[0], triangle.vertices[1]);
            Point3D mid2 = Average(triangle.vertices[1], triangle.vertices[2]);
            Point3D mid3 = Average(triangle.vertices[0], triangle.vertices[2]);
            Point3D tmid1 = Average(triangle.texturecoords[0], triangle.texturecoords[1]);
            Point3D tmid2 = Average(triangle.texturecoords[1], triangle.texturecoords[2]);
            Point3D tmid3 = Average(triangle.texturecoords[0], triangle.texturecoords[2]);
            List<ColorTriangle> triangles = new List<ColorTriangle>
            {
                new ColorTriangle(new List<Point3D> { mid1, mid2, mid3 }, new List<Point3D>{ tmid1, tmid2, tmid3 }),
                new ColorTriangle(new List<Point3D> { mid1, mid3, triangle.vertices[0] },new List<Point3D> { tmid1, tmid3, triangle.texturecoords[0] }),
                new ColorTriangle(new List<Point3D> { mid1, mid2, triangle.vertices[1] },new List<Point3D> { tmid1, tmid2, triangle.texturecoords[1] }),
                new ColorTriangle(new List<Point3D> { mid2, mid3, triangle.vertices[2] },new List<Point3D> { tmid2, tmid3, triangle.texturecoords[2] })
            };

            Point3D flooredpoint1 = new Point3D((int)Math.Floor(mid1.X), (int)Math.Floor(mid1.Y), (int)Math.Floor(mid1.Z));
            if (!coloredpoints.ContainsKey(flooredpoint1))
                coloredpoints.Add(flooredpoint1, tmid1);
            Point3D flooredpoint2 = new Point3D((int)Math.Floor(mid2.X), (int)Math.Floor(mid2.Y), (int)Math.Floor(mid2.Z));
            if (!coloredpoints.ContainsKey(flooredpoint2))
                coloredpoints.Add(flooredpoint2, tmid2);
            Point3D flooredpoint3 = new Point3D((int)Math.Floor(mid3.X), (int)Math.Floor(mid3.Y), (int)Math.Floor(mid3.Z));
            if (!coloredpoints.ContainsKey(flooredpoint3))
                coloredpoints.Add(flooredpoint3, tmid3);
           // coloredpoints.Add(new Tuple<Point3D,Point3D>( new Point3D((int)Math.Floor(mid1.X), (int)Math.Floor(mid1.Y), (int)Math.Floor(mid1.Z)),tmid1));
           // coloredpoints.Add(new Tuple<Point3D,Point3D>( new Point3D((int)Math.Floor(mid2.X), (int)Math.Floor(mid2.Y), (int)Math.Floor(mid2.Z)),tmid2));
           // coloredpoints.Add(new Tuple<Point3D,Point3D>( new Point3D((int)Math.Floor(mid3.X), (int)Math.Floor(mid3.Y), (int)Math.Floor(mid3.Z)),tmid3));
            //triangles.Add(new Triangle(new List<Point3D> { mid1, mid2, mid3 }));
            //triangles.Add(new Triangle(new List<Point3D> { mid1, mid3, triangle.vertices[0] }));
            //triangles.Add(new Triangle(new List<Point3D> { mid1, mid2, triangle.vertices[1] }));
            //triangles.Add(new Triangle(new List<Point3D> { mid2, mid3, triangle.vertices[2] }));
            //check for each triangle if bounds are smaller than 1x1x1, if not, split each again:
            foreach (ColorTriangle splittriangle in triangles)
            {
                // foreach(Point3D point in splittriangle.vertices)
                //   pointlist.Add(new Point3D((int)Math.Floor(point.X), (int)Math.Floor(point.Y), (int)Math.Floor(point.Z)));

                if (!splittriangle.BoundsSmallerThan(bounds))
                    Splitcolortriangles(splittriangle);
            }
            //return splittriangles;
        }

        private void Convertobj(object sender, DoWorkEventArgs e)
        {

            Scene scene = new AssimpImporter().ImportFile(file, PostProcessSteps.Triangulate);

            //List<Triangle> splittriangles = new List<Triangle>();
            pointlist = new HashSet<Point3D>();
            //pointlist.ToDictionary<>
            int progress = 0;
            int total = 0;
            try
            {
                

                foreach (Mesh m in scene.Meshes)
                    foreach (Face face in m.Faces)
                        total++;
                foreach (Mesh m in scene.Meshes)
                {

                    foreach (Face face in m.Faces)
                    {
                        IList<Point3D> vertices = new List<Point3D>();
                        foreach (uint i in face.Indices)
                        {
                            double x = m.Vertices[i].X * scale;
                            double y = m.Vertices[i].Y * scale;
                            double z = m.Vertices[i].Z * scale;
                            Point3D point;
                            if (flipyz)
                            {
                                if(flipxz)
                                {
                                    point = new Point3D(y,z, flipz * x);
                                }
                                else
                                {
                                    point = new Point3D(x,z, flipz * y);
                                }
                            }
                            else
                            {
                                if (flipxz)
                                {
                                    point = new Point3D(z,y, flipz * x);
                                }
                                else
                                    point = new Point3D(x,y, flipz * z);
                            }
                            vertices.Add(point);
                            pointlist.Add(new Point3D((int)Math.Floor(point.X), (int)Math.Floor(point.Y), (int)Math.Floor(point.Z)));
                        }
                        if (vertices.Count == 3)
                        {
                            Triangle triangle = new Triangle(vertices);
                            if (!triangle.BoundsSmallerThan(bounds))
                                Splittriangles(triangle);
                        }
                        else
                        { }
                        progress++;
                        backgroundWorker.ReportProgress((progress * 100) / total);
                        if (backgroundWorker.CancellationPending)
                            e.Cancel = true;
                    }

                }
            }
            catch(Exception ex)
            {
                if(!(ex is OperationCanceledException))
                    System.Windows.MessageBox.Show(ex.Message, "Something went wrong converting the obj");
                else
                    e.Cancel = true;
            }

            try
            {
                if (backgroundWorker.CancellationPending)
                    throw new OperationCanceledException();

                dynamic blueprint = new JObject();
                try
                {
                    blueprint = BlueprintOptimizer.CreateBlueprintFromPoints(pointlist);
                }
                catch(Exception bpex)
                {
                    System.Windows.MessageBox.Show(bpex.Message, "Something went wrong building the blueprint");
                }
                int amountgenerated = blueprint.bodies[0].childs.Count;
                string message = "converted obj to blueprint with " + amountgenerated + " blocks !";
                //MessageBox.Show(message+"\n\nOptimized to: "+count+" shapes");
                Random r = new Random();
                string blueprintpath = Database.User_ + "\\Blueprints\\Generatedblueprintobj-" + r.Next() + r.Next();
                dynamic description = new JObject();
                description.description = "generated obj blueprint with "+amountgenerated+" block";
                description.name = "generated blueprint" + r.Next();
                description.type = "Blueprint";
                description.version = 0;
                new Task(() =>
                {
                    System.Windows.MessageBox.Show(message + "\nPLEASE WAIT for the rendering to complete");
                }).Start();
                if (BP.Blueprintpath == null)
                {//no blueprint exists, initialize new one
                    new BP(blueprintpath, blueprint, description);
                }
                else
                {//overwrite current blueprint
                    BP.setblueprint(blueprint);
                    BP.Description.description += message;
                }
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show(exc.Message, "error");
            }

        }

        
        private void Splittriangles(Triangle triangle)
        {
            if (backgroundWorker.CancellationPending)
                throw new OperationCanceledException();
            //split triangle in 4 triangles:
            Point3D mid1 = Average(triangle.vertices[0], triangle.vertices[1]); 
            Point3D mid2 = Average(triangle.vertices[1], triangle.vertices[2]);
            Point3D mid3 = Average(triangle.vertices[0], triangle.vertices[2]);
            List<Triangle> triangles = new List<Triangle>
            {
                new Triangle(new List<Point3D> { mid1, mid2, mid3 }),
                new Triangle(new List<Point3D> { mid1, mid3, triangle.vertices[0] }),
                new Triangle(new List<Point3D> { mid1, mid2, triangle.vertices[1] }),
                new Triangle(new List<Point3D> { mid2, mid3, triangle.vertices[2] })
            };
            pointlist.Add(new Point3D((int)Math.Floor(mid1.X), (int)Math.Floor(mid1.Y), (int)Math.Floor(mid1.Z)));
            pointlist.Add(new Point3D((int)Math.Floor(mid2.X), (int)Math.Floor(mid2.Y), (int)Math.Floor(mid2.Z)));
            pointlist.Add(new Point3D((int)Math.Floor(mid3.X), (int)Math.Floor(mid3.Y), (int)Math.Floor(mid3.Z)));
            //triangles.Add(new Triangle(new List<Point3D> { mid1, mid2, mid3 }));
            //triangles.Add(new Triangle(new List<Point3D> { mid1, mid3, triangle.vertices[0] }));
            //triangles.Add(new Triangle(new List<Point3D> { mid1, mid2, triangle.vertices[1] }));
            //triangles.Add(new Triangle(new List<Point3D> { mid2, mid3, triangle.vertices[2] }));
            //check for each triangle if bounds are smaller than 1x1x1, if not, split each again:
            foreach (Triangle splittriangle in triangles)
            {
               // foreach(Point3D point in splittriangle.vertices)
                 //   pointlist.Add(new Point3D((int)Math.Floor(point.X), (int)Math.Floor(point.Y), (int)Math.Floor(point.Z)));

                if (!splittriangle.BoundsSmallerThan(bounds))
                    Splittriangles(splittriangle);
            }
            //return splittriangles;
        }

        private Point3D Average(Point3D point1, Point3D point2)
        {
            return new Point3D((point1.X + point2.X) / 2.0, (point1.Y + point2.Y) / 2.0, (point1.Z + point2.Z) / 2.0);
        }

        private void conversioncompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressWindow.Close();
            pointlist.Clear();
            if (!e.Cancelled)
            {
                mainWindow.RenderBlueprint();
            }
            backgroundWorker.Dispose();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            System.Windows.Controls.TextBox t = (System.Windows.Controls.TextBox)sender;
            try
            {
                if (Convert.ToDouble(t.Text) >= 0) { }
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
            this.scale = Convert.ToDouble(t.Text);
        }


    }



    public class Triangle
    {
        public IList<Point3D> vertices;

        public Triangle(IList<Point3D> vertices)
        {
            this.vertices = vertices;
        }
        public bool BoundsSmallerThan(Assimp.Vector3D bounds)
        {
            return (Math.Abs(vertices[0].X - vertices[1].X) < bounds.X && Math.Abs(vertices[0].X - vertices[2].X) < bounds.X && Math.Abs(vertices[2].X - vertices[1].X) < bounds.X &&
                    Math.Abs(vertices[0].Y - vertices[1].Y) < bounds.Y && Math.Abs(vertices[0].Y - vertices[2].Y) < bounds.Y && Math.Abs(vertices[2].Y - vertices[1].Y) < bounds.Y &&
                    Math.Abs(vertices[0].Z - vertices[1].Z) < bounds.Z && Math.Abs(vertices[0].Z - vertices[2].Z) < bounds.Z && Math.Abs(vertices[2].Z - vertices[1].Z) < bounds.Z);
        }
    }

    public class ColorTriangle : Triangle
    {
        public IList<Point3D> texturecoords;

        public ColorTriangle(IList<Point3D> vertices, IList<Point3D> texturecoords) : base(vertices)
        {
            this.texturecoords = texturecoords;
        }
    }
}
