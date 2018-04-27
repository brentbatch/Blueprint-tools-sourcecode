using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        
        public NotificationWindow()
        {
            InitializeComponent();
            fill_listbox();
            listBox_notis.MouseDoubleClick += new MouseButtonEventHandler(Notification_Click);
            new Task(() =>
            {
                try
                {
                    int amountnotis = 0;
                    while (true)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            if (amountnotis != Database.Notifications.Count)
                                this.fill_listbox();
                            amountnotis = Database.Notifications.Count;
                            if (amountnotis == 0)
                                this.Close();
                        }));
                        Thread.Sleep(2000);
                    }
                }
                catch { }
            }).Start();
        }

        private void fill_listbox()
        {
            listBox_notis.Items.Clear();
            foreach (Notification notification in Database.Notifications)
            {
                listBox_notis.Items.Add(notification);
            }

        }

        private void Notification_Click(object sender, RoutedEventArgs e)
        {
            if(listBox_notis.SelectedIndex>-1)
            {
                Notification currentnoti = ((Notification)listBox_notis.SelectedItem);
                currentnoti.performTask();
                Database.Notifications.Remove(((Notification)listBox_notis.SelectedItem));

                fill_listbox();//refresh
            }
        }
    }



    public class Notification
    {
        
        public string description { get; private set; }

        private Task task;
        //public string tag{get; private set;}
        public void performTask()
        {
            try
            {
                if (task != null)
                    task.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "\nerror while showing notification");
            }
        }
        public Notification(string description, Task task)
        {
            this.description = description;
            this.task = task;
        }
    }
}
