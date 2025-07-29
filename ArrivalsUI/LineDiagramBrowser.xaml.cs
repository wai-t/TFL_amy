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
using TflNetworkBuilder;

namespace ArrivalsUI
{
    public delegate void StationSelectedHandler(object sender, StationSelectedEventArgs station);
    /// <summary>
    /// Interaction logic for LineDiagramBrowser.xaml
    /// </summary>
    public partial class LineDiagramBrowser : UserControl
    {
        public static readonly RoutedEvent StationSelectedEvent =
            EventManager.RegisterRoutedEvent(
            "StationSelected",
            RoutingStrategy.Bubble,
            typeof(StationSelectedHandler),
            typeof(LineDiagramBrowser));

        public event StationSelectedHandler StationSelected
        {
            add { AddHandler(StationSelectedEvent, value); }
            remove { RemoveHandler(StationSelectedEvent, value); }
        }        


        public LineDiagramBrowser()
        {
            InitializeComponent();


        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock != null)
            {
                if (textBlock.DataContext is Label label)
                {
                    var args = new StationSelectedEventArgs(StationSelectedEvent, label.Station);
                    RaiseEvent(args);
                }
            }
        }
    }
    public class StationSelectedEventArgs : RoutedEventArgs
    {
        public Station SelectedStation { get; }

        public StationSelectedEventArgs(RoutedEvent routedEvent, Station station)
            : base(routedEvent)
        {
            SelectedStation = station;
        }
    }
}
