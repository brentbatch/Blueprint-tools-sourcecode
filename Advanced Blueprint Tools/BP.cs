﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

namespace Advanced_Blueprint_Tools
{
    public class BP
    {
        public static string Blueprintpath { get; private set; } 

        public static dynamic Blueprint { get; private set; }

        public static dynamic Description { get; private set; }

        public static PngBitmapEncoder Icon { get; set; }

        public static Dictionary<int, dynamic> Wires { get; set; }

        public static List<string> Useduuids { get; set; } = new List<string>();


        public static dynamic blocksxyz = new JObject();//for render, contains correct pos for blocks


        public static int minx = 10000,maxx = -10000, miny = 10000, maxy = -10000, minz = 10000, maxz = -10000;
        public static int centerx, centery, centerz;

        public static bool missingmod = false;

        public static void setblueprint(dynamic bp)
        {
            Blueprint = bp;
            calcrender();
        }

        public BP(string bppath, dynamic bp, dynamic desc)
        {
            Blueprintpath = bppath;
            Blueprint = bp;
            Description = desc;
            blocksxyz = new JObject();
            setblueprint(bp);
        }

        public BP(string bppath)
        {
            Blueprint = null;
            Description = null;

            Blueprintpath = bppath;
            blocksxyz = new JObject();
            //load directory of blueprints
            try
            {
                Blueprint = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(Blueprintpath + @"\blueprint.json"));
                Description = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(Blueprintpath + @"\description.json"));

                calcrender();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                
            }
        }

        private static void calcrender()
        {
            missingmod = false;
            //if(false)
            {
                Useduuids.Clear();
                blocksxyz = new JObject();
                Wires = new Dictionary<int, dynamic>();
                minx = 10000; maxx = -10000; miny = 10000; maxy = -10000; minz = 10000; maxz = -10000;
                if(Blueprint.bodies != null)
                foreach (dynamic body in Blueprint.bodies)
                    foreach (dynamic child in body.childs)
                    {

                        if (!Useduuids.Contains(child.shapeId.ToString()))
                            Useduuids.Add(child.shapeId.ToString());

                        dynamic correctedchild = getposandbounds(child);//outputs corrected child (default rotation, correct position)

                        string x = correctedchild.pos.x.ToString();
                        string y = correctedchild.pos.y.ToString();
                        string z = correctedchild.pos.z.ToString();
                        //if (child.bounds == null) child.bounds = correctedchild.bounds;
                        if (blocksxyz[x] == null) blocksxyz[x] = new JObject();
                        if (blocksxyz[x][y] == null) blocksxyz[x][y] = new JObject();
                        if (blocksxyz[x][y][z] == null) blocksxyz[x][y][z] = new JObject();
                        if (blocksxyz[x][y][z].blocks == null) blocksxyz[x][y][z].blocks = new JArray();
                        blocksxyz[x][y][z].blocks.Add(child);
                        if(child.controller != null && child.controller.id != null)
                        {
                            dynamic wire =  new JObject();
                            wire.pos = new JObject();
                            wire.pos.x =Convert.ToInt32(correctedchild.pos.x.ToString())+Convert.ToInt32(correctedchild.bounds.x.ToString())/2.0;
                            wire.pos.y =Convert.ToInt32(correctedchild.pos.y.ToString())+Convert.ToInt32(correctedchild.bounds.y.ToString())/2.0;
                            wire.pos.z =Convert.ToInt32(correctedchild.pos.z.ToString())+Convert.ToInt32(correctedchild.bounds.z.ToString())/2.0;
                            string color = child.color.ToString();
                            if (color.StartsWith("#"))
                                color = color.Substring(1, 6);
                            wire.color = color;
                            wire.connections = child.controller.controllers;
                            Wires.Add(Convert.ToInt32(child.controller.id.ToString()), wire);
                        }

                        //get whole creation bounds:
                        if (correctedchild.pos.x < minx) minx = correctedchild.pos.x;
                        if (correctedchild.pos.x + correctedchild.bounds.x > maxx) maxx = correctedchild.pos.x + correctedchild.bounds.x;
                        if (correctedchild.pos.y < miny) miny = correctedchild.pos.y;
                        if (correctedchild.pos.y + correctedchild.bounds.y > maxy) maxy = correctedchild.pos.y + correctedchild.bounds.y;
                        if (correctedchild.pos.z < minz) minz = correctedchild.pos.z;
                        if (correctedchild.pos.z + correctedchild.bounds.z > maxz) maxz = correctedchild.pos.z + correctedchild.bounds.z;
                    }
                if (Blueprint.joints != null)
                    for (int i = 0; i < Blueprint.joints.Count; i++)
                    {
                        if (!Useduuids.Contains(Blueprint.joints[i].shapeId.ToString()))
                            Useduuids.Add(Blueprint.joints[i].shapeId.ToString());
                        Blueprint.joints[i].xaxis = Blueprint.joints[i].xaxisA;
                        Blueprint.joints[i].zaxis = Blueprint.joints[i].zaxisA;
                        Blueprint.joints[i].pos = Blueprint.joints[i].posA;
                        dynamic correctedchild = getposandbounds(Blueprint.joints[i]);//outputs corrected child (default rotation, correct position)
                        correctedchild.pos = Blueprint.joints[i].posA;
                        if (!(Convert.ToInt32(Blueprint.joints[i].zaxis.ToString()) > 0 || !(correctedchild.bounds.x != 1 || correctedchild.bounds.y != 1 || correctedchild.bounds.z != 1)))
                        {
                            //correctedchild.pos = Blueprint.joints[i].posB;

                            int zaxis = Convert.ToInt32(Blueprint.joints[i].zaxis.ToString());
                            if (zaxis == -1)
                                correctedchild.pos.x -= correctedchild.bounds.x - 1;
                            if (zaxis == -2)
                                correctedchild.pos.y -= correctedchild.bounds.y - 1;
                            if (zaxis == -3)
                                correctedchild.pos.z -= correctedchild.bounds.z - 1;
                        }
                        string x = correctedchild.pos.x.ToString();
                        string y = correctedchild.pos.y.ToString();
                        string z = correctedchild.pos.z.ToString();
                        //if (Blueprint.joints[i].bounds == null) Blueprint.joints[i].bounds = correctedchild.bounds;
                        if (blocksxyz[x] == null) blocksxyz[x] = new JObject();
                        if (blocksxyz[x][y] == null) blocksxyz[x][y] = new JObject();
                        if (blocksxyz[x][y][z] == null) blocksxyz[x][y][z] = new JObject();
                        if (blocksxyz[x][y][z].blocks == null) blocksxyz[x][y][z].blocks = new JArray();

                        blocksxyz[x][y][z].blocks.Add(Blueprint.joints[i]);

                        if (Blueprint.joints[i].controller != null && Blueprint.joints[i].controller.id != null)
                        {
                            dynamic wire = new JObject();
                            wire.pos = new JObject();
                            wire.pos.x = Convert.ToInt32(correctedchild.pos.x.ToString()) + Convert.ToInt32(correctedchild.bounds.x.ToString())/2.0;
                            wire.pos.y = Convert.ToInt32(correctedchild.pos.y.ToString()) + Convert.ToInt32(correctedchild.bounds.y.ToString())/2.0;
                            wire.pos.z = Convert.ToInt32(correctedchild.pos.z.ToString()) + Convert.ToInt32(correctedchild.bounds.z.ToString())/2.0;
                            string color = Blueprint.joints[i].color.ToString();
                            if (color.StartsWith("#"))
                                color = color.Substring(1, 6);
                            wire.color = color;
                            wire.connections = Blueprint.joints[i].controller.controllers;
                            Wires.Add(Convert.ToInt32(Blueprint.joints[i].controller.id.ToString()), wire);
                        }

                        //get whole creation bounds:
                        if (correctedchild.pos.x < minx) minx = correctedchild.pos.x;
                        if (correctedchild.pos.x + correctedchild.bounds.x > maxx) maxx = correctedchild.pos.x + correctedchild.bounds.x;
                        if (correctedchild.pos.y < miny) miny = correctedchild.pos.y;
                        if (correctedchild.pos.y + correctedchild.bounds.y > maxy) maxy = correctedchild.pos.y + correctedchild.bounds.y;
                        if (correctedchild.pos.z < minz) minz = correctedchild.pos.z;
                        if (correctedchild.pos.z + correctedchild.bounds.z > maxz) maxz = correctedchild.pos.z + correctedchild.bounds.z;

                    }
                centerx = (maxx + minx) / 2;
                centery = (maxy + miny) / 2;
                centerz = (maxz + minz) / 2;
            }
            if (missingmod) MessageBox.Show("Missing mod for this blueprint! \nPlease download the required mod!\n\nwill work for now tho wiring/moving blocks not recommended!");

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
            dynamic child = Newtonsoft.Json.JsonConvert.DeserializeObject(Convert.ToString(whatever));
            string uuid = child.shapeId;

            //if (child.bounds == null) //add bounds to parts (blocks do not get affected)
            {
                if (Database.blocks.ContainsKey(child.shapeId.Value.ToLower()))
                {
                    Blockobject b = Database.blocks[child.shapeId.Value.ToLower()];
                    if(b is Part)
                    {
                        child.bounds = ((Part)b).GetBoundsDynamic();
                    }
                    else
                    {
                        child.bounds = new JObject { ["x"] = 1, ["y"] = 1, ["z"] = 1 };
                    }
                    //child.bounds = getbounds(gameblocks[child.shapeId.Value.ToLower()]);
                }
                else
                {
                    missingmod = true;
                    //child.bounds.x = 1;
                    //child.bounds.y = 1;
                    //child.bounds.z = 1;
                }

            }
            int xaxis = Math.Abs(Convert.ToInt32(child.xaxis));
            int zaxis = Math.Abs(Convert.ToInt32(child.zaxis));
            //if(xaxis != 1 && zaxis != 3)
            {

                if (xaxis == 3)
                {
                    if (zaxis == 2)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.y, ["y"] = child.bounds.z, ["z"] = child.bounds.x };
                    }
                    else
                    if (zaxis == 1)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.z, ["y"] = child.bounds.y, ["z"] = child.bounds.x };
                    }
                }
                else if (xaxis == 2)
                {
                    if (zaxis == 3)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.y, ["y"] = child.bounds.x, ["z"] = child.bounds.z };
                    }
                    else
                    if (zaxis == 1)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.z, ["y"] = child.bounds.x, ["z"] = child.bounds.y };
                    }
                }
                else if (xaxis == 1)
                {
                    if (zaxis == 2)
                    {
                        child.bounds = new JObject { ["x"] = child.bounds.x, ["y"] = child.bounds.z, ["z"] = child.bounds.y };
                    }
                }
            }
            //this updating pos only applies to parts, blocks do not get affected as they always have xaxis 1 zaxis 3
            try
            {
                if (child.xaxis == -1 | child.zaxis == -1 | (child.xaxis == 2 && child.zaxis == 3) | (child.xaxis == 3 && child.zaxis == -2) | (child.xaxis == -2 && child.zaxis == -3) | (child.xaxis == -3 && child.zaxis == 2))
                    child.pos.x -= child.bounds.x;
                if (child.xaxis == -2 | child.zaxis == -2 | (child.xaxis == -1 && child.zaxis == 3) | (child.xaxis == -3 && child.zaxis == -1) | (child.xaxis == 1 && child.zaxis == -3) | (child.xaxis == 3 && child.zaxis == 1))
                    child.pos.y -= child.bounds.y;
                if (child.xaxis == -3 | child.zaxis == -3 | (child.xaxis == -2 && child.zaxis == 1) | (child.xaxis == -1 && child.zaxis == -2) | (child.xaxis == 1 && child.zaxis == 2) | (child.xaxis == 2 && child.zaxis == -1))
                    child.pos.z -= child.bounds.z;
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
