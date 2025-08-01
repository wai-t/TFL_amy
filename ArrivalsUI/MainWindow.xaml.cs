using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArrivalsUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Lazy<LineDiagramWindow> _lineDiagramWindow = new Lazy<LineDiagramWindow>(() => new LineDiagramWindow());
        Lazy<StationBrowsingWindow> _stationBrowsingWindow = new Lazy<StationBrowsingWindow>(() => new StationBrowsingWindow());
        Lazy<Map> _mapWindow = new Lazy<Map>(() => new Map());
        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = _lineDiagramWindow.Value;
        }

        private void LineDiagrams_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _lineDiagramWindow.Value;
        }

        private void BrowseStationArrivals_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _stationBrowsingWindow.Value;
        }

        private void Map_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _mapWindow.Value;
        }
    }
}