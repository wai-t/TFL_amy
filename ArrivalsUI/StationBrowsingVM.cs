using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    internal class StationBrowsingVM : INotifyPropertyChanged
    {
        public ICollection<LineStations> TflLines { get; init; }

        public StationNode? SelectedStation { get; set; }

        public string? Filter { get; set; } = "";

        private ObservableCollection<LineStations> _filteredTflLines;
        public ObservableCollection<LineStations> FilteredTflLines
        {
            get => _filteredTflLines;
            set
            {
                _filteredTflLines = value;
                OnPropertyChanged(nameof(FilteredTflLines));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<PlatformArrivals> Arrivals { get; set; }

        public readonly Dictionary<string, List<string>> StationLineLookup = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        public StationBrowsingVM()
        {
            TflLines = [.. LoadStationList()];
            Filter = "";
            FilteredTflLines = new ObservableCollection<LineStations>(TflLines);

            foreach (var line in TflLines)
            {
                foreach (var station in line.Stations)
                {
                    if (!StationLineLookup.ContainsKey(station.StationId))
                    {
                        StationLineLookup[station.StationId] = new List<string>();
                    }
                    StationLineLookup[station.StationId].Add(line.Line);
                }
            }

            Arrivals = [];
        }
        private static List<LineStations> LoadStationList()
        {
            var networkGraph = new NetworkGraph(MetaData.Lines);
            return networkGraph.BuildLineStations();
        }

        internal async Task HandleSelection(StationNode station, List<string> lines)
        {
            var arrivals = await PlatformArrivalsClient.GetArrivalsAsync(station.Station.MatchedStop.Select(s => s.Id).ToList(), lines);
            Arrivals.Clear();
            foreach (var arrival in arrivals)
            {
                Arrivals.Add(arrival);
            }
        }

    }
}
