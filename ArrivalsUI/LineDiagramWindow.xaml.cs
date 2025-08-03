using System.Windows.Controls;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    /// <summary>
    /// Interaction logic for LineDiagramWindow.xaml
    /// </summary>
    public partial class LineDiagramWindow : UserControl
    {
        public LineDiagramVM ViewModel => (LineDiagramVM)DataContext;
        public LineDiagramWindow()
        {
            InitializeComponent();
            DataContext = new LineDiagramVM();
        }

        public async void LineDiagramBrowser_StationSelected(object sender, StationSelectedEventArgs args)
        {
            var station = args.SelectedStation as Station;
            await ViewModel.HandleStationSelection(station);
        }
    }
}
