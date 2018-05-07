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

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for ObjToBlueprint.xaml
    /// </summary>
    public partial class ObjToBlueprint : Window
    {
        MainWindow mainWindow;
        string file;
        double scale=1;
        Assimp.Vector3D bounds = new Assimp.Vector3D(1, 1, 1);
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        ProgressWindow progressWindow;
        HashSet<Point3D> pointlist = new HashSet<Point3D>();
        bool flipyz = false;
        bool flipxz = false;
        int flipz = 1;
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

                backgroundWorker.DoWork += new DoWorkEventHandler(convertobj);
                backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(conversioncompleted);
                backgroundWorker.ProgressChanged += backgroundWorker1_ProgressChanged;
                    
                backgroundWorker.RunWorkerAsync();
                
            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressWindow.UpdateProgress(e.ProgressPercentage);
        }
        
        private void convertobj(object sender, DoWorkEventArgs e)
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
                                    point = new Point3D((int)Math.Floor(y), (int)Math.Floor(z), flipz * (int)Math.Floor(x));
                                }
                                else
                                {
                                    point = new Point3D((int)Math.Floor(x), (int)Math.Floor(z), flipz * (int)Math.Floor(y));
                                }
                            }
                            else
                            {
                                if (flipxz)
                                {
                                    point = new Point3D((int)Math.Floor(z), (int)Math.Floor(y), flipz * (int)Math.Floor(x));
                                }
                                else
                                    point = new Point3D((int)Math.Floor(x), (int)Math.Floor(y), flipz * (int)Math.Floor(z));
                            }
                            vertices.Add(point);
                            pointlist.Add(point);
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

        private void objPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            string file = objPath.Text;
            if (file != null && file != ""&& File.Exists(file))
            {
                this.file = file;
                objPath.Text = file;
                
                new Thread(() =>
                {
                    try
                    {

                        int minx = 1000000, maxx = -1000000, miny = 1000000, maxy = -1000000, minz = 1000000, maxz = -1000000;
                        Scene scene = new AssimpImporter().ImportFile(file);
                        foreach (Mesh m in scene.Meshes)
                            foreach (Assimp.Vector3D p in m.Vertices)
                            {
                                if (p.X < minx) minx = (int)p.X;
                                else if (p.X > maxx) maxx = (int)p.X;
                                if (p.Y < miny) miny = (int)p.Y;
                                else if (p.Y > maxy) maxy = (int)p.Y;
                                if (p.Z < minz) minz = (int)p.Z;
                                else if (p.Z > maxz) maxz = (int)p.Z;
                            }
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            BoundsBox.Content = @"(" + (maxx - minx) + "x" + (maxy - miny) + "x" + (maxz - minz) + ")";
                        }));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                        this.file = null;
                    }
                }).Start();
            }
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
}
