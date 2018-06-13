using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Advanced_Blueprint_Tools
{
    class BlueprintOptimizer
    {
        public static dynamic optimizeblueprint(dynamic blueprint)
        {
            dynamic optimizedblueprint = new JObject();

            //code





            return blueprint;
        }

        public static dynamic CreateBlueprintFromPoints(HashSet<Point3D> pointlist)
        {
            

            Dictionary<Point3D, Bounds> mergex = new Dictionary<Point3D, Bounds>();
            int i = 0;
            foreach(Point3D point in pointlist)
                if(!pointlist.Contains(new Point3D(point.X-1,point.Y, point.Z)))
                {
                    while (pointlist.Contains(new Point3D(point.X + i, point.Y, point.Z)))
                    {
                        //pointlist.Remove(new Point3D(point.X + i, point.Y, point.Z));
                        if (!mergex.ContainsKey(point))
                            mergex.Add(point, new Bounds { x = 1, y = 1, z = 1 });
                        else
                            mergex[point].x++;
                        i++;
                    }
                    i = 0;
                }
            pointlist.Clear();
            
            Dictionary<Point3D, Bounds> mergey = new Dictionary<Point3D, Bounds>();

            foreach(Point3D point in mergex.Keys)
                if(!(mergex.ContainsKey(new Point3D(point.X, point.Y-1, point.Z)) && mergex[new Point3D(point.X, point.Y, point.Z)].x == mergex[new Point3D(point.X, point.Y - 1, point.Z)].x))
                {
                    while (mergex.ContainsKey(new Point3D(point.X, point.Y + i, point.Z)) && 
                           mergex[new Point3D(point.X, point.Y + i, point.Z)].x == mergex[point].x)//x bound need to be same to merge
                    {
                        //mergex.Remove(neighbourkey);
                        if (!mergey.ContainsKey(point))
                            mergey.Add(point, mergex[point]);
                        else
                            mergey[point].y++;
                        i++;
                    }
                    i = 0;
                }
            mergex.Clear();
            

            Dictionary<Point3D, Bounds> mergez = new Dictionary<Point3D, Bounds>();

            foreach (Point3D point in mergey.Keys)
                if (!(mergey.ContainsKey(new Point3D(point.X, point.Y , point.Z-1)) && 
                    mergey[new Point3D(point.X, point.Y, point.Z)].x == mergey[new Point3D(point.X, point.Y, point.Z - 1)].x && 
                    mergey[new Point3D(point.X, point.Y, point.Z)].y == mergey[new Point3D(point.X, point.Y, point.Z - 1)].y))
                {
                    while (mergey.ContainsKey(new Point3D(point.X, point.Y, point.Z + i)) && 
                        mergey[new Point3D(point.X, point.Y, point.Z + i)].x == mergey[point].x && 
                        mergey[new Point3D(point.X, point.Y, point.Z + i)].y == mergey[point].y)
                    {
                        //mergex.Remove(neighbourkey);
                        if (!mergez.ContainsKey(point))
                            mergez.Add(point, mergey[point]);
                        else
                            mergez[point].z++;
                        i++;
                    }
                    i = 0;
                }
            
            mergey.Clear();



            dynamic blueprint = new JObject();
            blueprint.version = 1;
            blueprint.bodies = new JArray();
            blueprint.bodies.Add(new JObject());
            blueprint.bodies[0].childs = new JArray();

            int amountgenerated = 0;
            int negz = 1;//-1/1
            foreach(var keypair in mergez)
            {
                dynamic child = new JObject();
                child.color = "5DB7E7";
                child.pos = new JObject();
                child.pos.x = (int)Math.Floor(keypair.Key.X);
                child.pos.y = (int)Math.Floor(keypair.Key.Y);
                child.pos.z = (int)Math.Floor(keypair.Key.Z);
                child.bounds = new JObject();
                child.bounds.x = keypair.Value.x;
                child.bounds.y = keypair.Value.y;
                child.bounds.z = keypair.Value.z;
                child.shapeId = "a6c6ce30-dd47-4587-b475-085d55c6a3b4";
                child.xaxis = 1;
                child.zaxis = negz*3;
                blueprint.bodies[0].childs.Add(child);
                amountgenerated++;
            }

            return blueprint;
        }
        public static dynamic CreateBlueprintFromPointsAndColor(HashSet<Tuple<Point3D, string>> colorpoints)
        {


            Dictionary<Point3D, Bounds> mergex = new Dictionary<Point3D, Bounds>();
            int i = 0;
            int j = 0;
            foreach (var point in colorpoints)
                if (!colorpoints.Contains(new Tuple<Point3D, string>( new Point3D(point.Item1.X - 1, point.Item1.Y, point.Item1.Z), point.Item2)))
                {
                    while (colorpoints.Contains(new Tuple<Point3D, string>(new Point3D(point.Item1.X + i, point.Item1.Y, point.Item1.Z), point.Item2)))
                    {
                        //pointlist.Remove(new Point3D(point.X + i, point.Y, point.Z));

                        if (!mergex.ContainsKey(point.Item1))
                            mergex.Add(point.Item1, new Bounds { x = 1, y = 1, z = 1, color = point.Item2 });
                        else
                            mergex[point.Item1].x++;
                        i++;
                    }
                    i = 0;
                    j++;
                }
            colorpoints.Clear();

            Dictionary<Point3D, Bounds> mergey = new Dictionary<Point3D, Bounds>();

            foreach (Point3D point in mergex.Keys)
                if (!(mergex.ContainsKey(new Point3D(point.X, point.Y - 1, point.Z)) &&
                    mergex[new Point3D(point.X, point.Y, point.Z)].x == mergex[new Point3D(point.X, point.Y - 1, point.Z)].x &&
                    mergex[new Point3D(point.X, point.Y, point.Z)].color == mergex[new Point3D(point.X, point.Y - 1, point.Z)].color))
                {
                    while (mergex.ContainsKey(new Point3D(point.X, point.Y + i, point.Z)) &&
                           mergex[new Point3D(point.X, point.Y + i, point.Z)].x == mergex[point].x &&
                           mergex[new Point3D(point.X, point.Y + i, point.Z)].color == mergex[point].color)//x bound need to be same to merge
                    {
                        //mergex.Remove(neighbourkey);
                        if (!mergey.ContainsKey(point))
                            mergey.Add(point, mergex[point]);
                        else
                            mergey[point].y++;
                        i++;
                    }
                    i = 0;
                }
            mergex.Clear();


            Dictionary<Point3D, Bounds> mergez = new Dictionary<Point3D, Bounds>();

            foreach (Point3D point in mergey.Keys)
                if (!(mergey.ContainsKey(new Point3D(point.X, point.Y, point.Z - 1)) &&
                    mergey[new Point3D(point.X, point.Y, point.Z)].x == mergey[new Point3D(point.X, point.Y, point.Z - 1)].x &&
                    mergey[new Point3D(point.X, point.Y, point.Z)].y == mergey[new Point3D(point.X, point.Y, point.Z - 1)].y &&
                    mergey[new Point3D(point.X, point.Y, point.Z)].color == mergey[new Point3D(point.X, point.Y, point.Z - 1)].color))
                {
                    while (mergey.ContainsKey(new Point3D(point.X, point.Y, point.Z + i)) &&
                        mergey[new Point3D(point.X, point.Y, point.Z + i)].x == mergey[point].x &&
                        mergey[new Point3D(point.X, point.Y, point.Z + i)].y == mergey[point].y &&
                        mergey[new Point3D(point.X, point.Y, point.Z + i)].color == mergey[point].color)
                    {
                        //mergex.Remove(neighbourkey);
                        if (!mergez.ContainsKey(point))
                            mergez.Add(point, mergey[point]);
                        else
                            mergez[point].z++;
                        i++;
                    }
                    i = 0;
                }

            mergey.Clear();



            dynamic blueprint = new JObject();
            blueprint.version = 1;
            blueprint.bodies = new JArray();
            blueprint.bodies.Add(new JObject());
            blueprint.bodies[0].childs = new JArray();

            int amountgenerated = 0;
            int negz = 1;//-1/1
            foreach (var keypair in mergez)
            {
                dynamic child = new JObject();
                child.color = keypair.Value.color;
                child.pos = new JObject();
                child.pos.x = (int)Math.Floor(keypair.Key.X);
                child.pos.y = (int)Math.Floor(keypair.Key.Y);
                child.pos.z = (int)Math.Floor(keypair.Key.Z);
                child.bounds = new JObject();
                child.bounds.x = keypair.Value.x;
                child.bounds.y = keypair.Value.y;
                child.bounds.z = keypair.Value.z;
                child.shapeId = "a6c6ce30-dd47-4587-b475-085d55c6a3b4";
                child.xaxis = 1;
                child.zaxis = negz * 3;
                blueprint.bodies[0].childs.Add(child);
                amountgenerated++;
            }

            return blueprint;
        }
        public class Bounds
        {
            public string color;
           public int y;
           public int z;
           public int x;
        }
    }

}
