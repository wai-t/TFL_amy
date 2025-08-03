using System.Windows;

namespace ArrivalsUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Lazy<LineDiagramWindow> _lineDiagramWindow = new Lazy<LineDiagramWindow>(() => new LineDiagramWindow());
        Lazy<StationBrowsingWindow> _stationBrowsingWindow = new Lazy<StationBrowsingWindow>(() => new StationBrowsingWindow());
        Lazy<MapWindow> _mapWindow = new Lazy<MapWindow>(() => new MapWindow());
        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = _mapWindow.Value;
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