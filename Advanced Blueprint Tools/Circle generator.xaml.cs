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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for Circle_generator.xaml
    /// </summary>
    public partial class Circle_generator : Window
    {

        private MainWindow mainwindow;
        public Circle_generator(MainWindow window)
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
            //float Z = Convert.ToInt32(TextBox_Z.Text);
            float t = 0.4f + (float)Convert.ToDouble(TextBox_thickness.Text);

            dynamic circle = new JObject();

            HashSet<Point3D> points = new HashSet<Point3D>();

            double precision = 0.95;
            double pp = 0;

            for (float x = -X; x < X; x++)
                for (float y = -Y; y < Y; y++)
                    if ((Math.Pow(x / X, 2) + Math.Pow(y / Y, 2)) < precision &&
                        (Math.Pow((x ) / (X - t), 2) + Math.Pow((y ) / (Y - t), 2)) > (precision-pp))
                    {
                        points.Add(new Point3D(x, y, 1));
                    }


            circle = BlueprintOptimizer.CreateBlueprintFromPoints(points);


            if (circle.bodies[0].childs.Count > 0)
            {
                string message = "++ An ellipse/circle with " + circle.bodies[0].childs.Count + " shapes has been generated!";
                Random r = new Random();
                string blueprintpath = Database.User_ + "\\Blueprints\\Generatedellipse-" + r.Next() + r.Next();
                dynamic description = new JObject();
                description.description = "generated ellipsoid";
                description.name = "generated ellipse/circle" + r.Next();
                description.type = "Blueprint";
                description.version = 0;

                if (BP.Blueprintpath == null)
                {//no blueprint exists, initialize new one
                    new BP(blueprintpath, circle, description);
                }
                else
                {//overwrite current blueprint
                    BP.setblueprint(circle);
                    BP.Description.description += message;
                }
                mainwindow.RenderBlueprint();
            }
            else
            {
                MessageBox.Show("no circle has been generated");
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
                if (Convert.ToDouble(t.Text) < 0 ){ t.Text = "0"; }
            }
            catch
            {
                t.Text = "0";
            }
        }
    }
}
