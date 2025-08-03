using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace TflNetworkBuilder
{
    public class NetworkGraph
    {
        List<string> _lines;
        public NetworkGraph(List<string> lines)
        {
            _lines = lines;
        }

        public List<LineStations> BuildLineStations()
        {
            List<LineStations> ret = [];
            foreach (var line in _lines)
            {
                var json = File.ReadAllText($"Data/{line}-RouteSequence.json");
                var routeSequence = JsonConvert.DeserializeObject<RouteSequence>(json)!;
                var lineGraph = new LineGraph(routeSequence);
                var lineStations = new LineStations
                {
                    Line = line,
                    Stations = lineGraph.OrderedNodes.Where(n=>n.StationId != "START" && n.StationId != "END").ToList()
                };
                ret.Add(lineStations);
            }
            return ret;
        }
    }

    public class LineStations
    {
        public required string Line { get; set; }
        public required List<StationNode> Stations { get; set; }
    }
}
