using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            if (e.NewValue is OrderedStation station)
            {
                ViewModel.SelectedStation = station;

                var line = ViewModel.StationLineLookup[station.Id];

                await RefreshPredictions(station, line);

            }

        }

        private async Task RefreshPredictions(OrderedStation station, string line)
        {
            var predictions = await ApiClient.LineClient.ArrivalsAsync([line], station.Id, null, null);

            ViewModel.UpdatePredictions(predictions);
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                e.Handled = true;
                if (ViewModel.SelectedStation != null)
                {
                    await RefreshPredictions(ViewModel.SelectedStation, ViewModel.StationLineLookup[ViewModel.SelectedStation.Id]);
                }
            }
        }
    }

}
