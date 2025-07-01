using System.IO;
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

namespace ArrivalsUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel ViewModel => (MainViewModel)DataContext;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

        }

        private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is OrderedStation station)
            {
                ViewModel.SelectedStation = station;

                var line = ViewModel.StationLineLookup[station.Id];

                var predictions = await ApiClient.LineClient.ArrivalsAsync([line], station.Id, null, null);

                ViewModel.UpdatePredictions(predictions);

            }

        }
    }
}