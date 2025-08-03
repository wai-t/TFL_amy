using System.Collections.ObjectModel;
using tfl_stats.Tfl;

namespace ArrivalsUI
{
    public record PlatformArrivals(string PlatformName, ObservableCollection<Prediction> Predictions);
}
