using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace ArrivalsUI
{
    internal record OrderedStation(string Name, string Id, int Order, IEnumerable<string> predecessors, IEnumerable<string> successors);

    internal record LineStations(string Line, IEnumerable<OrderedStation> Stations);

    internal record PlatformArrivals(string PlatformName, ObservableCollection<Prediction> Predictions);
}
