using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace ArrivalsUI
{
    internal class StationBrowsingVM : INotifyPropertyChanged
    {
        public ICollection<LineStations> TflLines { get; init; }

        public OrderedStation? SelectedStation { get; set; }

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

        public readonly Dictionary<string, string> StationLineLookup = new Dictionary<string, string>();

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
                    StationLineLookup[station.Id] = line.Line;
                }
            }

            Arrivals = [];
        }
        private static List<LineStations> LoadStationList()
        {
            var stationsData = File.ReadAllText("IndexedStops.json");

            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<LineStations>>(stationsData)!;

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
}
