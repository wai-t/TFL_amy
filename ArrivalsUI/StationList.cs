using System.Collections.ObjectModel;
using tfl_stats.Tfl;

namespace ArrivalsUI
{
    public record OrderedStation(string Name, string Id, int Order, IEnumerable<string> predecessors, IEnumerable<string> successors);

    public record LineStations(string Line, IEnumerable<OrderedStation> Stations);

    public record PlatformArrivals(string PlatformName, ObservableCollection<Prediction> Predictions);
}
