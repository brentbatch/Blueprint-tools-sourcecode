using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text;
using Assimp;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Xml;

namespace Advanced_Blueprint_Tools
{
    public static class Database
    {
        public static string steamapps { get; private set; }
        public static string User_ { get; private set; } //path
        public static string ScrapData { get; private set; } = "";
        private static string ModDatabase = "";

        public static long UserID { get; private set; }

        public static Dictionary<string, Blockobject> blocks = new Dictionary<string, Blockobject>();//uuid, block
        public static Dictionary<string, string> usedmods = new Dictionary<string, string>();//id, name
        public static Dictionary<string, BitmapSource> blueprints = new Dictionary<string, BitmapSource>();//path, img
        public static List<Notification> Notifications = new List<Notification>();

        public static bool bprefresh;

        public static void LoadAllBlocks()
        {
            LoadVanillaObjects();
            LoadModObjects();
        }
        public static string findPaths()
        {
            string userdir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Axolot Games\\Scrap Mechanic\\User";

            DateTime lasthigh = new DateTime(1900, 1, 1);
            string dir = "";
            foreach (string subdir in Directory.GetDirectories(userdir)) //get user_numbers folder that is last used
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
            User_ = dir;
            

            steamapps = Properties.Settings.Default.steamapps;
            /*
            {//not yet
                if(Properties.Settings.Default.times == 3)
                    System.Diagnostics.Process.Start("http://www.youtube.com/c/brentbatch?sub_confirmation=1");
            }*/

            if (File.Exists("steamapps") && !File.Exists("config"))
                File.Copy("steamapps", "config");

            dynamic config = new JObject();
            if (File.Exists("config") && !System.IO.Directory.Exists(steamapps + @"\common\Scrap Mechanic\Data\Objects\Database\ShapeSets"))
            {
                try
                {
                    config = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("steamapps"));
                }
                catch { }
                if (config.steamapps != null)
                {
                    steamapps = config.steamapps.ToString();
                    Properties.Settings.Default.steamapps = config.steamapps.ToString();
                }
                if (config.times != null)
                    Properties.Settings.Default.times = Convert.ToInt32(config.times.ToString());
                if (config.safemode == true)
                    Properties.Settings.Default.safemode = config.safemode == true ? true : false;
                if (config.wires == true)
                    Properties.Settings.Default.wires = config.wires == true ? true : false;
                if (config.colorwires == true)
                    Properties.Settings.Default.colorwires = config.colorwires == true ? true : false;
                if (config.wirecolor != null)
                    Properties.Settings.Default.wirecolor = config.wirecolor;
                if (config.blobcolor != null)
                    Properties.Settings.Default.blobcolor = config.blobcolor;
                if (config.coloropacity != null)
                    Properties.Settings.Default.coloropacity = Convert.ToByte(config.coloropacity.ToString());

            }
            Properties.Settings.Default.times++;

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
                while (!System.IO.Directory.Exists(ScrapData + @"\Objects\Database\ShapeSets"))
                {
                    MessageBoxResult result = MessageBox.Show("could not find the gamefiles folder which is needed to get the block properties\nPlease select the Scrap Mechanic folder, It contains: Cache,Data,Logs,Release,...\n\nSteam Library > Right click scrap mechanic > properties > Local Files > browse local files > Copy path from the folder", "Unusual folder location!", MessageBoxButton.OKCancel);
                    if (result != MessageBoxResult.OK)
                    {
                        MessageBox.Show("continuing without loading resources.\nWorking processes: \n- Loading blueprints -although a bit broken\n- sphere generator\n- mirror mode");
                    }

                    System.Windows.Forms.FolderBrowserDialog fbD = new System.Windows.Forms.FolderBrowserDialog();
                    fbD.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                    fbD.ShowDialog();
                    string scrap = fbD.SelectedPath;
                    try
                    {
                        steamapps = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(scrap));
                    }
                    catch { steamapps = ""; }
                    ScrapData = scrap + "\\Data";
                }
            }

            if (ScrapData == "")
                ScrapData = steamapps + @"\common\Scrap Mechanic\Data";

            if (steamapps != "")
                ModDatabase = steamapps + @"\workshop\content\387990";

            Properties.Settings.Default.steamapps = steamapps;
            Properties.Settings.Default.Save();
            return steamapps;
        }

        public static void LoadAllBlueprints()
        {
            string blueprintdir = User_ + "\\blueprints";
            UserID = Convert.ToInt64(Path.GetFileName(User_).ToString().Substring(5));
            LoadBpsIn(blueprintdir);
            if (Directory.Exists(steamapps + @"\workshop\content\387990"))
                LoadBpsIn(steamapps + @"\workshop\content\387990");

            Thread updatebps = new Thread(new ThreadStart(() =>
            {
                while(true)
                {
                    if (Directory.GetLastWriteTime(blueprintdir) > DateTime.Now.AddSeconds(-2))
                        LoadBpsIn(blueprintdir);
                    if (Directory.Exists(steamapps + @"\workshop\content\387990") &&
                    Directory.GetLastWriteTime(steamapps + @"\workshop\content\387990") > DateTime.Now.AddSeconds(-2))
                        LoadBpsIn(Database.steamapps + @"\workshop\content\387990");

                    Thread.Sleep(1500);
                }
            }));
            updatebps.IsBackground = true;
            updatebps.Start();

        }

        public static void LoadBpsIn(string directory)
        {
            ImageToBitmapSourceConverter converter = new ImageToBitmapSourceConverter();
            foreach (string blueprint in Directory.GetDirectories(directory))
            {
                if (File.Exists(blueprint + @"\blueprint.json") /*&& File.Exists(blueprint + @"\icon.png") && File.Exists(blueprint + @"\description.json")*/)
                {
                    if (!File.Exists(blueprint + @"\description.json"))
                    {
                        dynamic desc = new JObject();
                        desc.name = "unnamed blueprint";
                        desc.description = "has no name";
                        //create new description
                        File.WriteAllText(blueprint + @"\description.json", desc.ToString());
                    }

                    if (File.Exists(blueprint + @"\icon.png"))
                    {
                        Image image = Image.FromFile(blueprint + @"\icon.png");
                        if (!blueprints.ContainsKey(blueprint))
                            blueprints.Add(blueprint, (BitmapSource)converter.Convert((Image)image.Clone(),null,null,null));
                        else
                        {
                            if (!blueprints[blueprint].Equals((Image)image.Clone()))
                            {
                                //blueprints[blueprint].;
                                blueprints[blueprint] = (BitmapSource)converter.Convert((Image)image.Clone(), null, null, null);
                            }
                        }
                        image.Dispose();
                        
                    }
                    else
                    {
                        //load with 'missing icon' or sth
                        Image image = Image.FromFile("missingicon.bmp");
                        if (!blueprints.ContainsKey(blueprint))
                            blueprints.Add(blueprint, (BitmapSource)converter.Convert((Image)image.Clone(), null, null, null));
                        else
                        {
                            if (!blueprints[blueprint].Equals((Image)image.Clone()))
                            {
                                //blueprints[blueprint].;
                                blueprints[blueprint] = (BitmapSource)converter.Convert((Image)image.Clone(), null, null, null);
                            }
                        }
                        image.Dispose();
                    }
                }
                else if(File.Exists(blueprint + @"\icon.png"))
                {
                    //go over all files, if no .txt files or directories, remove folder

                    bool dirs = false;
                    foreach (string subdir in Directory.GetDirectories(blueprint))
                    {
                        if (Directory.Exists(subdir)) dirs = true;
                    }
                    if(!dirs)
                    {
                        bool hasTxtFile = false;
                        foreach (string subfile in Directory.GetFiles(blueprint))
                        {
                            string ext = Path.GetExtension(subfile);
                            if (ext == ".txt")
                                hasTxtFile = true;
                        }
                        if (!hasTxtFile)
                            Directory.Delete(blueprint, true);
                    }
                }
            }
        }
        

        private static void LoadVanillaObjects()
        {
            string inventorydecpath = ScrapData + @"\Gui\Language\English\InventoryItemDescriptions.json";
            JObject inventoryitemdesc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(inventorydecpath)) as dynamic;
            
            //VANILLA BLOCKS: 
            dynamic blockz = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(
                System.IO.File.ReadAllText(ScrapData + @"\Objects\Database\basicmaterials.json"));

            foreach (dynamic prop in blockz)
            {
                foreach (dynamic part in blockz[prop.Name])
                {
                    string name = "unnamed shape " + part.uuid.ToString();
                    if (inventoryitemdesc[part.uuid.ToString()] != null)
                        name = inventoryitemdesc[part.uuid.ToString()].title.ToString();
                    
                    blocks.Add(
                        part.uuid.ToString(),
                        new Block(ScrapData, name, "vanilla", part,null)
                        );

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
                            string name = "unnamed shape " + part.uuid.ToString();
                            if (inventoryitemdesc[part.uuid.ToString()] != null)
                                name = inventoryitemdesc[part.uuid.ToString()].title.ToString();

                            if (part.renderable != null)//only adding parts here, modded shit not allowed
                                blocks.Add(
                                    part.uuid.ToString(),
                                    new Part(ScrapData, name, "vanilla", part, null, true)
                                    );
                        }
                        catch (Exception e)
                        {
                            Notifications.Add(new Notification(
                                "unloadable file",
                                new Task(() =>
                                {
                                    System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(e.Message+"\n\nopen file?", "error in vanilla files!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                    if (messageBoxResult == MessageBoxResult.Yes)
                                    {
                                        System.Diagnostics.Process.Start(file);
                                    }
                                })
                            ));
                        }
                    }
                }
            }
        }

        private static void LoadModObjects()
        {
            List<String> workshoppages = new List<string>();

            if (ModDatabase != "")
                foreach (string folder in System.IO.Directory.GetDirectories(ModDatabase))
                {
                    //modded blocks:
                    if (System.IO.Directory.Exists(folder + @"\Objects\Database\ShapeSets"))//its a mod, not a blueprint (workshop dir contains blueprints too)
                    {
                        dynamic inventoryitemdesc = new JObject();
                        try
                        {
                            inventoryitemdesc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(folder + @"\Gui\Language\English\inventoryDescriptions.json"));
                            if (inventoryitemdesc == null)
                                inventoryitemdesc = new JObject();
                        }
                        catch
                        {//not parseable
                            if (!workshoppages.Contains("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder)))
                                workshoppages.Add("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder));
                            Notifications.Add(new Notification(
                                "unloadable file",
                                new Task(() =>
                                {
                                    System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("item desc not parseable, open location?", "unparseable file!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                    if (messageBoxResult == MessageBoxResult.Yes)
                                    {
                                            System.Diagnostics.Process.Start(folder +@"\Gui\Language\");
                                    }
                                })
                            ));
                        }

                        int conflictusemod = 0; //(1 use old, 2 use new

                        foreach (string file in System.IO.Directory.GetFiles(folder + @"\Objects\Database\ShapeSets"))
                            if (System.IO.Path.GetExtension(file).ToLower() == ".json")
                            {
                                dynamic parts = null;
                                try
                                {
                                    parts = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(file));
                                }
                                catch
                                {//not parseable, add to naughty list
                                    if (!workshoppages.Contains("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder)))
                                       workshoppages.Add("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder));

                                    Notifications.Add(new Notification(
                                        "unparseable file",
                                        new Task(() =>
                                        {
                                            System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("unparseable file found:\n"+file, "unparseable file!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                            if (messageBoxResult == MessageBoxResult.Yes)
                                            {
                                                System.Diagnostics.Process.Start(file);
                                            }
                                        })
                                    ));
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
                                                    if (conflictusemod == 0 && blocks.ContainsKey(part.uuid.ToString()) && blocks[part.uuid.ToString()].Path != folder)
                                                    {
                                                        conflictusemod = 1;//keep mod1

                                                        string uuid = part.uuid.ToString();
                                                        string mod1 = blocks[part.uuid.ToString()].Mod;
                                                        string mod2 = moddesc.name;

                                                        if (mod1.ToString() == "vanilla")
                                                        {
                                                            conflictusemod = 2;//overwrite, people have old installed mods in vanilla files!
                                                        }
                                                        else
                                                        {
                                                            MessageBoxResult messageBoxResult = MessageBox.Show
                                                                ("\"" + mod1 + "\" is using some(or all) uuid's of mod \"" + mod2 + "\"\n\nOverwrite blocks from \"" + mod1 + "\" with the blocks from \"" + mod2 + "\" ?\n\n" + file, "Conflicting mods!!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
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
                                                        usedmods[moddesc.fileId.ToString()] = moddesc.name.ToString();

                                                        string name = "unnamed shape " + part.uuid.ToString();
                                                        if (inventoryitemdesc[part.uuid.ToString()] != null)
                                                            name = inventoryitemdesc[part.uuid.ToString()].title.ToString();

                                                        if (part.renderable != null)//part
                                                        {
                                                            if(blocks.ContainsKey(part.uuid.ToString()))
                                                            { //OVERWRITE
                                                                blocks[part.uuid.ToString()] =
                                                                    new Part(folder, name, moddesc.name.ToString(), part, moddesc);
                                                            }
                                                            else
                                                            blocks.Add(
                                                                part.uuid.ToString(),
                                                                new Part(folder, name, moddesc.name.ToString(), part, moddesc)
                                                                );
                                                        }
                                                        else //block!
                                                        {
                                                            if (blocks.ContainsKey(part.uuid.ToString()))
                                                            { //OVERWRITE
                                                                blocks[part.uuid.ToString()] =
                                                                    new Block(folder, name, moddesc.name.ToString(), part, moddesc);
                                                            }
                                                            else
                                                                blocks.Add(
                                                                    part.uuid.ToString(),
                                                                    new Block(folder, name, moddesc.name.ToString(), part, moddesc)
                                                                );
                                                            //part.dif asg nor
                                                        }
                                                    }
                                                }
                                                else
                                                {

                                                    Notifications.Add(new Notification(
                                                        "broken appdata mod",
                                                        new Task(() =>
                                                        {
                                                            MessageBox.Show("broken mod description!\nfolder: " + folder);

                                                        })
                                                    ));
                                                    }

                                            }
                                            catch
                                            {
                                                if(!workshoppages.Contains("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder)))
                                                    workshoppages.Add("http://steamcommunity.com/sharedfiles/filedetails/?id=" + System.IO.Path.GetFileName(folder));
                                            }
                                        }
                                    }
                            }
                    }
                    
                }


            try //appdata mods
            {
                string appdatamods = User_ + "\\Mods";
                
                foreach (string folder in System.IO.Directory.GetDirectories(appdatamods))
                {
                    if (System.IO.Directory.Exists(folder + @"\Objects\Database\ShapeSets"))
                    {
                        dynamic inventoryitemdesc = new JObject();
                        try
                        {
                            inventoryitemdesc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(folder + @"\Gui\Language\English\inventoryDescriptions.json"));
                            if (inventoryitemdesc == null)
                                inventoryitemdesc = new JObject();
                        }
                        catch (Exception e)
                        {

                            Notifications.Add(new Notification(
                                "broken appdata mod",
                                new Task(() =>
                                {
                                    MessageBox.Show(e.Message + "\n\n" + folder+ @"\Gui\Language\English\inventoryDescriptions.json", "broken appdata mod!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);

                                })
                            ));
                        }

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

                                    Notifications.Add(new Notification(
                                        "broken appdata mod",
                                        new Task(() =>
                                        {
                                            MessageBox.Show(e.Message + "\n\n" + file, "broken appdata mod!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);

                                        })
                                    ));
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
                                                    usedmods[moddesc.fileId.ToString()] = moddesc.name.ToString();

                                                    string name = "unnamed shape " + part.uuid.ToString();
                                                    if (inventoryitemdesc[part.uuid.ToString()] != null)
                                                        name = inventoryitemdesc[part.uuid.ToString()].title.ToString();


                                                    if (part.renderable != null)//part
                                                    {

                                                        if (blocks.ContainsKey(part.uuid.ToString()))
                                                        {
                                                            blocks[part.uuid.ToString()] =
                                                                new Part(folder,name, moddesc.name.ToString(),part, moddesc);
                                                        }
                                                        else
                                                            blocks.Add(
                                                                part.uuid.ToString(),
                                                                new Part(folder, name, moddesc.name.ToString(), part, moddesc)
                                                            );
                                                    }
                                                    else //block!
                                                    {
                                                        if (blocks.ContainsKey(part.uuid.ToString()))
                                                        {
                                                            blocks[part.uuid.ToString()] =
                                                                new Block(folder, name, moddesc.name.ToString(), part, moddesc);
                                                        }
                                                        else
                                                            blocks.Add(
                                                                part.uuid.ToString(),
                                                                new Block(folder, name, moddesc.name.ToString(), part, moddesc)
                                                            );
                                                    }
                                                }
                                                else
                                                {

                                                    MessageBox.Show("broken mod description!\nFile: " + folder);
                                                    //MessageBox.Show("you have a broken mod!\nFile: " + file,"", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                                }

                                            }
                                            catch (Exception e)
                                            {
                                                MessageBox.Show(e.Message + "\n" + file, "\nSomething wrong with a self-made mod", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                                            }
                                        }
                                    }
                            }
                    }
                    
                }


            }
            catch (Exception e)
            { MessageBox.Show(e.Message, "Error while loading appdata mods", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly); }


            if (workshoppages.Count != 0)
            {
                Notifications.Add(new Notification(
                    "unloadable mods --workshop pages",
                    new Task(() =>
                    {
                        System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Mods: " + usedmods.Count() + "\nBlocks loaded: " + blocks.Count() + "\n\nFound mods that weren't able to be loaded\nEither broken or made by a lazy modder\n\npls Remove or Unsub\n\nOpen Workshop pages?", "Found unloadable mods!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            foreach (string page in workshoppages)
                            {
                                System.Diagnostics.Process.Start(page);
                            }
                        }
                    })
                ));
            }
        }
        
    }

    public abstract class Blockobject
    {
        public string Path { get; private set; }//path to mod
        public string Mod { get; private set; } = "";//modname
        public string Name { get; private set; } = "unnamed shape";//block name
        public int id { get; private set; }//mod id
        private Bitmap icon;
        

        public bool glass { get; protected set; } = false;

        public Blockobject(string path, string Name, string Modname, dynamic desc)
        {
            this.Path = path;
            this.Name = Name;
            this.Mod = Modname;
            if(desc != null)
            {
                this.id = desc.fileId;
            }
        }

        public Bitmap GetIcon(string uuid)
        {
            if(File.Exists(this.Path + @"\Gui\IconMap.png") && File.Exists(this.Path + @"\Gui\IconMap.xml"))
            {
                Bitmap IconMap = new Bitmap(this.Path + @"\Gui\IconMap.png");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(this.Path + @"\Gui\IconMap.xml"));
                string iconstring = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc).Replace("@", "");
                dynamic icons = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(iconstring);

                string[] size = icons.MyGUI.Resource.Group.size.ToString().Split(' ');

                foreach (dynamic icon in icons.MyGUI.Resource.Group.Index)
                {
                    if(icon.@name.ToString() == uuid)
                    {
                        string[] point = icon.Frame.point.ToString().Split(' ');
                        Bitmap bmp = IconMap.Clone(new System.Drawing.Rectangle(Convert.ToInt32(point[0]), Convert.ToInt32(point[1]), Convert.ToInt32(size[0]), Convert.ToInt32(size[1])), IconMap.PixelFormat);
                        IconMap.Dispose();
                        return bmp;
                    }
                }
            }
            return null;
        }

        public abstract int GetWeight(dynamic bounds = null);

        public xyzpair BoundsByRotation(xyzpair bounds, int xaxis, int zaxis)
        {
            if (Math.Abs(xaxis) == 3)
            {
                if (Math.Abs(zaxis) == 2)
                {
                    return new xyzpair(bounds.y, bounds.z, bounds.x);
                }
                else
                if (Math.Abs(zaxis) == 1)
                {
                    return new xyzpair(bounds.z, bounds.y, bounds.x);
                }
            }
            if (Math.Abs(xaxis) == 2)
            {
                if (Math.Abs(zaxis) == 3)
                {
                    return new xyzpair(bounds.y, bounds.x, bounds.z);
                }
                else
                if (Math.Abs(zaxis) == 1)
                {
                    return new xyzpair(bounds.z, bounds.x, bounds.y);
                }
            }
            if (Math.Abs(xaxis) == 1)
            {
                if (Math.Abs(zaxis) == 2)
                {
                    return new xyzpair(bounds.x, bounds.z, bounds.y);
                }
            }
            return bounds;
        }
        
        protected Model3D Render(double x, double y, double z, Geometry3D geometry3D, xyzpair bounds, string color, int xaxis, int zaxis)
        {
            //x => x - centerx;
            if (color.StartsWith("#"))
                color = color.Substring(1);
            System.Windows.Media.Color c = System.Windows.Media.Color.FromRgb(Convert.ToByte(color.Substring(0, 2), 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16));
            if (glass == true)
                c.A = 120;
            System.Windows.Media.Media3D.Material material = new DiffuseMaterial(new SolidColorBrush(c));

            //for textures: prob need to switch to: https://www.nuget.org/packages/HelixToolkit.Wpf.SharpDX/
            //https://github.com/helix-toolkit/helix-toolkit/issues/471
            

            Model3D renderedblock = new GeometryModel3D { Geometry = geometry3D, Material = material };

            #region Rotation and Translation

            renderedblock.Transform = new MatrixTransform3D(new Matrix3D());
            Transform3DGroup baseLinkMove = new Transform3DGroup();
            TranslateTransform3D baseLinkTranslate = new TranslateTransform3D();
            RotateTransform3D baseLinkRotatex = new RotateTransform3D();
            RotateTransform3D baseLinkRotatez = new RotateTransform3D();
            bool xpos = xaxis > 0;
            bool zpos = zaxis > 0;
            switch (Math.Abs(xaxis))
            {
                case 1:
                    baseLinkRotatex.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, xpos ? 0 : 1), 180);
                    baseLinkMove.Children.Add(baseLinkRotatex);
                    switch (Math.Abs(zaxis))
                    {
                        case 1:
                            MessageBox.Show("Incorrect rotationset found !");
                            break;
                        case 2:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(zpos ? -1 : 1, 0, 0), 90);
                            break;
                        case 3:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(zpos ? 0 : 1, 0, 0), 180);
                            break;
                    }
                    baseLinkMove.Children.Add(baseLinkRotatez);
                    break;
                case 2:
                    baseLinkRotatex.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, xpos ? 1 : -1), 90);
                    baseLinkMove.Children.Add(baseLinkRotatex);
                    switch (Math.Abs(zaxis))
                    {
                        case 1:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, zpos ? 1 : -1, 0), 90);
                            break;
                        case 2:
                            MessageBox.Show("Incorrect rotationset found !");
                            break;
                        case 3:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, zpos ? 0 : 1, 0), 180);
                            break;
                    }
                    baseLinkMove.Children.Add(baseLinkRotatez);
                    break;
                case 3:
                    baseLinkRotatex.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, xpos ? -1 : 1, 0), 90);
                    baseLinkMove.Children.Add(baseLinkRotatex);
                    switch (Math.Abs(zaxis))
                    {
                        case 1:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, (zpos == xpos) ? 1 : 0), 180);
                            break;
                        case 2:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, (zpos == xpos) ? -1 : 1), 90);
                            break;
                        case 3:
                            MessageBox.Show("Incorrect rotationset found !");
                            break;
                    }
                    baseLinkMove.Children.Add(baseLinkRotatez);
                    break;
            } //rotations translate!



            baseLinkTranslate.OffsetX = x + (float)(bounds.x) / 2;
            baseLinkTranslate.OffsetY = y + (float)(bounds.y) / 2;
            baseLinkTranslate.OffsetZ = z + (float)(bounds.z) / 2;

            baseLinkMove.Children.Add(baseLinkTranslate);
            renderedblock.Transform = baseLinkMove;
            #endregion

            return renderedblock;
        }

    }

    public class Block : Blockobject
    {
        private dynamic block;
        //private static Dictionary<dynamic,Geometry3D> previousGeometries;
        private int weight = -1;


        public Block(string path, string Name, string Modname, JObject block, JObject desc) :base( path,  Name,  Modname, desc)
        {
            this.block = block;
            if (this.block != null && this.block.glass != null && this.block.glass == true)
                base.glass = true;
        }

        public override int GetWeight(dynamic bounds)
        {
            if (this.weight != -1) return this.weight;
            double density = 500;
            if (this.block.density != null) density = this.block.density;
            int weight = (int)(density * (int)bounds.x + density * (int)bounds.y + density * (int)bounds.z);
            return weight;
        }

        public Model3D Render(double x, double y, double z, dynamic bounds, string color, int xaxis, int zaxis)
        {
            var meshBuilder = new MeshBuilder(false, false);
            int bx = Convert.ToInt32(bounds.x);
            int by = Convert.ToInt32(bounds.y);
            int bz = Convert.ToInt32(bounds.z);
            meshBuilder.AddBox(new Point3D(0, 0, 0), bx, by, bz);

            Geometry3D geometry3D = meshBuilder.ToMesh(true);

            return base.Render(x, y, z, geometry3D, BoundsByRotation(new xyzpair(bx, by, bz), xaxis, zaxis), color, xaxis, zaxis);
        }
    }

    public class Part : Blockobject
    {
        public string uuid { get; private set; }
        private dynamic part;
        private Geometry3D geometry3D;
        private xyzpair bounds;
        private int weight = -1;
        public enum properties
        {
            unknown,
            noninteractive,
            spotlight,
            engine,
            thruster,
            steering,
            seat,
            timedjoint,
            lever,
            button,
            sensor,
            logic,
            timer,
            piston,
            simpleInteractive,
            bearing,
            spring,
            radio,
            horn,
            tone
        }
        private properties property = properties.unknown;

        public bool IsConnectable { get { return (GetProperty() != properties.noninteractive) && (this.property != properties.bearing) && (this.property != properties.spring); } private set { } }

        public properties GetProperty()
        {
            if (property == properties.unknown)
            {
                if (part.spotlight != null)
                    this.property = properties.spotlight;
                else
                if (part.engine != null)
                    this.property = properties.engine;
                else
                if (part.thruster != null)
                    this.property = properties.thruster;
                else
                if (part.steering != null)
                    this.property = properties.steering;
                else
                if (part.seat != null)
                    this.property = properties.seat;
                else
                if (part.timedjoint != null)
                    this.property = properties.timedjoint;
                else
                if (part.lever != null)
                    this.property = properties.lever;
                else
                if (part.button != null)
                    this.property = properties.button;
                else
                if (part.sensor != null)
                    this.property = properties.sensor;
                else
                if (part.logic != null)
                    this.property = properties.logic;
                else
                if (part.timer != null)
                    this.property = properties.timer;
                else
                if (part.piston != null)
                    this.property = properties.piston;
                else
                if (part.simpleInteractive != null)
                    this.property = properties.simpleInteractive;
                else
                if (part.bearing != null)
                    this.property = properties.bearing;
                else
                if (part.spring != null)
                    this.property = properties.spring;
                else
                if (part.radio != null)
                    this.property = properties.radio;
                else
                if (part.horn != null)
                    this.property = properties.horn;
                else
                if (part.tone != null)
                    this.property = properties.tone;
                else
                    this.property = properties.noninteractive;
            }
            return property;
        }
        
        public Part(string path, string Name, string Modname, JObject part, JObject desc, bool prerender = false ) : base(path, Name, Modname, desc)
        {
            this.part = part;
            this.uuid = this.part.uuid;
            if (prerender == true)
            {
                new Thread(new ThreadStart(() => { this.GetGeometry(); })).Start();
                this.GetProperty(); 
                this.GetBounds();
            }
        }

        public xyzpair GetBounds()
        {
            if(this.bounds == null)
                this.bounds = xyzpair.getPartBounds(part);
            return this.bounds;
        }

        public override int GetWeight(dynamic bounds)
        {
            if (this.weight != -1) return this.weight;
            double density = 500;
            if (this.part.density != null) density = this.part.density;
            int weight = (int)(density * (int)bounds.x + density* (int)bounds.y + density * (int)bounds.z);
            return weight;
        }

        public dynamic GetBoundsDynamic()
        {
            this.GetBounds();
            dynamic bounds = new JObject();
            bounds.x = this.bounds.x;
            bounds.y = this.bounds.y;
            bounds.z = this.bounds.z;
            return bounds;
        }


        public Model3D Render(double x, double y, double z, string color, int xaxis,int zaxis)
        {
            return base.Render(x, y, z, GetGeometry(), BoundsByRotation(GetBounds(), xaxis,zaxis), color, xaxis, zaxis);
        }

        public Geometry3D GetGeometry()
        {
            if(geometry3D != null)
                return geometry3D;

            if (part.renderable.lodList[0].subMeshList != null)
                foreach (dynamic submesh in part.renderable.lodList[0].subMeshList)
                    if (submesh.material.ToString() == "Glass")
                        base.glass = true;

            //part.renderable.lodList[].subMeshList[].textureList[]
            //part.renderable.lodList[].subMeshMap.Shark.textureList[]

            string meshlocation = part.renderable.lodList[0].mesh.ToString();


            this.geometry3D = LoadMesh(meshlocation);

            return geometry3D;

        }


        private Geometry3D LoadMesh(string meshlocation)
        {
            Geometry3D geometry = null;
            try
            {
                bool game = false;
                if (meshlocation.ToLower().StartsWith("$game_data"))
                {
                    meshlocation = meshlocation.Substring(10);
                    meshlocation = Database.ScrapData + meshlocation;
                    game = true;
                }
                if (meshlocation.ToLower().StartsWith("$mod_data"))
                {
                    meshlocation = meshlocation.Substring(9);
                    meshlocation = base.Path + meshlocation;
                }
                if (meshlocation.ToLower().StartsWith("../data"))
                {
                    meshlocation = meshlocation.Substring(7);
                    meshlocation = base.Path + meshlocation;
                    game = true;
                }
                string location = meshlocation;

                if (System.IO.Path.GetExtension(meshlocation) != ".obj")
                {
                    string obj = System.IO.Path.GetDirectoryName(meshlocation) + "\\" + System.IO.Path.GetFileNameWithoutExtension(meshlocation) + ".obj";
                    if (File.Exists(obj))
                        location = obj;//loading obj is prefered

                    string meshes = obj.Replace(System.IO.Path.GetDirectoryName(Path), "Meshes");
                    if (game == true)
                        meshes = obj.Replace(System.IO.Path.GetDirectoryName(Database.ScrapData), "Meshes");
                    if (File.Exists(meshes))
                    {
                        location = meshes;//if converted obj in install files exists, use it
                    }
                }


                Scene a = new AssimpImporter().ImportFile(location);


                //Mesh m = a.Meshes[0];
                //a.Materials[0]

                var meshBuilder = new MeshBuilder(false, false);
                foreach(Mesh m in a.Meshes)
                {

                    foreach (Face face in m.Faces)
                    {

                        IList<Point3D> vertices = new List<Point3D>();
                        foreach (uint i in face.Indices)
                        {
                            vertices.Add(new Point3D(m.Vertices[i].X, m.Vertices[i].Y, m.Vertices[i].Z));
                        }
                        meshBuilder.AddPolygon(vertices);
                        vertices = new List<Point3D>();
                    }
                    if (false)
                    {
                        var texturecoords = m.GetTextureCoords(0);
                        meshBuilder.TextureCoordinates = new PointCollection();
                        foreach (Assimp.Vector3D vec in m.GetTextureCoords(0))
                        {
                            meshBuilder.TextureCoordinates.Add(new System.Windows.Point(Convert.ToDouble(vec.X), Convert.ToDouble(vec.Y)));
                        }
                    }
                }

                //meshBuilder.Normals = m.Normals;
                //meshBuilder.Tangents = m.Tangents;
                //meshBuilder.BiTangents = m.BiTangents;
                geometry = meshBuilder.ToMesh(true);

            }
            catch
            {
                bool devmode = false;

                if(devmode)//create meshes structure inside tool folder
                {
                    string toolmesh = meshlocation.Replace(System.IO.Path.GetDirectoryName(Path), "Meshes");
                    System.IO.Directory.CreateDirectory(System.IO.Directory.GetParent(toolmesh).ToString());
                    if(!File.Exists(toolmesh))
                        File.Copy(meshlocation, toolmesh);
                }


                //failed to load .fbx/mesh file
                //construct geometry out of pointlist

                var meshBuilder = new MeshBuilder(false, false);
                if (part.box != null)
                {
                    meshBuilder.AddBox(new Point3D(0, 0, 0),Convert.ToDouble(part.box.x), Convert.ToDouble(part.box.y), Convert.ToDouble(part.box.z));
                    return meshBuilder.ToMesh(true);
                }
                if (part.cylinder != null)
                {
                    Point3D p1 = new Point3D(0, 0, 0);
                    Point3D p2 = new Point3D(0, 0, 0);
                    double margin = Convert.ToDouble(part.cylinder.margin);
                    double depth = Convert.ToDouble(part.cylinder.depth);
                    if (part.cylinder.axis.ToString().ToLower() == "x")
                    {
                        p1.X -= depth / 2;
                        p2.X += depth / 2;
                    }
                    if (part.cylinder.axis.ToString().ToLower() == "y")
                    {
                        p1.Y -= depth / 2;
                        p2.Y += depth / 2;
                    }
                    if (part.cylinder.axis.ToString().ToLower() == "z")
                    {
                        p1.Z -= depth / 2;
                        p2.Z += depth / 2;
                    }
                    meshBuilder.AddCylinder(p1, p2, Convert.ToDouble(part.cylinder.diameter) * margin, (Convert.ToInt32(part.cylinder.diameter) + 5 ) * 2);
                    return meshBuilder.ToMesh(true);
                }
                if(part.hull != null)
                {
                    if(part.hull.pointList != null && part.hull.pointList.Count > 5)
                    {
                        List<Point3D> points = new List<Point3D>();
                        foreach(dynamic point in part.hull.pointList)
                        {
                            points.Add(new Point3D(
                                Convert.ToDouble(point.x) * Convert.ToInt32(part.hull.x),
                                Convert.ToDouble(point.y) * Convert.ToInt32(part.hull.y),
                                Convert.ToDouble(point.z) * Convert.ToInt32(part.hull.z)));
                        }
                        meshBuilder.AddPolygon(points);
                        return meshBuilder.ToMesh(true);
                    }
                    else
                    {
                        meshBuilder.AddBox(new Point3D(0, 0, 0), Convert.ToInt32(part.hull.x), Convert.ToInt32(part.hull.y), Convert.ToInt32(part.hull.z));
                        return meshBuilder.ToMesh(true);
                    }
                }

                MessageBox.Show("error building mesh", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                //shouldn't be able to get to this line.
            }
            return geometry;
        }
    }
    
    public class xyzpair
    {
        public int x;
        public int y;
        public int z;

        public xyzpair(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static xyzpair getPartBounds(dynamic part)
        {
            dynamic bounds = getDynamicPartBounds(part);
            xyzpair b = new xyzpair(1,1,1);
            b.x = bounds.x;
            b.y = bounds.y;
            b.z = bounds.z;
            return b;
        }

        private static dynamic getDynamicPartBounds(dynamic part)
        {
            dynamic bounds = new JObject();
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
    }

    [ValueConversion(typeof(Image), typeof(BitmapSource))]
    public class ImageToBitmapSourceConverter : IValueConverter
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Image myImage = (Image)value;

            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
