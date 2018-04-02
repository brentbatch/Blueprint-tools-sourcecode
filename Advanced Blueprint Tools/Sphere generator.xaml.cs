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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for Sphere_generator.xaml
    /// </summary>
    public partial class Sphere_generator : Window
    {
        private MainWindow mainwindow;
        public Sphere_generator(MainWindow window)
        {
            this.mainwindow = window;
            InitializeComponent();
        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://youtu.be/glLgQemUS2I?t=1479");
        }

        private void Button_Create_Click(object sender, RoutedEventArgs e)
        {
            float X = Convert.ToInt32(TextBox_X.Text);
            float Y = Convert.ToInt32(TextBox_Y.Text);
            float Z = Convert.ToInt32(TextBox_Z.Text);
            float t = Convert.ToInt32(TextBox_thickness.Text);

            dynamic Sphere = new JObject();
            Sphere.bodies = new JArray();
            Sphere.bodies.Add(new JObject() as dynamic);
            Sphere.bodies[0].childs = new JArray();

            dynamic blocksXYZ = new JObject();
            int amountgenerated=0;
            for (float x = -X; x < X; x++)
                for (float y = -Y; y < Y; y++)
                    for (float z = -Z; z < Z; z++)
                    {

                        if ((Math.Pow(x / X, 2) + Math.Pow(y / Y, 2) + Math.Pow(z / Z, 2)) < 0.95 &&
                            (Math.Pow((x + (x > 0 ? t : -t)) / X, 2) + Math.Pow((y + (y > 0 ? t : -t)) / Y, 2) + Math.Pow((z + (z > 0 ? t : -t)) / Z, 2)) > 0.95)
                        {
                            if (blocksXYZ[x.ToString()] == null)
                                blocksXYZ[x.ToString()] = new JObject();
                            if (blocksXYZ[x.ToString()][y.ToString()] == null)
                                blocksXYZ[x.ToString()][y.ToString()] = new JObject();
                            if (blocksXYZ[x.ToString()][y.ToString()][z.ToString()] == null)
                                blocksXYZ[x.ToString()][y.ToString()][z.ToString()] = new JObject();
                            dynamic bounds = new JObject();
                            bounds.x = 1;
                            bounds.y = 1;
                            bounds.z = 1;
                            blocksXYZ[x.ToString()][y.ToString()][z.ToString()].bounds = bounds;
                            amountgenerated++;
                        }
                    }

            foreach (dynamic x in blocksXYZ)
                foreach (dynamic y in x.Value)
                    foreach (dynamic z in y.Value)
                    {
                        dynamic bounds = z.Value.bounds;
                        if (bounds != null)
                        {
                            for (int i = Convert.ToInt32(z.Name) + 1; i < 64; i++)
                            {
                                if (blocksXYZ[x.Name][y.Name][i.ToString()] != null && blocksXYZ[x.Name][y.Name][i.ToString()].bounds != null)
                                {
                                    blocksXYZ[x.Name][y.Name][i.ToString()].bounds = null;

                                    blocksXYZ[x.Name][y.Name][z.Name].bounds.z = Convert.ToInt32(blocksXYZ[x.Name][y.Name][z.Name].bounds.z) + 1;
                                }
                                else
                                    break;
                            }
                        }
                    }

            foreach (dynamic x in blocksXYZ)
                foreach (dynamic y in x.Value)
                    foreach (dynamic z in y.Value)
                    {
                        dynamic bounds = z.Value.bounds;
                        if (bounds != null)
                        {
                            for (int i = Convert.ToInt32(y.Name) + 1; i < 64; i++)
                            {
                                if (blocksXYZ[x.Name][i.ToString()] != null && blocksXYZ[x.Name][i.ToString()][z.Name] != null &&
                                    blocksXYZ[x.Name][i.ToString()][z.Name].bounds != null &&
                                    blocksXYZ[x.Name][i.ToString()][z.Name].bounds.z == blocksXYZ[x.Name][y.Name][z.Name].bounds.z)
                                {
                                    blocksXYZ[x.Name][i.ToString()][z.Name].bounds = null;

                                    blocksXYZ[x.Name][y.Name][z.Name].bounds.y = Convert.ToInt32(blocksXYZ[x.Name][y.Name][z.Name].bounds.y) + 1;
                                }
                                else
                                    break;
                            }
                        }
                    }
            foreach (dynamic x in blocksXYZ)
                foreach (dynamic y in x.Value)
                    foreach (dynamic z in y.Value)
                    {
                        dynamic bounds = z.Value.bounds;
                        if (bounds != null)
                        {
                            for (int i = Convert.ToInt32(x.Name) + 1; i < 64; i++)
                            {
                                if (blocksXYZ[i.ToString()] != null && blocksXYZ[i.ToString()][y.Name] != null && blocksXYZ[i.ToString()][y.Name][z.Name] != null &&
                                    blocksXYZ[i.ToString()][y.Name][z.Name].bounds != null &&
                                    blocksXYZ[i.ToString()][y.Name][z.Name].bounds.z == blocksXYZ[x.Name][y.Name][z.Name].bounds.z &&
                                    blocksXYZ[i.ToString()][y.Name][z.Name].bounds.y == blocksXYZ[x.Name][y.Name][z.Name].bounds.y)
                                {
                                    blocksXYZ[i.ToString()][y.Name][z.Name].bounds = null;

                                    blocksXYZ[x.Name][y.Name][z.Name].bounds.x = Convert.ToInt32(blocksXYZ[x.Name][y.Name][z.Name].bounds.x) + 1;
                                }
                                else
                                    break;
                            }
                        }
                    }


            foreach (dynamic x in blocksXYZ)
                foreach (dynamic y in x.Value)
                    foreach (dynamic z in y.Value)
                        if(z.Value.bounds != null)
                            Sphere.bodies[0].childs.Add(block(Convert.ToInt32(x.Name), Convert.ToInt32(y.Name), Convert.ToInt32(z.Name), z.Value.bounds));

            if (amountgenerated > 0)
            {
                string message = "++ An ellipsoid with " + amountgenerated + " blocks has been generated!";
                //MessageBox.Show(message+"\n\nOptimized to: "+count+" shapes");
                Random r = new Random();
                string blueprintdir = this.mainwindow.blueprintdir+"\\GeneratedEllipsoid-"+r.Next()+r.Next();
                dynamic description = new JObject();
                description.description = "generated ellipsoid";
                description.name = "generated sphere" + r.Next();
                description.type = "Blueprint";
                description.version = 0;

                if (mainwindow.OpenedBlueprint== null)
                {
                    mainwindow.OpenedBlueprint = new BP(blueprintdir, Sphere, description);
                }
                else
                {
                    mainwindow.OpenedBlueprint.blueprint = Sphere;
                }
                mainwindow.OpenedBlueprint.setblueprint(mainwindow.OpenedBlueprint.blueprint);

                mainwindow.OpenedBlueprint.description.description = message;
                mainwindow.UpdateOpenedBlueprint();
            }
            else
            {
                MessageBox.Show("no ellipsoid has been generated");
            }

        }
        private dynamic block(int x, int y, int z, dynamic bounds)
        {
            dynamic block = new JObject();
            block.color = "5DB7E7";
            block.pos = new JObject();
            block.pos.x = x;
            block.pos.y = y;
            block.pos.z = z;
            block.bounds = new JObject();
            block.bounds.x = bounds.x;
            block.bounds.y = bounds.y;
            block.bounds.z = bounds.z;
            block.shapeId = "a6c6ce30-dd47-4587-b475-085d55c6a3b4";
            block.xaxis = 1;
            block.zaxis = 3;
            return block;
        }

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
    }
}
