using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using tfl_stats.Tfl;
using Xunit;
using static System.Formats.Asn1.AsnWriter;

namespace TflNetworkBuilder
{
    public class GeometryAnalyser
    {
        List<Branch> _branches;
        List<StationNode> _stations;

        double _meanlat;
        double _meanlon;
        double _latvar;
        double _lonvar;
        double _covar;
        double _latcoeff;
        double _loncoeff;
        double _offsetScale;
        double _latscale;
        double _lonscale;

        //public GeometryAnalyser(List<Branch> branches, List<StationNode> stations) { 
        //    _branches = branches;
        //    _stations = stations;
        //    ComputeMajorAxis();
        //}

        public GeometryAnalyser(string line)
        {
            var json = File.ReadAllText($"Data/{line}-RouteSequence.json");
            var routeSequence = JsonConvert.DeserializeObject<RouteSequence>(json)!;

            var lineGraph = new LineGraph(routeSequence);

            _branches = lineGraph.Branches;
            _stations = lineGraph.OrderedNodes;

            ComputeMajorAxis();
        }

        //
        // By analysing the correlation of latitude and longitude of all the stations on a line, we
        // can work out the major direction of the line, i.e. does it mostly go EastWest or NorthSouth
        // or some other direction.
        private void ComputeMajorAxis()
        {
            double sumlat2 = 0, sumlon2 = 0, sumlatlon = 0, sumlat = 0, sumlon = 0 ;
            int n = 0;
            foreach (Branch br in _branches)
            {
                foreach (MatchedStop stop in br.StopPointSequence.StopPoint)
                {
                    sumlat += stop.Lat!.Value;
                    sumlon += stop.Lon!.Value;
                    sumlat2 += (stop.Lat!.Value) * (stop.Lat!.Value);
                    sumlon2 += (stop.Lon!.Value) * (stop.Lon!.Value);
                    sumlatlon += (stop.Lat!.Value) * (stop.Lon!.Value);
                    n++;
                }
            }

            var meanlat = sumlat2 / n;
            var meanlon = sumlon2 / n;
            var meanlatlon = sumlatlon / n;
            _meanlat = sumlat / n;
            _meanlon = sumlon / n;

            _latvar = (meanlat - _meanlat * _meanlat);
            _lonvar = (meanlon - _meanlon * _meanlon);
            _covar = (meanlatlon - _meanlat * _meanlon);

            double eigen1 = ((_latvar + _lonvar) + Math.Sqrt((_latvar + _lonvar) * (_latvar + _lonvar) - 4.0 * (_latvar * _lonvar - _covar *_covar)))/2.0;
            double eigen2 = ((_latvar + _lonvar) - Math.Sqrt((_latvar + _lonvar) * (_latvar + _lonvar) - 4.0 * (_latvar * _lonvar - _covar * _covar))) / 2.0;

            _offsetScale = 1.0 / eigen2;

            _latscale = 1.0 / Math.Sqrt(_latvar);
            _lonscale = 1.0 / Math.Sqrt(_lonvar);

            double AminusL = _latvar - eigen1;
            double BminusL = _lonvar - eigen1;

            double check = _covar * _covar - AminusL * BminusL;
            Assert.True(Math.Abs(check) < 1.0E-12, "Determinant check failed: " + check);
            
            if (Math.Abs(AminusL) > Math.Abs(BminusL))
            {
                _latcoeff = AminusL;
                _loncoeff = _covar;
            }
            else
            {
                _latcoeff = _covar;
                _loncoeff = BminusL;
            }
            var norm = Math.Sqrt(_latcoeff * _latcoeff + _loncoeff * _loncoeff);
            _latcoeff /= norm;
            _loncoeff /= norm;

        }

        private double ComputeOffsetFromMedian(MatchedStop stopPoint)
        {
            return (stopPoint.Lat!.Value - _meanlat) * _latcoeff + (stopPoint.Lon!.Value - _meanlon) * _loncoeff;
        }

        public void BuildLineDiagram(ILineDiagramClient graphicsOutput)
        {
            List<EdgeData>? incoming = null;

            int maxSlot = 0;
            int rowNo = 0;
            foreach (var station in _stations.SkipLast(1))
            {
                if (incoming == null)
                {
                    // the first station will be a START StationNode which is not
                    // a real station, but it links to the one or more Stations
                    // at the head of the track. So call BuildEdge to get
                    // the first set of tracks heading downstream
                    incoming = [BuildEdge(station)];
                }

                List<EdgeData> outgoing = [];

                // this will be the slot position of the current station at each iteration.
                // Use this to identify the slot position at which incoming tracks will
                // merge to a single slot.
                int thisStationSlot = -1;

                // Keep count of the current incoming slot
                int incomingSlot = 0;
                foreach (var input in incoming)
                {
                    foreach (var output in input.Targets)
                    {
                        // usually only one Target, unless the input came from a fork at the
                        // last station
                        if (output.StationId == station.StationId)
                        {
                            if (thisStationSlot == -1)
                            {
                                // Find the outgoing slot for the current station so
                                // that all lines merging will join at the correct slot
                                thisStationSlot = outgoing.Count;
                                //
                                // Any forks in the line are made at a station. So
                                // BuildEdge will make sure that the EdgeData will have one Target
                                // for each branch of a fork.
                                outgoing.Add(BuildEdge(station));

                                // Output the graphic to mark the station on the correct slot
                                graphicsOutput.AddStopMarker(rowNo, thisStationSlot);
                            }

                            // tracks coming from START and 
                            // leading to END are fake, so are not visible.
                            if (!input.Hide)
                                graphicsOutput.AddTrackSection(rowNo, incomingSlot, thisStationSlot);
                        }
                        else
                        {
                            // tracks coming from START and 
                            // leading to END are fake, so are not visible.
                            if (!input.Hide && !(output.StationId == "END"))
                                graphicsOutput.AddTrackSection(rowNo, incomingSlot, outgoing.Count);
                            outgoing.Add(new EdgeData { Hide = input.Hide || output.StationId == "END", Targets = [output] });
                        }
                    }
                    incomingSlot++;
                }
                if (outgoing.Count > maxSlot) maxSlot = outgoing.Count;
                incoming = outgoing;
                rowNo++;
            }

            AddStationLabels(maxSlot, graphicsOutput);

        }

        private EdgeData BuildEdge(StationNode station)
        {
            return new EdgeData
            {
                Hide = station.StationId == "START",
                // The Targets are ordered from left to right as seen from the line's main direction. So
                // a line that is mostly North-South will have the West-most fork on the left and the East-most
                // fork on the right. This is calculated using ComputeOffsetFromMedian()
                Targets = station.Next
                                .OrderByDescending(n =>
                                    n.StationId == "END" ? int.MaxValue : ComputeOffsetFromMedian(
                                    n.Station.MatchedStop.First())
                                ).ToList()
            };
        }

        private void AddStationLabels(int left, ILineDiagramClient graphicsOutput)
        {
            int rowNo = 1;
            foreach (var station in _stations.Skip(1).SkipLast(1))
            {
                graphicsOutput.AddStationName(rowNo, left + 1, station.Station);
                rowNo++;
            }
        }

        private record EdgeData
        {
            public required bool Hide { get; set; }
            public required List<StationNode> Targets { get; set; } = [];
        }
    }

    public interface ILineDiagramClient
    {
        void AddStationName(int rowNo, int colNo, Station station);
        void AddTrackSection(int rowNo, int colNo, int targetColNo);
        void AddStopMarker(int rowNo, int colNo);
    }
}
