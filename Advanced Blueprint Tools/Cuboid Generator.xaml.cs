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
    /// Interaction logic for Cuboid_Generator.xaml
    /// </summary>
    public partial class Cuboid_Generator : Window
    {
        MainWindow mainWindow;
        public Cuboid_Generator(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://youtu.be/glLgQemUS2I?t=1627");
        }

        private void Button_Create_Click(object sender, RoutedEventArgs e)
        {
            int thickness = Convert.ToInt32(TextBox_thickness.Text);
            int sizeX = Convert.ToInt32(TextBox_X.Text);
            int sizeY = Convert.ToInt32(TextBox_Y.Text);
            int sizeZ = Convert.ToInt32(TextBox_Z.Text) - thickness;


            dynamic Cuboid = new JObject();
            Cuboid.bodies = new JArray();
            Cuboid.bodies.Add(new JObject());
            Cuboid.bodies[0].childs = new JArray();

            Cuboid.bodies[0].childs.Add(block(0, 0, 0, sizeX, sizeY, thickness));
            Cuboid.bodies[0].childs.Add(block(0, 0, thickness, sizeX, thickness, sizeZ));
            Cuboid.bodies[0].childs.Add(block(0, 0, thickness, thickness, sizeY, sizeZ));
            Cuboid.bodies[0].childs.Add(block(sizeX - thickness, thickness, thickness, thickness, sizeY - thickness, sizeZ - thickness));
            Cuboid.bodies[0].childs.Add(block(thickness, sizeY - thickness , thickness, sizeX - thickness, thickness, sizeZ - thickness));
            Cuboid.bodies[0].childs.Add(block(thickness, thickness, sizeZ, sizeX - thickness, sizeY - thickness, thickness));

            string message = "++ A Cuboid has been generated!";
            Random r = new Random();
            string blueprintdir = Database.User_ + "\\Blueprints" + "\\GeneratedCuboid-" + r.Next() + r.Next();
            dynamic description = new JObject();
            description.description = "generated cuboid";
            description.name = "generated cuboid" + r.Next();
            description.type = "Blueprint";
            description.version = 0;

            if (BP.Blueprintpath == null)
            {//no blueprint exists, initialize new one
                new BP(blueprintdir, Cuboid, description);
            }
            else
            {//a blueprint exists, overwrite it
                BP.setblueprint(Cuboid);
                BP.Description.description += message;
            }
            mainWindow.RenderBlueprint();
        }

        private dynamic block(int x, int y, int z, int bx, int by, int bz)
        {
            dynamic block = new JObject();
            block.color = "5DB7E7";
            block.pos = new JObject();
            block.pos.x = x;
            block.pos.y = y;
            block.pos.z = z;
            block.bounds = new JObject();
            block.bounds.x = bx;
            block.bounds.y = by;
            block.bounds.z = bz;
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
    }
}
