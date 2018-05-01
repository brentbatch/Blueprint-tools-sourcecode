using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace Advanced_Blueprint_Tools
{
    class Updater
    {

        public bool CheckForUpdates() //for current branch & version: 'properties.settings'
        {
            try
            {
                
                var request = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/brentbatch/blueprinttool/master/version.json");
                
                request.UserAgent = "VSTS-Get";
                request.ContentType = "application/json";
                request.Method = "GET";

                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                var result = reader.ReadToEnd();
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
                int gotoversion = Convert.ToInt32(json.Public.version.ToString());
                if (Properties.Settings.Default.branch == "Test")
                {
                    gotoversion = Convert.ToInt32(json.Test.version.ToString());
                }

                /*
                if (Properties.Settings.Default.version > gotoversion && Properties.Settings.Default.branch != "Test")
                {//going from high version to public branch(lower version)
                    Database.Notifications.Add(new Notification("manual update required",
                    new Task(() =>
                    {
                        System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("going from Test version to public version requires a manual re-install\nYou'll need to remove the current version and install public version yourself\n\nOpen download link of 'public' branch in browser?", "Update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://github.com/brentbatch/blueprinttool/archive/master.zip");//public
                        } //manual update
                    })
                    ));
                    return true;
                }*/
                if (Properties.Settings.Default.version < gotoversion || (Properties.Settings.Default.version > gotoversion && Properties.Settings.Default.branch != "Test"))
                {//normal update
                    Database.Notifications.Add(new Notification("Update Available!",
                    new Task(() =>
                    {
                        System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Found a new update for branch: "+ Properties.Settings.Default.branch +"\n\nDo you want to update to this version?\ncurrent:"+ Properties.Settings.Default.version+ "\nbranch version:"+gotoversion.ToString()+"\n\nWould you like to download this version?", "Update Available!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            string root = Directory.GetDirectoryRoot("Meshes") + "bptools\\";
                            if (File.Exists(root + "update.zip"))
                                File.Delete(root + "update.zip");
                            string applicationfiles = "";
                            if (!Directory.Exists(root))
                                Directory.CreateDirectory(root);
                            if (Properties.Settings.Default.branch == "Test")
                            {
                                //System.Diagnostics.Process.Start("https://github.com/brentbatch/Blueprinttool_test/archive/master.zip");
                                try
                                {
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                    using (WebClient webClient = new WebClient())
                                    {
                                        webClient.DownloadFile("https://github.com/brentbatch/Blueprinttool_test/archive/master.zip", root+"update.zip");
                                    }
                                    //create new desktop shortcuts
                                    applicationfiles = root + "Update\\Blueprinttool_test-master\\Application Files";
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message);
                                }

                            }
                            else
                            {//public
                                //System.Diagnostics.Process.Start("https://github.com/brentbatch/Blueprinttool/archive/master.zip");
                                try
                                {
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                    using (WebClient webClient = new WebClient())
                                    {
                                        webClient.DownloadFile("https://github.com/brentbatch/Blueprinttool/archive/master.zip", @"update.zip");
                                    }
                                    applicationfiles = root + "Update\\Blueprinttool-master\\Application Files";
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message);
                                }
                            }
                            String ZipPath = root + @"update.zip";
                            String extractPath = root + @"Update";
                            string startPath = root + @"start";
                            //System.IO.Compression.ZipFile.CreateFromDirectory(startPath, ZipPath);
                            string master = Path.GetDirectoryName(applicationfiles);
                            if (File.Exists(master + "\\Advanced Blueprint Tools.application"))
                                File.Delete(master + "\\Advanced Blueprint Tools.application");
                            if (File.Exists(master + "\\README.md"))
                                File.Delete(master + "\\README.md");
                            if (File.Exists(master + "\\version.json"))
                                File.Delete(master + "\\version.json");
                            if (File.Exists(master + "\\setup.exe"))
                                File.Delete(master + "\\setup.exe");
                            try
                            {
                                System.IO.Compression.ZipFile.ExtractToDirectory(ZipPath, extractPath);
                            }
                            catch { }

                            dynamic config = new JObject();
                            config.steamapps = Properties.Settings.Default.steamapps;
                            config.times = Properties.Settings.Default.times;
                            config.branch = Properties.Settings.Default.branch;
                            config.wires = Properties.Settings.Default.wires;
                            config.safemode = Properties.Settings.Default.safemode;
                            config.colorwires = Properties.Settings.Default.colorwires;
                            config.wirecolor = Properties.Settings.Default.wirecolor;
                            config.blobcolor = Properties.Settings.Default.blobcolor;

                            DateTime lasthigh = new DateTime(1900, 1, 1);
                            string findlastversionname = "";
                            foreach (string subdir in Directory.GetDirectories(applicationfiles)) //get user_numbers folder that is last used
                            {
                                DirectoryInfo fi1 = new DirectoryInfo(subdir);
                                DateTime created = fi1.LastWriteTime;
                                fi1 = new DirectoryInfo(subdir);

                                if (created > lasthigh)
                                {
                                    findlastversionname = subdir;
                                    lasthigh = created;
                                }
                            }

                            File.WriteAllText(findlastversionname + "\\config", config.ToString());

                            object shDesktop = (object)"Desktop";
                            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Advanced Blueprint Tools.lnk";
                            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                            shortcut.Description = "Shortcut for blueprint tool "+gotoversion;
                            shortcut.TargetPath = findlastversionname + "\\Advanced Blueprint Tools.exe";
                            shortcut.WorkingDirectory = findlastversionname;
                            shortcut.Save();
                            MessageBox.Show("new version installed, close current application and open shortcut on desktop.");
                            
                        }
                    })
                    ));
                    return true;
                }
            }
            catch
            {
                Database.Notifications.Add(new Notification("unable to check for updates",null));
            }

            return false;
        }
    }
}
