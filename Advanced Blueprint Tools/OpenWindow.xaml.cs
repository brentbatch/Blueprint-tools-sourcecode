using System;
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

namespace Advanced_Blueprint_Tools
{
    /// <summary>
    /// Interaction logic for OpenWindow.xaml
    /// </summary>
    public partial class OpenWindow : Window
    {
        MainWindow mainwindow;
        Thread loadbp;
        public OpenWindow(MainWindow mainwindow)
        {
            InitializeComponent();
            this.mainwindow = mainwindow;

            //load mainwindow.blueprints in list
            loadbp = new Thread(new ThreadStart(Loadblueprints));
            loadbp.Start();
            //Loadblueprints(""); 
        }
        public void Loadblueprints() //TextBox_Search.Text.ToString()
        {
            string searchby="";
            this.Dispatcher.Invoke((Action)(() =>
            {//this refer to form in WPF application 
                searchby = TextBox_Search.Text.ToString();
            }));
            List<string> blueprints = new List<string>();
            while(true)
            { 
                while (blueprints.Count < mainwindow.blueprints.Count)
                {
                    blueprints = new List<string>();
                    blueprints.AddRange(mainwindow.blueprints);

                    int i = 0;
                    foreach (string blueprint in blueprints)
                        if (File.Exists(blueprint + @"\blueprint.json") && File.Exists(blueprint + @"\icon.png") && File.Exists(blueprint + @"\description.json"))
                        {
                            try
                            {
                                dynamic desc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(blueprint + @"\description.json"));
                                string descname = desc.name.ToString().ToLower();
                                if (searchby == "" || descname.Contains(searchby.ToLower()))
                                {
                                    this.Dispatcher.Invoke((Action)(() =>
                                    {
                                        listBox_blueprints.Items.Add(new Blueprint(blueprint + @"\icon.png"));
                                    }));
                                    i++;
                                }
                            }
                            catch (Exception e)
                            {
                                if(e.HResult != -2146233040)
                                {
                                    MessageBoxResult result = MessageBox.Show(e.Message + "\n\n" + blueprint + "\n\nWould you like to open this location?", "Couldn't load this blueprint!", MessageBoxButton.YesNo);
                                    if (result == MessageBoxResult.Yes)
                                    {
                                        System.Diagnostics.Process.Start(blueprint);
                                    }
                                }
                            }
                        }
                }
                Thread.Sleep(3);
                try
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                     //check if window still active

                        if (this.IsActive)
                        { }
                    }));
                }
                catch
                {
                    break;
                }//end thread if window closed
            }

        }   

        private void ListBox_Selectionchanged(object sender, RoutedEventArgs e)
        {
            if(listBox_blueprints.SelectedIndex!=-1)
            {
                string bp = Directory.GetParent(((Blueprint)listBox_blueprints.SelectedItem).image).ToString();
                dynamic desc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(bp + @"\description.json"));
                Label_name.Content = desc.name;
            }

        }

        private void button_refresh_Copy_Click(object sender, RoutedEventArgs e)
        {
            listBox_blueprints.Items.Clear();
            Label_name.Content = "/";
            loadbp.Abort();
            loadbp = new Thread(new ThreadStart(Loadblueprints));
            loadbp.Start();
        }

        private void button_LOAD_Click(object sender, RoutedEventArgs e)
        {
            //load();
            Thread loadthread = new Thread(new ThreadStart(load));
            loadthread.SetApartmentState(ApartmentState.STA);
            loadthread.Start();
        }

        public void load()
        {
            BP bp = null;
            dynamic blocks = mainwindow.getgameblocks();
            if (((JObject)blocks).Count > 100)
            {
                try
                {
                    string bppath = "";
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                        bppath = Directory.GetParent(((Blueprint)listBox_blueprints.SelectedItem).image).ToString();
                    }));
                    bp = new BP(bppath);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
                if (bp != null)
                    this.Dispatcher.Invoke((Action)(() =>
                    {//this refer to form in WPF application 
                        Loadwindow l = new Loadwindow();
                        l.Show();
                        mainwindow.OpenedBlueprint = bp;
                        mainwindow.UpdateOpenedBlueprint();
                        if (mainwindow.advancedconnections != null && mainwindow.advancedconnections.IsLoaded) mainwindow.advancedconnections.update();
                        if (mainwindow.advancedcolorwindow != null && mainwindow.advancedcolorwindow.IsLoaded) mainwindow.advancedcolorwindow.update();
                        if (mainwindow.swapblockswindow != null && mainwindow.swapblockswindow.IsLoaded) mainwindow.swapblockswindow.update();
                        if (mainwindow.blockProperties != null && mainwindow.blockProperties.IsLoaded) mainwindow.blockProperties.Close();

                        l.Close();

                    }));
            }
            else
                MessageBox.Show("Resources aren't loaded yet, please give it a moment");
        }

        private void button_DELETE_Click(object sender, RoutedEventArgs e)
        {
            //messagebox YES/NO
        }
    }
    public class Blueprint //shit for pictures
    {
        public string image { get; set; }

        
        public Blueprint(string image)
        {
            this.image = image;
        }
    }

    
}
