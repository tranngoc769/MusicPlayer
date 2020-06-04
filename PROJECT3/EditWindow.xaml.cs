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

namespace PROJECT3
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public string newPlaylist;
        public EditWindow(string listName)
        {
            this.Name = "DDD";
            InitializeComponent();
            newPlaylist = listName;
            fromText.Text = listName;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            newPlaylist = fromText.Text;
            DialogResult = true;
        }
    }
}
