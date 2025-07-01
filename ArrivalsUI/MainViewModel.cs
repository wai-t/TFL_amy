using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using tfl_stats.Tfl;

namespace ArrivalsUI
{
    internal class MainViewModel
    {
        public ObservableCollection<LineStations> TflLines { get; init; }

        public OrderedStation? SelectedStation { get; set; }

        public ObservableCollection<PlatformArrivals> Arrivals { get; set; }

        public readonly Dictionary<string, string> StationLineLookup = new Dictionary<string, string>();

        public MainViewModel()
        {
            TflLines = [.. LoadStationList()];

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

    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value / 60;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
