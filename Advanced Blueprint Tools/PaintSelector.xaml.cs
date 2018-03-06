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
    /// Interaction logic for PaintSelector.xaml
    /// </summary>
    public partial class PaintSelector : Window
    {
        MainWindow window;
        string PaintColor;
        public PaintSelector(MainWindow window)
        {
            InitializeComponent();
            this.window = window;
            this.PaintColor = window.PaintColor;
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Brush b = button.Background;
            textbox_color.Text = "#"+ (b as SolidColorBrush).Color.ToString().Substring(3);
        }

        private void textbox_color_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeTextBoxColor();
        }

        private void changeTextBoxColor()
        {
            if(textbox_color.Text.Length==7)
            {
                try
                {
                    var bc = new BrushConverter();
                    string color = "#FF" + textbox_color.Text.Substring(1);
                    textbox_color.Background = (Brush)bc.ConvertFrom(color);
                }
                catch
                {
                    MessageBox.Show("Please use the right format\n \"#123abc\" where 1-9,a-f (hex)");
                }
            }
            else
            {
                var bc = new BrushConverter();
                textbox_color.Background = (Brush)bc.ConvertFrom("#eeeeee");
            }
            if(window!=null) window.PaintColor = textbox_color.Text;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string red = Convert.ToInt32(slider_red.Value).ToString("X");
            string green = Convert.ToInt32(slider_green.Value).ToString("X");
            string blue = Convert.ToInt32(slider_blue.Value).ToString("X");
            string color = "#" + red.PadLeft(2,'0') +""+ green.PadLeft(2,'0') +""+ blue.PadLeft(2,'0');
            textbox_color.Text = color;
        }
        
        private void textbox_color_MouseEnter(object sender, MouseEventArgs e)
        {
            changeTextBoxColor();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            textbox_color.Text = "#";
        }


        //onpaint change: this.window.paint = #iets;
    }
}
