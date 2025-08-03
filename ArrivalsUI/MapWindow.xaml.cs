using MapControl;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Color = System.Windows.Media.Color;
using TflNetworkBuilder;

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
        private void MapPath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ui = sender as FrameworkElement;
            var templatedParent = ui?.TemplatedParent as FrameworkElement;

            var stopPointVM = templatedParent?.DataContext as StopPointItem;

            if (stopPointVM != null)
            {
                // Handle the click event for the stop point
                viewModel.HandleStationSelection(stopPointVM.Id, stopPointVM.Lines);
            }
        }
    }


    // This is a list of TfL lines and their corresponding RGB codes for use in the LineDiagramBrowser  
    public class LineColours
    {
        public static SolidColorBrush LineBrush(string lineId)
        {
            if (LineColorMap.TryGetValue(lineId, out var color))
            {
                return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
            }
            return Brushes.Black; // Default color if not found
        }
        public static readonly Dictionary<string, Color> LineColorMap = new()
        {
            { "bakerloo", Color.FromRgb(179, 99, 5) },
            { "central", Color.FromRgb(227, 32, 23) },
            { "circle", Color.FromRgb(255, 211, 0) },
            { "district", Color.FromRgb(0, 120, 42) },
            { "hammersmith-city", Color.FromRgb(243, 169, 187) },
            { "jubilee", Color.FromRgb(160, 165, 169) },
            { "metropolitan", Color.FromRgb(155, 0, 86) },
            { "northern", Color.FromRgb(0, 0, 0) },
            { "piccadilly", Color.FromRgb(0, 54, 136) },
            { "victoria", Color.FromRgb(0, 152, 212) },
            { "waterloo-city", Color.FromRgb(149, 205, 186) },
            { "dlr", Color.FromRgb(0, 164, 167) },
            { "elizabeth", Color.FromRgb(113, 86, 165) }
        };
    }
}
