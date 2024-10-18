using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace myTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MusicRepo musicRepo = new MusicRepo();
        private List<String> musicList = new List<String>();
        public MainWindow()
        {
            InitializeComponent();

            musicList.Add("All Music");
            foreach (var playlist in musicRepo.Playlists)
            {
                musicList.Add(playlist);
            }
            
            songListBox.ItemsSource = musicList;
            songListBox.SelectedIndex = 0;
        }

        private void songListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPlaylist = songListBox.SelectedItem as String;
            if (selectedPlaylist != null)
            {
                if (selectedPlaylist == "All Music")
                {
                    songDataGrid.ItemsSource = musicRepo.Songs.DefaultView;
                }
                else
                {
                    songDataGrid.ItemsSource = musicRepo.SongsForPlaylist(selectedPlaylist).DefaultView;
                }
            }
        }

        private void addSongButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void addPlaylistButton_Click(object sender, RoutedEventArgs e)
        {

        }
        
        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new();
            aboutWindow.ShowDialog();
        }

    }
}