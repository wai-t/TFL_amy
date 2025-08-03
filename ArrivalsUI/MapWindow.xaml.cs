using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArrivalsUI
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : UserControl
    {
        MapWindowVM viewModel = new MapWindowVM();
        public MapWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        private async void MapPath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ui = sender as FrameworkElement;
            var templatedParent = ui?.TemplatedParent as FrameworkElement;

            var stopPointVM = templatedParent?.DataContext as StopPointItem;

            if (stopPointVM != null)
            {
                // Handle the click event for the stop point
                await viewModel.HandleStationSelection(stopPointVM);
            }
        }
    }
}
