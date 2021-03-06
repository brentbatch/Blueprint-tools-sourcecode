﻿using System;
using System.Collections.Generic;
using System.IO;
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
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Drawing;
namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for OpenWindow.xaml
    /// </summary>
    public partial class OpenWindow : Window
    {
        MainWindow mainwindow;
        Thread loadbp;
        public OpenWindow(MainWindow mainWindow)
        {
            this.mainwindow = mainWindow;
            InitializeComponent();

            //load mainwindow.blueprints in list
            loadbp = new Thread(new ThreadStart(Loadblueprints));
            loadbp.SetApartmentState(ApartmentState.STA);
            loadbp.IsBackground = true;
            loadbp.Start();
            //Loadblueprints(""); 
        }
        
        public void Loadblueprints()//threaded
        {
            string searchby = "";
            int totalbps = 0;
            while (true)
            {
                while(totalbps != Database.blueprints.Count())
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        searchby = TextBox_Search.Text;
                    }));

                    //filter:
                    //if()
                    try
                    {
                        List<Blueprint> items = new List<Blueprint>();
                        foreach (string path in Database.blueprints.Keys)
                        {
                            Blueprint blueprint = new Blueprint(path, Database.blueprints[path]);
                            string name = blueprint.getname();
                            if ((searchby == "" || blueprint.getname().ToLower().Contains(searchby.ToLower()))&& Directory.Exists(path))
                            {
                                items.Add(blueprint);
                            }
                        }
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            listBox_blueprints.ItemsSource = null;
                            listBox_blueprints.Items.Clear();
                            listBox_blueprints.ItemsSource = items;
                        }));
                    }
                    catch { }
                    totalbps = Database.blueprints.Count();
                }
                try
                {

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        if (Database.bprefresh == true)
                        {
                            Database.bprefresh = false;
                            button_refresh_Copy_Click(null, null);
                        }
                    }));
                }
                catch
                {
                    break;
                }
                Thread.Sleep(1000);
            }


        }
        private void button_refresh_Copy_Click(object sender, RoutedEventArgs e)
        {
            Label_name.Content = "/";
            loadbp.Abort();
            loadbp = new Thread(new ThreadStart(Loadblueprints));
            loadbp.Start();
        }

        private void ListBox_Selectionchanged(object sender, RoutedEventArgs e)
        {
            if(listBox_blueprints.SelectedIndex!=-1)
            {
                try
                {
                    string bp = ((Blueprint)listBox_blueprints.SelectedItem).blueprintpath.ToString();
                    dynamic desc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(bp + @"\description.json"));
                    Label_name.Content = desc.name;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }


        private void button_LOAD_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        public void Load()
        {
            Loadwindow l = new Loadwindow();
            string bppath = "";
            try
            {
                bppath = ((Blueprint)listBox_blueprints.SelectedItem).blueprintpath.ToString();

                l.Show();
                new BP(bppath);
                mainwindow.RenderBlueprint();


                if (mainwindow.advancedconnections != null && mainwindow.advancedconnections.IsLoaded) mainwindow.advancedconnections.Update();
                if (mainwindow.advancedcolorwindow != null && mainwindow.advancedcolorwindow.IsLoaded) mainwindow.advancedcolorwindow.Update();
                if (mainwindow.swapblockswindow != null && mainwindow.swapblockswindow.IsLoaded) mainwindow.swapblockswindow.Update();
                if (mainwindow.blockProperties != null && mainwindow.blockProperties.IsLoaded) mainwindow.blockProperties.Update();
                if (mainwindow.areaProperties != null && mainwindow.areaProperties.IsLoaded) mainwindow.areaProperties.Update();
                if (mainwindow.blockPropertiesRAW != null && mainwindow.blockPropertiesRAW.IsLoaded) mainwindow.blockPropertiesRAW.Update();

                l.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                l.Close();
            }
        }

        private void button_DELETE_Click(object sender, RoutedEventArgs e)
        {
            //messagebox YES/NO
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove this blueprint?"," ",MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
            if(result == MessageBoxResult.Yes)
            {
                try
                {
                    if(((Blueprint)listBox_blueprints.SelectedItem).blueprintpath.Contains(Database.steamapps))
                    {
                        MessageBoxResult res2 = MessageBox.Show("Removing a workshop blueprint will result in the game complaining:\n'failed to find blahblah, verify integrity'!\n\nAre you sure you want to continue?\nYou will have to 'verify integrity' through steam", "WORKSHOP BLUEPRINT!", MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                        if (res2 == MessageBoxResult.No)
                            throw new OperationCanceledException("Canceled removing workshop blueprint");

                    }
                    Directory.Delete(((Blueprint)listBox_blueprints.SelectedItem).blueprintpath, true);
                    //Database.LoadBpsIn(Database.User_ + "\\blueprints");
                    Database.blueprints.Remove(((Blueprint)listBox_blueprints.SelectedItem).blueprintpath);
                    button_refresh_Copy_Click(null, null);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,"failed to remove");
                }
            }
        }
    }
    public class Blueprint
    {
        public string image { get; set; }
        

        public string blueprintpath { get; private set; }
        public BitmapSource imsource { get; private set; }
        //public string name { get; private set; }

        public Blueprint(string blueprintpath, BitmapSource src)
        {
            this.blueprintpath = blueprintpath;
            this.imsource = src;
            this.image = blueprintpath + "\\icon.png";
            //name = getname();
        }

        public string getname()
        {
            try
            {
                dynamic desc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(blueprintpath + @"\description.json"));
                return desc.name.ToString().ToLower();
            }
            catch
            {
                return null;
            }
        }

    }
    
}
