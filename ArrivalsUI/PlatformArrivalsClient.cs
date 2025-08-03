using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using tfl_stats.Tfl;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    public static class PlatformArrivalsClient
    {
        public static LineClient LineClient { get; } = new LineClient(new HttpClient());

        public static async Task<List<PlatformArrivals>> GetArrivalsAsync(List<string> stopPointIds, List<string> lines)
        {
            List<Prediction> predictions = [];
            foreach (var stopPointId in stopPointIds)
            {
                var ret = await LineClient.ArrivalsAsync(lines, stopPointId, null, null);
                predictions.AddRange(ret);
            }
            return UpdatePredictions(predictions);
        }
        private static List<PlatformArrivals> UpdatePredictions(ICollection<Prediction> predictions)
        {
            List<PlatformArrivals> arrivals = new List<PlatformArrivals>();

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
                arrivals.Add(new PlatformArrivals(platform.Key,
                    new ObservableCollection<Prediction>(platform.Value.OrderBy(p => p.TimeToStation))));
            }

            return arrivals;
        }
    }
}
