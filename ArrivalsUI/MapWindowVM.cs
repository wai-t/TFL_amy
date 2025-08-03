using MapControl;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    public class MapWindowVM : IMapClient
    {

        Dictionary<string, StopPointItem> _stopPoints = [];
        Dictionary<string, List<string>> _stationStopPoints = [];
        public MapWindowVM()
        {
            var mapBuilder = new MapBuilder(MetaData.Lines);
            mapBuilder.BuildMap(this);

            foreach (var stopPoint in mapBuilder.StopPoints)
            {
                var stationId = stopPoint.ParentId ?? stopPoint.Id;
                if (!_stationStopPoints.ContainsKey(stationId))
                {
                    _stationStopPoints[stationId] = [];
                }
                _stationStopPoints[stationId].Add(stopPoint.Id);
            }
        }


        public async Task HandleStationSelection(StopPointItem stopPointVM)
        {
            var stationId = _stationStopPoints.Where(kvp => kvp.Value.Contains(stopPointVM.Id)).Select(kvp => kvp.Key).FirstOrDefault()!;
            var arrivals = await PlatformArrivalsClient.GetArrivalsAsync(_stationStopPoints[stationId], stopPointVM.Lines);
            Arrivals.Clear();
            foreach (var arrival in arrivals)
            {
                Arrivals.Add(arrival);
            }
        }

        public void AddStopPoint(string label, string id, double lat, double lon, IEnumerable<string> lines)
        {
            if (!_stopPoints.ContainsKey(id))
            {
                // If the stop point does not exist, create a new one
                _stopPoints[id] = new StopPointItem
                {
                    Id = id,
                    Name = label,
                    Lat = lat,
                    Lon = lon,
                    Lines = lines.ToList()
                };
            }
            else if (lines.Except(_stopPoints[id].Lines).Any())
            {
                // should never happen
                _stopPoints[id].Lines.AddRange(lines.Except(_stopPoints[id].Lines));
                throw new Exception($"Stop point {id} already exists but with different lines. Existing: {string.Join(", ", _stopPoints[id].Lines)}, New: {string.Join(", ", lines)}");
            }
        }

        public void AddSequence(string line, IEnumerable<(double, double)> points)
        {
            var locations = points.Select(p => new Location(p.Item1, p.Item2)).ToList();
            Sequences.Add(new SequenceItem
            {
                Line = line,
                Locations = new LocationCollection(locations)
            });
        }

        public ObservableCollection<SequenceItem> Sequences { get; set; } = [];

        public List<StopPointItem> StopPoints => _stopPoints.Values.ToList();

        public ObservableCollection<PlatformArrivals> Arrivals { get; set; } = [];

    }

    public class SequenceItem
    {
        public required string Line { get; set; }

        public SolidColorBrush Brush => new SolidColorBrush(MetaData.LinesAndColours[Line]);
        public required LocationCollection Locations { get; set; }
    }

    public class StopPointItem
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public List<string> Lines { get; set; } = [];

        public double Lat { get; set; }
        public double Lon { get; set; }

        public Location Location => new(Lat, Lon);
        public string Label => Name + "\n" + string.Join("\n", Lines);
    }
}
