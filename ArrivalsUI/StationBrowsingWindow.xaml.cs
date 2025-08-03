using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    /// <summary>
    /// Interaction logic for StationBrowsingWindow.xaml
    /// </summary>
    public partial class StationBrowsingWindow : UserControl
    {
        StationBrowsingVM ViewModel => (StationBrowsingVM)DataContext;
        public StationBrowsingWindow()
        {
            InitializeComponent();
            DataContext = new StationBrowsingVM();
        }

        private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is StationNode station)
            {
                ViewModel.SelectedStation = station;

                var lines = ViewModel.StationLineLookup[station.StationId];

                await ViewModel.HandleSelection(station, lines);

            }

        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                e.Handled = true;
                if (ViewModel.SelectedStation != null)
                {
                    await ViewModel.HandleSelection(ViewModel.SelectedStation, ViewModel.StationLineLookup[ViewModel.SelectedStation.Station.MatchedStop.First().Id]);
                }
            }
        }
    }

}
