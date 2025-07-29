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
using System.Windows.Shapes;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    /// <summary>
    /// Interaction logic for TabbedLines.xaml
    /// </summary>
    public partial class TabbedLines : Window
    {
        public TabbedLinesVM ViewModel => (TabbedLinesVM)DataContext;
        public TabbedLines()
        {
            InitializeComponent();

            DataContext = new TabbedLinesVM();
        }

        public void LineDiagramBrowser_StationSelected(object sender, StationSelectedEventArgs args)
        {
            var station = args.SelectedStation as Station;
            ViewModel.HandleStationSelection(station);
        }
    }
}
