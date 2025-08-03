using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace TflNetworkBuilder
{
    public class MapBuilder
    {
        private List<string> _lines;
        private Dictionary<string, MatchedStop> _stopPoints = new();
        public MapBuilder(List<string> lines)
        {
            _lines = lines;
        }

        public void BuildMap(IMapClient client)
        {
            foreach (var line in _lines)
            {
                var json = File.ReadAllText($"Data/{line}-RouteSequence.json");
                var routeSequence = JsonConvert.DeserializeObject<RouteSequence>(json)!;
                foreach (var branch in routeSequence.StopPointSequences)
                {
                    //_sequences.Add(branch.StopPointSequence);
                    foreach (var stop in branch.StopPoint)
                    {
                        if (!_stopPoints.ContainsKey(stop.Id))
                        {
                            _stopPoints[stop.Id] = stop;
                        }
                        //if (!_stopPoints[stop.Id].Lines.Contains(line))
                        //{
                        //    _stopPoints[stop.Id].Lines.Add(line);
                        //}
                    }
                    var locations = branch.StopPoint.Select(sp => ((double)_stopPoints[sp.Id]!.Lat!, (double)_stopPoints[sp.Id].Lon!));

                    client.AddSequence(line, locations);
                }

                foreach (var sp in _stopPoints.Values)
                {
                    client.AddStopPoint(sp.Name, sp.Id, (double)sp.Lat!, (double)sp.Lon!, sp.Lines.Select(l => l.Id));
                }
            }
        }

        public List<MatchedStop> StopPoints => _stopPoints.Values.ToList();
    }

    public interface IMapClient
    {
        void AddStopPoint(string label, string id, double lat, double lon, IEnumerable<string> lines);

        public void AddSequence(string line, IEnumerable<(double, double)> points);
    }
}
