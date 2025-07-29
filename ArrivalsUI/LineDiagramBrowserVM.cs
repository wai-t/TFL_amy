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
    public class LineDiagramBrowserVM : INotifyPropertyChanged, IGraphicsClient
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        const int GridSize = 12; // Size of 1 X,Y unit in pixels
        const int Left = 12;     // Leave some empty space on the left
        const int Top = 12;      // Leave some empty space on the right

        readonly List<Branch> _branches;
        readonly List<StationNode> _stations;
        public LineDiagramBrowserVM(string line)
        {
            _line = line;
            var branchesJson = File.ReadAllText($"Data/{_line}-BranchesList.json");
            _branches = JsonConvert.DeserializeObject<List<Branch>>(branchesJson)!.ToList();

            var stationsJson = File.ReadAllText($"Data/{_line}-StationNodeDtoList.json");
            var dtoList = JsonConvert.DeserializeObject<List<StationNodeDto>>(stationsJson)!;
            _stations = dtoList.FromDtoList();

            var geometry = new GeometryAnalyser(_branches, _stations);
            geometry.BuildConnectionDiagram(this);
        }

        // Interface implementation of IGraphicsClient methods
        // These are called by GeomentryAnalyser.BuildConnectionDiagram to tell us
        // where to draw a station marker, the station name, and a piece of railway line
        public void AddStopMarker(int rowNo, int colNo)
        {
            Stops.Add(new StopVM
            {
                X = colNo * GridSize + Left - 2,
                Y = rowNo * GridSize + Top - 2,
            });
        }
        public void AddStationName(int rowNo, int colNo, Station station)
        {
            StationNames.Add(new LabelVM
            {
                X = colNo * GridSize + Left,
                Y = rowNo * GridSize + Top - 8,
                Station = station, 
            });
        }

        public void AddTrackSection(int rowNo, int colNo, int targetColNo)
        {
            Connections.Add(new ConnectionVM
            {
                X0 = colNo * GridSize + Left,
                Y0 = (rowNo - 1) * GridSize + Top,
                X1 = targetColNo * GridSize +Left,
                Y1 = rowNo * GridSize + Top,
            });
        }

        public ObservableCollection<StopVM> Stops { get; set; } = [];
        public ObservableCollection<ConnectionVM> Connections { get; set; } = [];

        public ObservableCollection<LabelVM> StationNames { get; set; } = [];

        private string _line;
        public string Line { get => _line; set { _line = value; OnPropertyChanged(nameof(Line)); } }

    }

    public class StopVM : INotifyPropertyChanged
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; } = 5;

        public int Height { get; set; } = 5;

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class ConnectionVM : INotifyPropertyChanged
    {
        public int X0 { get; set; }
        public int X1 { get; set; }
        public int Y0 { get; set; }
        public int Y1 { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class LabelVM
    {
        public string Name => Station.MatchedStop.First().Name;

        public required Station Station { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }
}
