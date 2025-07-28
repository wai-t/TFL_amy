using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using TflNetworkBuilder;

namespace ArrivalsUI
{
    public class LineDiagramViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        int gridSize = 12;
        int left = 12;
        int top = 12;

        List<Branch> _branches;
        List<StationNode> _stations;
        public LineDiagramViewModel(string line, Action<Station>? stationSelectionHandler)
        {
            _stationSelectionHandler = stationSelectionHandler;

            _line = line;
            var branchesJson = File.ReadAllText($"Data/{_line}-BranchesList.json");
            _branches = JsonConvert.DeserializeObject<List<Branch>>(branchesJson)!.ToList();

            var stationsJson = File.ReadAllText($"Data/{_line}-StationNodeDtoList.json");
            var dtoList = JsonConvert.DeserializeObject<List<StationNodeDto>>(stationsJson)!;
            _stations = dtoList.FromDtoList();

            BuildConnectionDiagram();
        }

        private void BuildConnectionDiagram()
        {
            var geometry = new GeometryAnalyser(_branches);

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
                    incoming = [BuildEdge(station, geometry)];
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
                                outgoing.Add(BuildEdge(station, geometry));

                                // Output the graphic to mark the station on the correct slot
                                AddStopMarker(rowNo, thisStationSlot);
                            }

                            // tracks coming from START and 
                            // leading to END are fake, so are not visible.
                            if (!input.Hide)
                                AddTrackSection(rowNo, incomingSlot, thisStationSlot);
                        }
                        else
                        {
                            // tracks coming from START and 
                            // leading to END are fake, so are not visible.
                            if (!input.Hide && !(output.StationId == "END"))
                                AddTrackSection(rowNo, incomingSlot, outgoing.Count);
                            outgoing.Add(new EdgeData { Hide = input.Hide || output.StationId == "END", Targets = [output] });
                        }
                    }
                    incomingSlot++;
                }
                if (outgoing.Count > maxSlot) maxSlot = outgoing.Count;
                incoming = outgoing;
                rowNo++;
            }

            AddStationLabels(maxSlot);

        }

        private EdgeData BuildEdge(StationNode station, GeometryAnalyser geometry)
        {
            return new EdgeData
            {
                Hide = station.StationId == "START",
                // The Targets are ordered from left to right as seen from the line's main direction. So
                // a line that is mostly North-South will have the West-most fork on the left and the East-most
                // fork on the right. This is calculated using geometry.ComputeOffsetFromMedian()
                Targets = station.Next
                                .OrderByDescending(n =>
                                    n.StationId == "END" ? int.MaxValue : geometry.ComputeOffsetFromMedian(
                                    n.Station.MatchedStop.First())
                                ).ToList()
            };
        }
 
        private void AddStationLabels(int left)
        {
            int rowNo = 1;
            foreach (var station in _stations.Skip(1).SkipLast(1))
            {
                AddStationName(rowNo, left + 1, station.Station);
                rowNo++;
            }
        }

        private record EdgeData
        {
            public required bool Hide { get; set; }
            public required List<StationNode> Targets { get; set; } = [];
        }
        private void AddStationName(int rowNo, int colNo, Station station)
        {
            StationNames.Add(new Label
            {
                X = colNo * gridSize + left,
                Y = rowNo * gridSize + top - 8,
                Station = station, 
                OnStationSelect = _stationSelectionHandler
            });
        }

        private void AddTrackSection(int rowNo, int colNo, int targetColNo)
        {
            Connections.Add(new ConnectionViewModel
            {
                X0 = colNo * gridSize + left,
                Y0 = (rowNo - 1) * gridSize + top,
                X1 = targetColNo * gridSize +left,
                Y1 = rowNo * gridSize + top,
            });
        }

        private void AddStopMarker(int rowNo, int colNo)
        {
            Stops.Add(new StopViewModel
            {
                X = colNo * gridSize + left - 2,
                Y = rowNo * gridSize + top - 2,
            });
        }
        public ObservableCollection<StopViewModel> Stops { get; set; } = [];
        public ObservableCollection<ConnectionViewModel> Connections { get; set; } = [];

        public ObservableCollection<Label> StationNames { get; set; } = [];

        private Action<Station>? _stationSelectionHandler;
        private string _line;
        public string Line { get => _line; set { _line = value; OnPropertyChanged(nameof(Line)); } }

    }

    public class StopViewModel : INotifyPropertyChanged
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; } = 5;

        public int Height { get; set; } = 5;

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class ConnectionViewModel : INotifyPropertyChanged
    {
        public int X0 { get; set; }
        public int X1 { get; set; }
        public int Y0 { get; set; }
        public int Y1 { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class Label
    {
        public Action<Station>? OnStationSelect;
        public string Name => Station.MatchedStop.First().Name;


        public required Station Station { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }
}
