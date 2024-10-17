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
        public MainWindow()
        {
            InitializeComponent();

            songListBox.ItemsSource = musicRepo.Playlists;
        }

        private void songListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}