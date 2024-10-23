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
using TagLib;


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
        // Got the following code from ChatGPT asking how to sort a DataGrid
        System.ComponentModel.SortDescription sorting = new System.ComponentModel.SortDescription("Title", System.ComponentModel.ListSortDirection.Ascending);

        private bool isPlaying = false; // For disabling stop button
        private Point startPos; // For mouse location (drag and drop)

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

            this.Closed += MainWindow_FormClosed;
            songDataGrid.Items.SortDescriptions.Add(sorting);
        }

        private void MainWindow_FormClosed(object? sender, EventArgs e)
        {
            musicRepo.Save();
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
            songDataGrid.Items.SortDescriptions.Remove(sorting);
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
                string? playlistName = newPlaylistWindow.PlaylistName;
                // asked GitHub Copilot "How to test for empty string along with space characters"
                if (string.IsNullOrWhiteSpace(playlistName))
                {
                    MessageBox.Show("Playlist name cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (playlistName != null)
                {
                    bool nameIsValid = musicRepo.AddPlaylist(playlistName);
                    if (nameIsValid)
                    {
                        musicList.Add(playlistName);
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
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete" +
                        $" \"{dataRow["Title"].ToString()}\"?", "Confirmation",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                miRemoveFromPlaylist.Header = "Remove from Playlist";
                miRemoveFromPlaylist.Click += DeleteSongFromPlaylist_MenuItemClick;
                contextMenu.Items.Add(miRemoveFromPlaylist);
            }
        }

        private void songListBox_LayoutUpdated(object sender, EventArgs e)
        {
            foreach (var item in songListBox.Items)
            {
                // asked ChatGPT "How to make sure a ListBoxItem is not null"
                ListBoxItem listBoxItem = (ListBoxItem)songListBox.ItemContainerGenerator.ContainerFromItem(item);

                if (listBoxItem != null)
                {
                    if (item.ToString() == "All Music")
                    {
                        listBoxItem.ContextMenu = null;
                    }
                    else
                    {
                        ContextMenu contextMenu = new ContextMenu();
                        MenuItem miRename = new MenuItem();
                        miRename.Header = "Rename";
                        miRename.Click += RenamePlaylist_Click;
                        contextMenu.Items.Add(miRename);

                        MenuItem miDelete = new MenuItem();
                        miDelete.Header = "Delete";
                        miDelete.Click += DeletePlaylist_Click;
                        contextMenu.Items.Add(miDelete);

                        listBoxItem.ContextMenu = contextMenu;
                    }
                }
            }
        }

        private void ReloadDataGrid(string selectedPlaylist)
        {
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

        private void RenamePlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlaylist = songListBox.SelectedItem as String;

            if (selectedPlaylist != null)
            {
                
            ChangePlaylistNameWindow changePlaylistNameWindow = new();
            var result = changePlaylistNameWindow.ShowDialog();
                if (result == true)
                {
                    string? playlistName = changePlaylistNameWindow.PlaylistName;
                    // asked GitHub Copilot "How to test for empty string along with space characters" (from addPlaylistButton_Click)
                    if (string.IsNullOrWhiteSpace(playlistName))
                    {
                        MessageBox.Show("Playlist name cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (playlistName != null)
                    {

                        bool nameIsValid = musicRepo.RenamePlaylist(selectedPlaylist, playlistName);
                        if (nameIsValid || playlistName == selectedPlaylist)
                        {
                            musicList[musicList.IndexOf(selectedPlaylist)] = playlistName;
                            songListBox.ItemsSource = musicList;
                        }
                        else
                        {
                            MessageBox.Show("Playlist name already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void DeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlaylist = songListBox.SelectedItem as String;
            if (selectedPlaylist != null)
            {
                if (selectedPlaylist != "All Music")
                {
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete" +
                        $" \"{selectedPlaylist}\"?", "Confirmation",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        musicRepo.DeletePlaylist(selectedPlaylist);
                        musicList.Remove(selectedPlaylist);
                        songListBox.ItemsSource = musicList;
                        songListBox.SelectedIndex = 0;
                    }
                }
            }
        }

        private void songDataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPoint = e.GetPosition(null);
            Vector diff = startPos - currentPoint;

            // Start Drag-Drop if mouse moved far away enough
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Initiate dragging
                DragDrop.DoDragDrop(songDataGrid, songDataGrid.SelectedItem, DragDropEffects.Move);
            }
        }

        private void songDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store mouse position
            startPos = e.GetPosition(null);
        }

        private void songListBox_DragOver(object sender, DragEventArgs e)
        {
            Label? playlist = sender as Label;
            if (playlist != null)
            {
                var playlistName = playlist.Content as string;
                if (playlistName != "All music")
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        private void songListBox_Drop(object sender, DragEventArgs e)
        {
            Label? playlist = sender as Label;
            if (playlist != null)
            {
                var playlistName = playlist.Content as string;
                var selectedItem = songDataGrid.SelectedItems[0];
                var data = selectedItem as DataRowView;
                if (data != null && playlistName != null)
                {
                    var dataRow = data.Row;
                    int songId = Convert.ToInt32(dataRow["Id"]);
                    musicRepo.AddSongToPlaylist(songId, playlistName);
                }
            }
        }
    }
}