using System.Collections.ObjectModel;
using tfl_stats.Tfl;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    public class LineDiagramVM
    {
        public ObservableCollection<LineDiagramTabVM> Tabs { get; set; } = [];
        public LineDiagramTabVM SelectedTab { get; set; }

        public ObservableCollection<PlatformArrivals> Arrivals { get; set; } = [];

        private List<string> _lines =  [
            "bakerloo",
            "central",
            "circle",
            "district",
            "dlr",
            "elizabeth",
            "hammersmith-city",
            "jubilee",
            "metropolitan",
            "northern",
            "piccadilly",
            "victoria",
            "waterloo-city",
        ];

        public LineDiagramVM() 
        {
            _lines.ForEach(
                l => Tabs.Add(
                    new LineDiagramTabVM(l)
                )
            );
            SelectedTab = Tabs[0];
        }

        public async void HandleStationSelection(Station station)
        {
            var line = SelectedTab.Header;
            var stopPoint = station.MatchedStop.Where(m => m.Lines.Select(l => l.Id).Contains(line)).First();
            var predictions = await ApiClient.LineClient.ArrivalsAsync([line], stopPoint.Id, null, null);
            UpdatePredictions(predictions);
        }
        internal void UpdatePredictions(ICollection<Prediction> predictions)
        {
            Arrivals.Clear();
            Dictionary<string, IList<Prediction>> platformArrivals = [];

            foreach (var prediction in predictions)
            {
                if (!platformArrivals.TryGetValue(prediction.PlatformName, out var platformList))
                {
                    platformList = new List<Prediction>();
                    platformArrivals[prediction.PlatformName] = platformList;
                }
                platformList.Add(prediction);
            }

            foreach (var platform in platformArrivals)
            {
                Arrivals.Add(new PlatformArrivals(platform.Key,
                    new ObservableCollection<Prediction>(platform.Value.OrderBy(p => p.TimeToStation))));
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
