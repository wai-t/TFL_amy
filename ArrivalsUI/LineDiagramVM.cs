using System.Collections.ObjectModel;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    public class LineDiagramVM
    {
        public ObservableCollection<LineDiagramTabVM> Tabs { get; set; } = [];
        public LineDiagramTabVM SelectedTab { get; set; }

        public ObservableCollection<PlatformArrivals> Arrivals { get; set; } = [];

        public LineDiagramVM()
        {
            MetaData.Lines.ForEach(
                l => Tabs.Add(
                    new LineDiagramTabVM(l)
                )
            );
            SelectedTab = Tabs[0];
        }

        public async Task HandleStationSelection(Station station)
        {
            var line = SelectedTab.Header;
            var arrivals = await PlatformArrivalsClient.GetArrivalsAsync(station.MatchedStop.Select(m => m.Id).ToList(), [line]);
            Arrivals.Clear();
            foreach (var arrival in arrivals)
            {
                Arrivals.Add(arrival);
            }
        }
    }

    public class LineDiagramTabVM
    {
        public string Header { get; set; }

        private Lazy<LineDiagramBrowserVM> _lineDiagram;

        public LineDiagramTabVM(string header)
        {
            Header = header;
            _lineDiagram
                = new Lazy<LineDiagramBrowserVM>(() => new LineDiagramBrowserVM(Header));
        }
        public LineDiagramBrowserVM LineDiagramViewModel
        {
            get
            {
                return _lineDiagram.Value;
            }
        }
    }
}
