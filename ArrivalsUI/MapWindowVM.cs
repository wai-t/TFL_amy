using MapControl;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using tfl_stats.Tfl;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    public class MapWindowVM
    {
        private List<string> _lines = [
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
        HashSet<StationLink> _links = [];

        List<StopPointSequence> _sequences = [];
        Dictionary<string, StopPointItem> _stopPoints = [];
        public MapWindowVM()
        {

            foreach (var line in _lines)
            {
                var links = File.ReadAllText($"Data/{line}-StationLinks.json");
                var linkList = JsonConvert.DeserializeObject<List<StationLink>>(links)!;
                foreach (var link in linkList)
                {
                    // Add the link to the set, which will ensure uniqueness (by line)
                    _links.Add(link);
                }

                var branches = File.ReadAllText($"Data/{line}-BranchesList.json");
                var branchList = JsonConvert.DeserializeObject<List<Branch>>(branches)!;
                foreach (var branch in branchList)
                {
                    _sequences.Add(branch.StopPointSequence);
                    foreach (var stop in branch.StopPointSequence.StopPoint)
                    {
                        if (!_stopPoints.ContainsKey(stop.Id))
                        {
                            _stopPoints[stop.Id] = new StopPointItem
                            {
                                Id = stop.Id,
                                Name = stop.Name,
                                Location = new Location(stop.Lat!.Value, stop.Lon!.Value)
                            };
                        }
                        if (!_stopPoints[stop.Id].Lines.Contains(line))
                        {
                            _stopPoints[stop.Id].Lines.Add(line);
                        }
                    }
                    PolylineItems.Add(new PolylineItem
                    {
                        Line = line,
                        Locations = new LocationCollection(branch.StopPointSequence.StopPoint.Select(sp => _stopPoints[sp.Id].Location))
                    });
                }
            }
        }
        public async void HandleStationSelection(string stopPointId, List<string> lines)
        {
            var predictions = await ApiClient.LineClient.ArrivalsAsync(lines, stopPointId, null, null);
            UpdatePredictions(predictions);
        }
        internal void UpdatePredictions(ICollection<Prediction> predictions)
        {
            Arrivals.Clear();
            Dictionary<string, IList<Prediction>> platformArrivals = [];

            foreach (var prediction in predictions)
            {
                var key = $"{prediction.LineId.ToUpper()}-{prediction.PlatformName}";
                if (!platformArrivals.TryGetValue(key, out var platformList))
                {
                    platformList = new List<Prediction>();
                    platformArrivals[key] = platformList;
                }
                platformList.Add(prediction);
            }

            foreach (var platform in platformArrivals)
            {
                Arrivals.Add(new PlatformArrivals(platform.Key,
                    new ObservableCollection<Prediction>(platform.Value.OrderBy(p => p.TimeToStation))));
            }
        }

        public Dictionary<string, List<StationLink>> Lines => _links.GroupBy(l => l.LineId)
            .ToDictionary(g => g.Key, g => g.ToList());


        public ObservableCollection<PolylineItem> PolylineItems { get; set; } = [];

        public List<StopPointItem> StopPoints => _stopPoints.Values.ToList();

        public ObservableCollection<PlatformArrivals> Arrivals { get; set; } = [];

    }

    public class PolylineItem
    {
        public required string Line { get; set; }

        public SolidColorBrush Brush => LineColours.LineBrush(Line);
        public required LocationCollection Locations { get; set; }
    }

    public class StopPointItem
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public List<string> Lines { get; } = [];
        public required Location Location { get; set; }

        public string Label => Name + "\n" + string.Join("\n", Lines);
    }
}
