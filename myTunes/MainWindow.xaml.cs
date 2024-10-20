﻿using Microsoft.Win32;
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
                // C+P'd code to this method, so deleting songs can update the dataGrid too
                ReloadDataGrid(selectedPlaylist); 
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
                songDataGrid.Focus();
                songDataGrid.SelectedItem = songDataGrid.Items[songDataGrid.Items.Count - 1];
                songDataGrid.ScrollIntoView(songDataGrid.SelectedItem);
            }
        }

        private void addPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistWindow newPlaylistWindow = new();
            var result = newPlaylistWindow.ShowDialog();
            if (result == true)
            {
                string? playListName = newPlaylistWindow.PlaylistName;
                // asked GitHub Copilot "How to test for empty string along with space characters"
                if (string.IsNullOrWhiteSpace(playListName))
                {
                    MessageBox.Show("Playlist name cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
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

        private void PlaySong_MenuItemClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = songDataGrid.SelectedItems[0];
            var data = selectedItem as DataRowView;
            if (data != null)
            {
                var dataRow = data.Row;
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
        }

        private void DeleteSong_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (songDataGrid.SelectedItem != null)
            {
                // Got this method of acquiring song Id from https://www.syncfusion.com/forums/160649/get-the-value-from-the-first-column-of-a-selected-row 
                var selectedItem = songDataGrid.SelectedItems[0];
                var data = selectedItem as DataRowView;
                if (data != null)
                {
                    var dataRow = data.Row;
                    int songId = (int)dataRow["Id"];
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete \"{dataRow["Title"].ToString()}\"?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        musicRepo.DeleteSong(songId);
                    }
                }
            }
        }

        private void DeleteSongFromPlaylist_MenuItemClick(Object sender, RoutedEventArgs e)
        { 
            if (songDataGrid.SelectedItem != null)
            {
                // Got this method of acquiring song Id from https://www.syncfusion.com/forums/160649/get-the-value-from-the-first-column-of-a-selected-row 
                var selectedItem = songDataGrid.SelectedItems[0];
                var data = selectedItem as DataRowView;
                if (data != null)
                {
                    var selectedPlaylist = songListBox.SelectedItem as String;
                    if (selectedPlaylist != null)
                    {
                        var dataRow = data.Row;

                        int songId = Convert.ToInt32(dataRow["id"]);
                        int songPos = Convert.ToInt32(dataRow["position"]);
                        musicRepo.RemoveSongFromPlaylist(songPos, songId, selectedPlaylist);
                        songListBox.SelectedItem = songListBox.SelectedItem; 
                        ReloadDataGrid(selectedPlaylist);
                    }
                }
            }
        }

    private void PlayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedItem = songDataGrid.SelectedItems[0];
            var data = selectedItem as DataRowView;
            if (data != null)
            {
                var dataRow = data.Row;
                int songId = Convert.ToInt32(dataRow["Id"]);
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

        // Creates a Context Menu when songDataGrid loads
        // Got most information from 
        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/contextmenu-overview?view=netframeworkdesktop-4.8 
        private void songDataGrid_LayoutUpdated(object sender, EventArgs e)
        {
            if (songListBox.SelectedItem == null)
            {
                return;
            }

            var contextMenu = new ContextMenu();
            songDataGrid.ContextMenu = contextMenu;
            var miPlay = new MenuItem();
            miPlay.Header = "Play";
            miPlay.Click += PlaySong_MenuItemClick;
            contextMenu.Items.Add(miPlay);

            var selectedPlaylist = songListBox.SelectedItem as String;

            if (selectedPlaylist == "All Music")
            {
                var miRemove = new MenuItem();
                miRemove.Header = "Remove";
                miRemove.Click += DeleteSong_MenuItemClick;
                contextMenu.Items.Add(miRemove);
            }
            else
            {
                var miRemoveFromPlaylist = new MenuItem();
                miRemoveFromPlaylist.Header = "Remove from Playlst";
                miRemoveFromPlaylist.Click += DeleteSongFromPlaylist_MenuItemClick;
                contextMenu.Items.Add(miRemoveFromPlaylist);
            }
        }

        private void ReloadDataGrid(string selectedPlaylist)
        {
            if (selectedPlaylist != null)
            {
                if (selectedPlaylist == "All Music")
                {
                    songDataGrid.ItemsSource = musicRepo.Songs.DefaultView;
                    // Asked ChatGPT "I have my dataGrid, but I want it to load sorted by a column automatically how do I do that"
                    // Responded with collection view, but I implemented it slightly differently by applying to the Items directly, haven't found anything wrong with it so far
                    songDataGrid.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Title", System.ComponentModel.ListSortDirection.Ascending));
                }
                else
                {
                    songDataGrid.ItemsSource = musicRepo.SongsForPlaylist(selectedPlaylist).DefaultView;
                }
            }
        }
    }
}