using Microsoft.Win32;
using System.Collections.ObjectModel;
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

// TO DO : "Play" inside Context Menu (should call play song method), and change Context Menu "Remove" to "Remove from Playlist" if a playlist is selected

namespace myTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MusicRepo musicRepo = new MusicRepo();
        private ObservableCollection<String> musicList = new ObservableCollection<String>();
        private MediaPlayer mediaPlayer;

        private bool isPlaying = false; // For disabling stop button
        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();

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
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            // FileDialog filter from https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.filedialog.filter?view=windowsdesktop-8.0
            openFileDialog1.Filter = "Music files (*.mp3;*.mp4;*.wma;*.wav)|*.mp3;*.mp4;*.wma;*.wav|All files|*.*";
            if (openFileDialog1.ShowDialog() == true)
            {
                musicRepo.AddSong(openFileDialog1.FileName);
            }
        }

        private void addPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistWindow newPlaylistWindow = new();
            var result = newPlaylistWindow.ShowDialog();
            if (result == true)
            {
                string? playListName = newPlaylistWindow.PlaylistName;
                if (playListName != null)
                {
                    bool nameIsValid = musicRepo.AddPlaylist(playListName);
                    if (nameIsValid)
                    {
                        musicList.Add(playListName);
                        songListBox.ItemsSource = musicList;
                    }
                    else
                    {
                        MessageBox.Show("Playlist name already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        
        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new();
            aboutWindow.ShowDialog();
        }

        private void DeleteSong_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (songDataGrid.SelectedItem != null)
            {
                // Got this method of acquiring song Id from https://www.syncfusion.com/forums/160649/get-the-value-from-the-first-column-of-a-selected-row 
                var selectedItem = songDataGrid.SelectedItems[0];
                var dataRow = (selectedItem as DataRowView).Row;
                int songId = (int)dataRow["Id"];
                musicRepo.DeleteSong(songId);
            }
        }
        private void PlayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedItem = songDataGrid.SelectedItems[0];
            var dataRow = (selectedItem as DataRowView).Row;
            int songId = (int)dataRow["Id"];
            Song? s = musicRepo.GetSong(songId);
            if (s != null)
            {
                if (s.Filename != null)
                {
                    mediaPlayer.Open(new Uri(s.Filename));
                    mediaPlayer.Play();
                    isPlaying = true;
                }
            }
        }

        private void PlayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (songDataGrid.SelectedItem == null)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }
        private void StopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Stop();
            isPlaying = false;
        }

        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (isPlaying)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

    }
}