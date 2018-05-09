using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Advanced_Blueprint_Tools
{
    public class BP
    {
        public static string Blueprintpath { get; private set; } 

        public static dynamic Blueprint { get; private set; }

        public static dynamic Description { get; private set; }

        public static PngBitmapEncoder Icon { get; set; }

        private static Dictionary<int, dynamic> Wires { get; set; }

        private static HashSet<string> Useduuids = new HashSet<string>();
        
        public static bool missingmod = false;
        public static bool brokenrotation = false;

        public static void setblueprint(dynamic bp)
        {
            Blueprint = bp;
            missingmod = false;
            brokenrotation = false;
            Useduuids.Clear();
            GetUsedUuids();
            Wires = new Dictionary<int, dynamic>();
        }

        public BP(string bppath, dynamic bp, dynamic desc)
        {
            Blueprintpath = bppath;
            Blueprint = bp;
            Description = desc;
            setblueprint(bp);
        }

        public BP(string bppath)
        {
            Blueprint = null;
            Description = null;

            Blueprintpath = bppath;
            //load directory of blueprints
            try
            {
                Blueprint = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(Blueprintpath + @"\blueprint.json"));
                Description = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(Blueprintpath + @"\description.json"));

                setblueprint(Blueprint);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                
            }
        }

        public static HashSet<string> GetUsedUuids()
        {
            if (Useduuids.Count > 0) return Useduuids;

            Useduuids = new HashSet<string>();

            if (Blueprint.bodies != null)
                foreach (dynamic body in Blueprint.bodies)
                    foreach (dynamic child in body.childs)
                            Useduuids.Add(child.shapeId.ToString());
            if (Blueprint.joints != null)
                foreach(dynamic joint in Blueprint.joints)
                        Useduuids.Add(joint.shapeId.ToString());
            return Useduuids;
        }

        public static Dictionary<string, Tuple<string, BitmapSource, int>> GetUsedPartsList()
        {
            Dictionary<string, int> uuidamounts = new Dictionary<string, int>();
            try
            {

                if (Blueprint.bodies != null)
                    foreach (dynamic body in Blueprint.bodies)
                        foreach (dynamic child in body.childs)
                        {
                            int modifier = 1;
                            if (child.bounds != null)
                                modifier = Convert.ToInt32(child.bounds.x.ToString()) * Convert.ToInt32(child.bounds.y.ToString()) * Convert.ToInt32(child.bounds.z.ToString());
                            string uuid = child.shapeId.ToString();
                            if (!uuidamounts.ContainsKey(uuid))
                                uuidamounts.Add(uuid, modifier);
                            else
                                uuidamounts[uuid]+=modifier;
                        }
                if (Blueprint.joints != null)
                    foreach (dynamic joint in Blueprint.joints)
                    {
                        string uuid = joint.shapeId.ToString();
                        if (!uuidamounts.ContainsKey(uuid))
                            uuidamounts.Add(uuid, 1);
                        else
                            uuidamounts[uuid]++;
                    }
                Dictionary<string, Tuple<string, BitmapSource, int>> list = new Dictionary<string, Tuple<string, BitmapSource, int>>();
                foreach (string uuid in uuidamounts.Keys)
                {
                    if (Database.blocks.ContainsKey(uuid))
                    {
                        list.Add(uuid, new Tuple<string, BitmapSource, int>(Database.blocks[uuid].Name, null, uuidamounts[uuid]));
                    }
                }

                return list;
            }
            catch { return null; }
        }

        public static Dictionary<int, dynamic> GetWires()
        {
            Wires = new Dictionary<int, dynamic>();

            if (Blueprint.bodies != null)
                foreach (dynamic body in Blueprint.bodies)
                    foreach (dynamic child in body.childs)
                        if (child.controller != null && child.controller.id != null)
                        {
                            dynamic correctedchild = getposandbounds(child);
                            dynamic wire = new JObject();
                            wire.pos = new JObject()
                            {
                                ["x"]= Convert.ToInt32(correctedchild.pos.x.ToString()) + Convert.ToInt32(correctedchild.bounds.x.ToString()) / 2.0,
                                ["y"]= Convert.ToInt32(correctedchild.pos.y.ToString()) + Convert.ToInt32(correctedchild.bounds.y.ToString()) / 2.0,
                                ["z"]= Convert.ToInt32(correctedchild.pos.z.ToString()) + Convert.ToInt32(correctedchild.bounds.z.ToString()) / 2.0
                            };
                            string color = child.color.ToString();
                            if (color.StartsWith("#"))
                                color = color.Substring(1);
                            wire.color = color;
                            wire.connections = child.controller.controllers;
                            Wires.Add(Convert.ToInt32(child.controller.id.ToString()), wire);
                        }

            if (Blueprint.joints != null)
                foreach (dynamic joint in Blueprint.joints)
                    if (joint.controller != null && joint.controller.id != null)
                    {
                        dynamic correctedchild = getposandbounds(joint);
                        dynamic wire = new JObject();
                        wire.pos = new JObject()
                        {
                            ["x"] = Convert.ToInt32(correctedchild.pos.x.ToString()) + Convert.ToInt32(correctedchild.bounds.x.ToString()) / 2.0,
                            ["y"] = Convert.ToInt32(correctedchild.pos.y.ToString()) + Convert.ToInt32(correctedchild.bounds.y.ToString()) / 2.0,
                            ["z"] = Convert.ToInt32(correctedchild.pos.z.ToString()) + Convert.ToInt32(correctedchild.bounds.z.ToString()) / 2.0
                        };
                        string color = joint.color.ToString();
                        if (color.StartsWith("#"))
                            color = color.Substring(1);
                        wire.color = color;
                        wire.connections = joint.controller.controllers;
                        Wires.Add(Convert.ToInt32(joint.controller.id.ToString()), wire);
                    }

            return Wires;
        }

        public static dynamic GetBounds()
        {
            int minx = 10000, maxx = -10000, miny = 10000, maxy = -10000, minz = 10000, maxz = -10000;
            foreach(dynamic body in Blueprint.bodies)
                foreach(dynamic child in body.childs)
                {
                    dynamic correctedchild = getposandbounds(child);
                    if (correctedchild.pos.x < minx) minx = correctedchild.pos.x;
                    else
                    if (correctedchild.pos.x + correctedchild.bounds.x > maxx) maxx = correctedchild.pos.x + correctedchild.bounds.x;
                    if (correctedchild.pos.y < miny) miny = correctedchild.pos.y;
                    else
                    if (correctedchild.pos.y + correctedchild.bounds.y > maxy) maxy = correctedchild.pos.y + correctedchild.bounds.y;
                    if (correctedchild.pos.z < minz) minz = correctedchild.pos.z;
                    else
                    if (correctedchild.pos.z + correctedchild.bounds.z > maxz) maxz = correctedchild.pos.z + correctedchild.bounds.z;
                }
            if (Blueprint.joints != null)
                foreach (dynamic joint in Blueprint.joints)
                {
                    dynamic correctedchild = getposandbounds(joint);
                    if (correctedchild.pos.x < minx) minx = correctedchild.pos.x;
                    else
                    if (correctedchild.pos.x + correctedchild.bounds.x > maxx) maxx = correctedchild.pos.x + correctedchild.bounds.x;
                    if (correctedchild.pos.y < miny) miny = correctedchild.pos.y;
                    else
                    if (correctedchild.pos.y + correctedchild.bounds.y > maxy) maxy = correctedchild.pos.y + correctedchild.bounds.y;
                    if (correctedchild.pos.z < minz) minz = correctedchild.pos.z;
                    else
                    if (correctedchild.pos.z + correctedchild.bounds.z > maxz) maxz = correctedchild.pos.z + correctedchild.bounds.z;
                }
            return new JObject() { ["minx"] = minx, ["maxx"] = maxx, ["miny"] = miny, ["maxy"] = maxy, ["minz"] = minz, ["maxz"] = maxz };
        }

        public static Tuple<Point3D,int> GetCenterOffMass()
        {
            Point3D center = new Point3D(0, 0, 0);
            int totalweight = 0;
            
            foreach(dynamic body in Blueprint.bodies)
                foreach(dynamic child in body.childs)
                    if(Database.blocks.ContainsKey(child.shapeId.ToString()))
                    {
                        dynamic realpos = getposandbounds(child);
                        int weight = Database.blocks[child.shapeId.ToString()].GetWeight(realpos.bounds);

                        var point = new Point3D((int)realpos.pos.x + (int)realpos.bounds.x/ 2.0, (int)realpos.pos.y + (int)realpos.bounds.y / 2.0, (int)realpos.pos.z + (int)realpos.bounds.z / 2.0);

                        int tot = totalweight + weight;
                        center = new Point3D((center.X * totalweight + point.X * weight) / tot, (center.Y * totalweight + point.Y * weight) / tot, (center.Z * totalweight + point.Z * weight) / tot);
                        totalweight = tot;
                    }
            return new Tuple<Point3D, int>(center, totalweight);
        }

        public static Tuple<Model3DGroup, Model3DGroup> RenderBlocks()
        {
            Model3DGroup blocks = new Model3DGroup();
            Model3DGroup glass = new Model3DGroup();

            Block block = new Block(null, "unknown", "unknown", null, null);

            if (Blueprint.bodies != null)
                foreach (dynamic body in Blueprint.bodies)
                    foreach (dynamic child in body.childs)
                    {
                        dynamic realpos = getposandbounds(child);
                        int x = realpos.pos.x;
                        int y = realpos.pos.y;
                        int z = realpos.pos.z;

                        if (Database.blocks.ContainsKey(child.shapeId.ToString()))
                        {
                            Blockobject blockobject = Database.blocks[child.shapeId.ToString()];
                            if (blockobject is Block)
                            {
                                Model3D model = (blockobject as Block).Render(x, y, z, realpos.bounds, child.color.ToString(), Convert.ToInt32(realpos.xaxis), Convert.ToInt32(realpos.zaxis));
                                if (!blockobject.glass)
                                    blocks.Children.Add(model);
                                else
                                    glass.Children.Add(model);
                            }
                            else//part
                            {
                                Model3D model = (blockobject as Part).Render(x , y, z, child.color.ToString(), Convert.ToInt32(realpos.xaxis), Convert.ToInt32(realpos.zaxis));
                                if (!blockobject.glass)
                                    blocks.Children.Add(model);
                                else
                                    glass.Children.Add(model);
                                
                            }
                        }
                        else//not in database
                        {
                            dynamic bounds = new JObject() { ["x"] = 1, ["y"] = 1, ["z"] = 1 };
                            if (realpos.bounds != null)
                                bounds = realpos.bounds;

                            Model3D model = block.Render(x, y, z, bounds, child.color.ToString(), Convert.ToInt32(realpos.xaxis), Convert.ToInt32(realpos.zaxis));

                            blocks.Children.Add(model);

                        }
                    }
            if(Blueprint.joints != null)
                foreach(dynamic joint in Blueprint.joints)
                {
                    dynamic realpos = getposandbounds(joint);
                    int x = realpos.pos.x;
                    int y = realpos.pos.y;
                    int z = realpos.pos.z;

                    if (Database.blocks.ContainsKey(joint.shapeId.ToString()))
                    {
                        Blockobject blockobject = Database.blocks[joint.shapeId.ToString()];
                        if (blockobject is Block)
                        {
                            Model3D model = (blockobject as Block).Render(x, y, z, realpos.bounds, joint.color.ToString(), Convert.ToInt32(realpos.xaxis), Convert.ToInt32(realpos.zaxis));
                            if (!blockobject.glass)
                                blocks.Children.Add(model);
                            else
                                glass.Children.Add(model);
                        }
                        else//part
                        {
                            Model3D model = (blockobject as Part).Render(x, y, z, joint.color.ToString(), Convert.ToInt32(realpos.xaxis), Convert.ToInt32(realpos.zaxis));
                            if (!blockobject.glass)
                                blocks.Children.Add(model);
                            else
                                glass.Children.Add(model);
                        }
                    }
                    else//not in database
                    {
                        dynamic bounds = new JObject() { ["x"] = 1, ["y"] = 1, ["z"] = 1 };
                        if (realpos.bounds != null)
                            bounds = realpos.bounds;

                        Model3D model = block.Render(x, y, z, bounds, joint.color.ToString(), Convert.ToInt32(realpos.xaxis), Convert.ToInt32(realpos.zaxis));

                        blocks.Children.Add(model);

                    }

                }
            new Task(() =>
            {
                if (BP.missingmod) MessageBox.Show("Missing mod for this blueprint! \nPlease download the required mod!\n\nwill work for now tho wiring/moving blocks not recommended!");
            }).Start();
            new Task(() =>
            {
                if (BP.brokenrotation) MessageBox.Show("Broken Rotation in this blueprint!\n\nTools 'mirror tool', [] ,.. will BREAK your blueprint!");
            }).Start();
            return new Tuple<Model3DGroup, Model3DGroup>(blocks,glass);
        }

        

        public static void Save()
        {

            string blueprinttext = Convert.ToString(Blueprint);
            string descriptiontext = Newtonsoft.Json.JsonConvert.SerializeObject(Description);
            
            if(!Directory.Exists(Blueprintpath))
                System.IO.Directory.CreateDirectory(Blueprintpath);

            backup();
            System.IO.File.WriteAllText(Blueprintpath + "\\blueprint.json", blueprinttext); //save blueprint
            System.IO.File.WriteAllText(Blueprintpath + "\\description.json", descriptiontext); //save description

            using (Stream stm = File.Create(Blueprintpath + "\\icon.png"))
            {
                Icon.Save(stm);
            }
            Database.LoadBpsIn(Database.User_ + "\\blueprints");//refresh icon
            Database.bprefresh = true;
            new System.Threading.Thread(new System.Threading.ThreadStart(()=> { MessageBox.Show("Blueprint saved!"); })).Start();
            

        }
        public static void backup()
        {
            if(File.Exists(Blueprintpath + "\\blueprint.json"))
            {
                for(int i = 1; i<30; i++)
                {
                    if (!File.Exists(Blueprintpath + "\\blueprint_backup_" + i.ToString() + ".json"))
                    {
                        System.IO.File.Copy(Blueprintpath + "\\blueprint.json", Blueprintpath + "\\blueprint_backup_" + i.ToString() + ".json");
                        break;
                    }
                }
            }
        }
        public static void SaveAs(string name)//in scrapdata dir
        {
            string newbp = Database.User_ + @"\Blueprints\" + name;
            System.IO.Directory.CreateDirectory(newbp);

            string blueprinttext = Convert.ToString(Blueprint);
            string descriptiontext = Newtonsoft.Json.JsonConvert.SerializeObject(Description);

            System.IO.File.WriteAllText(newbp + "\\blueprint.json", blueprinttext); //save blueprint
            System.IO.File.WriteAllText(newbp + "\\description.json", descriptiontext); //save description
            using (Stream stm = File.Create(newbp+"\\icon.png"))
            {
                Icon.Save(stm);
            }
            Database.LoadBpsIn(Database.User_ + "\\blueprints");
            Database.bprefresh = true;
            
            
            new System.Threading.Thread(() => { MessageBox.Show("Saved as new blueprint!\nwill require game restart to be able to find it in-game!\n(blame axolot)"); }).Start();

        }



        //get bounds from 
        private static dynamic getbounds(dynamic part)
        {
            dynamic bounds = new Newtonsoft.Json.Linq.JObject();
            if (part.box != null)
            {
                return part.box;
            }
            if (part.hull != null)
            {
                bounds.x = part.hull.x;
                bounds.y = part.hull.y;
                bounds.z = part.hull.z;
                return bounds;
            }
            if (part.cylinder != null)
            {
                if (part.cylinder.axis.ToString().ToLower() == "x")
                {
                    bounds.x = part.cylinder.depth;
                    bounds.y = part.cylinder.diameter;
                    bounds.z = part.cylinder.diameter;
                    return bounds;

                }
                else
                if (part.cylinder.axis.ToString().ToLower() == "y")
                {
                    bounds.x = part.cylinder.diameter;
                    bounds.y = part.cylinder.depth;
                    bounds.z = part.cylinder.diameter;
                    return bounds;

                }
                else
                if (part.cylinder.axis.ToString().ToLower() == "z")
                {
                    bounds.x = part.cylinder.diameter;
                    bounds.y = part.cylinder.diameter;
                    bounds.z = part.cylinder.depth;
                    return bounds;

                }
            }
            bounds.X = 1;
            bounds.Y = 1;
            bounds.Z = 1;
            MessageBox.Show("no bounds\n\nImpossible senario. please report");
            return bounds;
        }


        //get new pos and correct ingame bounds for child
        public static dynamic getposandbounds(dynamic whatever)
        {

            dynamic child = whatever.DeepClone();
            string uuid = child.shapeId;

            //if (child.bounds == null) //add bounds to parts (blocks do not get affected)
            {
                if (Database.blocks.ContainsKey(uuid))
                {
                    Blockobject b = Database.blocks[uuid];
                    if(b is Part)
                    {
                        child.bounds = ((Part)b).GetBoundsDynamic();
                    }
                }
                else
                {
                    missingmod = true;
                }

                if (child.bounds == null)
                {
                    child.bounds = new JObject { ["x"] = 1, ["y"] = 1, ["z"] = 1 };
                }

            }
            bool IsJoint = false;
            if(child.posA != null)
            {
                IsJoint = true;
                child.pos = child.posA;
                child.xaxis = child.xaxisA;
                child.zaxis = child.zaxisA;
            }

            try
            {
                int xaxis = Convert.ToInt32(child.xaxis);
                int zaxis = Convert.ToInt32(child.zaxis);
                int xaxisabs = Math.Abs(xaxis);
                int zaxisabs = Math.Abs(zaxis);

                if (xaxisabs == 3)
                {
                    if (zaxisabs == 2)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.y, ["y"] = child.bounds.z, ["z"] = child.bounds.x };
                    }
                    else
                    if (zaxisabs == 1)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.z, ["y"] = child.bounds.y, ["z"] = child.bounds.x };
                    }
                    else
                        brokenrotation = true;
                }
                else if (xaxisabs == 2)
                {
                    if (zaxisabs == 3)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.y, ["y"] = child.bounds.x, ["z"] = child.bounds.z };
                    }
                    else
                    if (zaxisabs == 1)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.z, ["y"] = child.bounds.x, ["z"] = child.bounds.y };
                    }
                    else
                        brokenrotation = true;
                }
                else if (xaxisabs == 1)
                {
                    if (zaxisabs == 2)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.x, ["y"] = child.bounds.z, ["z"] = child.bounds.y };
                    }
                    else if(zaxisabs != 3)
                        brokenrotation = true;
                }
                else
                    brokenrotation = true;

                //this updating pos only applies to parts, blocks do not get affected as they always have xaxis 1 zaxis 3
                if (!IsJoint)
                {
                    if (xaxis == -1 | zaxis == -1 | (xaxis == 2 && zaxis == 3) | (xaxis == 3 && zaxis == -2) | (xaxis == -2 && zaxis == -3) | (xaxis == -3 && zaxis == 2))
                        child.pos.x -= child.bounds.x;
                    if (xaxis == -2 | zaxis == -2 | (xaxis == -1 && zaxis == 3) | (xaxis == -3 && zaxis == -1) | (xaxis == 1 && zaxis == -3) | (xaxis == 3 && zaxis == 1))
                        child.pos.y -= child.bounds.y;
                    if (xaxis == -3 | zaxis == -3 | (xaxis == -2 && zaxis == 1) | (xaxis == -1 && zaxis == -2) | (xaxis == 1 && zaxis == 2) | (xaxis == 2 && zaxis == -1))
                        child.pos.z -= child.bounds.z;
                }
                else
                {
                    if (!(zaxis > 0 || !(child.bounds.x != 1 || child.bounds.y != 1 || child.bounds.z != 1)))
                    {
                        //correctedchild.pos = Blueprint.joints[i].posB;
                        if (zaxis == -1)
                            child.pos.x -= child.bounds.x - 1;
                        if (zaxis == -2)
                            child.pos.y -= child.bounds.y - 1;
                        if (zaxis == -3)
                            child.pos.z -= child.bounds.z - 1;
                    }

                }
            }
            catch
            {

            }
            return child;
        }


        public static dynamic calcbppos(dynamic whatever)
        {
            dynamic child = Newtonsoft.Json.JsonConvert.DeserializeObject(Convert.ToString(whatever));

            //this updating pos only applies to parts, blocks do not get affected as they always have xaxis 1 zaxis 3
            if (child.xaxis == -1 | child.zaxis == -1 | (child.xaxis == 2 && child.zaxis == 3) | (child.xaxis == 3 && child.zaxis == -2) | (child.xaxis == -2 && child.zaxis == -3) | (child.xaxis == -3 && child.zaxis == 2))
                child.pos.x += child.bounds.x;
            if (child.xaxis == -2 | child.zaxis == -2 | (child.xaxis == -1 && child.zaxis == 3) | (child.xaxis == -3 && child.zaxis == -1) | (child.xaxis == 1 && child.zaxis == -3) | (child.xaxis == 3 && child.zaxis == 1))
                child.pos.y += child.bounds.y;
            if (child.xaxis == -3 | child.zaxis == -3 | (child.xaxis == -2 && child.zaxis == 1) | (child.xaxis == -1 && child.zaxis == -2) | (child.xaxis == 1 && child.zaxis == 2) | (child.xaxis == 2 && child.zaxis == -1))
                child.pos.z += child.bounds.z;
            return child;
        }
    }
}
