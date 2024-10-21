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

namespace myTunes
{
    /// <summary>
    /// Interaction logic for ChangePlaylistNameWindow.xaml
    /// </summary>
    public partial class ChangePlaylistNameWindow : Window
    {
        public string? PlaylistName { get; private set; }
        public ChangePlaylistNameWindow()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            PlaylistName = playlistNameTextBox.Text;
            this.DialogResult = true;
        }
    }
}
