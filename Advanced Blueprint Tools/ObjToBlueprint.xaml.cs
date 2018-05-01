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



                /*int minx = int.MaxValue, maxx = int.MinValue, miny = int.MaxValue, maxy = int.MinValue, minz = int.MaxValue, maxz = int.MinValue;

               foreach(Mesh m in scene.Meshes)
                   foreach(Face face in m.Faces)
                       foreach (uint i in face.Indices)
                       {
                           double x = m.Vertices[i].X * scale;
                           double y = m.Vertices[i].Y * scale;
                           double z = m.Vertices[i].Z * scale;
                           if (x > maxx) maxx = (int)x;
                           if (y > maxy) maxx = (int)y;
                           if (z > maxz) maxx = (int)z;
                           if (y < miny) minx = (int)y;
                           if (z < minz) minx = (int)z;
                           if (x < minx) minx = (int)x;
                       }*/

                //bool[,,] array = new bool[maxx - minx+2, maxy - miny+2, maxz - minz+2];
                //int xref = minx-1, yref = miny-1, zref = minz-1;
                /*
                System.Windows.MessageBoxResult result = MessageBoxResult.Yes;
                if (maxx - minx > 300 || maxy - miny > 300 || maxz - minz > 300)
                    result = System.Windows.MessageBox.Show("Size: \nx: " + (maxx - minx) + "\ny: " + (maxy - miny) + "\nz: " + (maxz - minz) + "\n\nAre you sure you want to continue?","", System.Windows.MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)*/
                {
                    backgroundWorker = new BackgroundWorker();

                    backgroundWorker.WorkerSupportsCancellation = true;
                    progressWindow = new ProgressWindow(backgroundWorker);
                    progressWindow.Show();

                    backgroundWorker.WorkerReportsProgress = true;
                    backgroundWorker.DoWork += new DoWorkEventHandler(convertobj);
                    backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(conversioncompleted);
                    backgroundWorker.ProgressChanged += backgroundWorker1_ProgressChanged;
                    
                    backgroundWorker.RunWorkerAsync();

                    //blueprint = convertobj();
                    //conversioncompleted(null, null);
                    

                }
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
                            //minx = int.MaxValue; maxx = int.MinValue; miny = int.MaxValue; maxy = int.MinValue; minz = int.MaxValue; maxz = int.MinValue;

                            IList<Point3D> vertices = new List<Point3D>();
                            foreach (uint i in face.Indices)
                            {
                                double x = m.Vertices[i].X * scale;
                                double y = m.Vertices[i].Y * scale;
                                double z = m.Vertices[i].Z * scale;
                                vertices.Add(new Point3D(x, y, z));
                                pointlist.Add(new Point3D((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z)));
                            }
                            //splittriangles.AddRange(Splittriangles(new Triangle(vertices)));
                            Triangle triangle = new Triangle(vertices);
                        /*if (triangle.BoundsSmallerThan(bounds))
                            splittriangles.Add(triangle);
                        else
                            splittriangles.AddRange(Splittriangles(triangle));*/
                            if (!triangle.BoundsSmallerThan(bounds))
                                Splittriangles(triangle);
                            progress++;
                            backgroundWorker.ReportProgress((progress * 100) / total);
                        }

                        if (backgroundWorker.CancellationPending)
                            throw new OperationCanceledException();
                    }
                }
                catch(Exception ex)
                {
                    if(!(ex is OperationCanceledException))
                        System.Windows.MessageBox.Show(ex.Message, "Something went wrong converting the obj");
                }

            try
            {
                if (backgroundWorker.CancellationPending)
                    throw new OperationCanceledException();
                //dynamic pointlist = new JObject();
                //pointlist[x.tostring() + "." +...] = new jobject() { x y z};

                //can be split in 4 threads: 
                /*foreach(Triangle triange in splittriangles)//reduce amount of points,  as some triangles use same points
                {
                    foreach(Point3D point in triange.vertices)
                    {
                        if (!pointlist.Contains(new Point3D((int)Math.Floor(point.X), (int)Math.Floor(point.Y), (int)Math.Floor(point.Z))))
                            pointlist.Add(new Point3D((int)Math.Floor(point.X), (int)Math.Floor(point.Y), (int)Math.Floor(point.Z)));

                    }
                }*/
                dynamic blueprint = new JObject();
                int amountgenerated = 0;
                try
                {
                    blueprint.version = 1;
                    blueprint.bodies = new JArray();
                    blueprint.bodies.Add(new JObject());
                    blueprint.bodies[0].childs = new JArray();
                    foreach (Point3D point in pointlist)
                    {
                        dynamic child = new JObject();
                        child.color = "5DB7E7";
                        child.pos = new JObject();
                        child.pos.x = -(int)Math.Floor(point.X);
                        child.pos.y = (int)Math.Floor(point.Y);
                        child.pos.z = (int)Math.Floor(point.Z);
                        child.bounds = new JObject();
                        child.bounds.x = 1;
                        child.bounds.y = 1;
                        child.bounds.z = 1;
                        child.shapeId = "a6c6ce30-dd47-4587-b475-085d55c6a3b4";
                        child.xaxis = 1;
                        child.zaxis = 3;
                        blueprint.bodies[0].childs.Add(child);
                        amountgenerated++;
                    }
                }
                catch(Exception bpex)
                {
                    System.Windows.MessageBox.Show(bpex.Message, "Something went wrong building the blueprint");
                }
                string message = "converted obj to blueprint with " + amountgenerated + " blocks !";
                //MessageBox.Show(message+"\n\nOptimized to: "+count+" shapes");
                Random r = new Random();
                string blueprintpath = Database.User_ + "\\Blueprints\\Generatedblueprintobj-" + r.Next() + r.Next();
                dynamic description = new JObject();
                description.description = "generated obj blueprint with "+amountgenerated+" block";
                description.name = "generated blueprint" + r.Next();
                description.type = "Blueprint";
                description.version = 0;
                System.Windows.MessageBox.Show(message+"\nPLEASE WAIT for the rendering to complete");
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

        private List<Triangle> Splittriangles(Triangle triangle)
        {
            List<Triangle> triangles = new List<Triangle>();
            //split triangle in 4 triangles:
            Point3D mid1 = Average(triangle.vertices[0], triangle.vertices[1]); 
            Point3D mid2 = Average(triangle.vertices[1], triangle.vertices[2]);
            Point3D mid3 = Average(triangle.vertices[0], triangle.vertices[2]);
            triangles.Add(new Triangle(new List<Point3D> { mid1, mid2, mid3 }));
            triangles.Add(new Triangle(new List<Point3D> { mid1, mid3, triangle.vertices[0] }));
            triangles.Add(new Triangle(new List<Point3D> { mid1, mid2, triangle.vertices[1] }));
            triangles.Add(new Triangle(new List<Point3D> { mid2, mid3, triangle.vertices[2] }));

            //check for each triangle if bounds are smaller than 1x1x1, if not, split each again:
            List<Triangle> splittriangles = new List<Triangle>();
            foreach(Triangle splittriangle in triangles)
            {
                foreach(Point3D point in splittriangle.vertices)
                    pointlist.Add(new Point3D((int)Math.Floor(point.X), (int)Math.Floor(point.Y), (int)Math.Floor(point.Z)));

                if (splittriangle.BoundsSmallerThan(bounds))
                    splittriangles.Add(splittriangle);
                else
                    splittriangles.AddRange(Splittriangles(splittriangle));
            }
            return splittriangles;
        }
        private Point3D Average(Point3D point1, Point3D point2)
        {
            return new Point3D((point1.X + point2.X) / 2.0, (point1.Y + point2.Y) / 2.0, (point1.Z + point2.Z) / 2.0);
        }
        private void conversioncompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pointlist.Clear();
            progressWindow.Close();
            mainWindow.UpdateOpenedBlueprint();
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
            if (file != null && file != "")
            {
                this.file = file;
                objPath.Text = file;
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
