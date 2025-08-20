using TflNetworkBuilder;
namespace LineDiagramFileGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var line = "central";
            var geometry = new GeometryAnalyser(line);
            //var fileWriter = new JsonFileWriter(line);
            var fileWriter = new JsxFileWriter(line);
            geometry.BuildLineDiagram(fileWriter);
            fileWriter.EndWriting();
        }
    }

    class JsonFileWriter : ILineDiagramClient, IDisposable
    {
        private string _line;
        private StreamWriter _fileStream;
        private bool disposedValue;

        private List<(int rowNo, int colNo, string name, string id)> _stationNames = [];
        private List<(int rowNo, int colNo)> _stopMarkers = [];
        private List<(int rowNo, int colNo, int targetColNo)> _trackSections = [];
        public JsonFileWriter(string line)
        {
            _line = line;
            _fileStream = new StreamWriter(_line + ".json");
            _fileStream.WriteLine("{");
            _fileStream.WriteLine($"\"Line\": \"{line}\",");
        }
        public void AddStationName(int rowNo, int colNo, Station station)
        {
            _stationNames.Add((rowNo, colNo, station.MatchedStop.First().Name, station.MatchedStop.First().Id));
        }

        public void AddStopMarker(int rowNo, int colNo)
        {
            //throw new NotImplementedException();
        }

        public void AddTrackSection(int rowNo, int colNo, int targetColNo)
        {
            //throw new NotImplementedException();
        }

        public void EndWriting()
        {
            // output the station names, markers, and tracksections

            //_fileStream.WriteLine("\"Stops\" : [");
            //_fileStream.WriteLine("\"Connections\" : [");
            
            _fileStream.WriteLine("\"StationNames\" : [");
            foreach (var station in _stationNames)
            {
                _fileStream.WriteLine("{");
                _fileStream.WriteLine($"\"Name\": \"{station.name}\",");
                _fileStream.WriteLine($"\"Id\": \"{station.id}\",");
                _fileStream.WriteLine($"\"X\": {station.colNo}\",");
                _fileStream.WriteLine($"\"Y\": {station.rowNo}\"");
                _fileStream.WriteLine("},");
            }
            _fileStream.WriteLine("], ");

            _fileStream.WriteLine("}");
            _fileStream.Close();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~JsonFileWriter()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /**
     *                 <Station
                    key={i}
                    x={s.X}
                    y={s.Y}
                    name={data.StationNames?.[i]?.Name}
                    markerProps={{
                        r: 3, 
                        fill: "#fff",
                        stroke: "#111",
                        strokeWidth: 1.2
                    }}
                    labelOffset={{ dx: 10, dy: 0 }}
                    labelProps={{
                        fontSize: 8, 
                        dominantBaseline: "middle",
                        textAnchor: "start" 
                    }}
                />
                    <LineSegment
                    key={i}
                    x1={c.X0}
                    y1={c.Y0}
                    x2={c.X1}
                    y2={c.Y1}
                    stroke="#E32017"         
                    strokeWidth={3}
                    strokeLinecap="round"
                />
     * */
    class JsxFileWriter : ILineDiagramClient, IDisposable
    {
        private string _line;
        private StreamWriter _fileStream;
        private bool disposedValue;

        private int _trackSectionCounter = 0;

        public JsxFileWriter(string line)
        {
            _line = line;
            _fileStream = new StreamWriter(_line + ".jsx");
            _fileStream.WriteLine("export content = [ ");
        }
        public void AddStationName(int rowNo, int colNo, Station station)
        {
            
        }

        public void AddStopMarker(int rowNo, int colNo)
        {
            //throw new NotImplementedException();
        }

        public void AddTrackSection(int rowNo, int colNo, int targetColNo)
        {
            
            _fileStream.WriteLine("< LineSegment");
            _fileStream.WriteLine($"key ={_trackSectionCounter++}");

            _fileStream.WriteLine($"x1 ={colNo}");
            _fileStream.WriteLine($"y1 ={rowNo}");
            _fileStream.WriteLine($"x2 ={targetColNo}");
            _fileStream.WriteLine($"y2 ={rowNo + 1}");
            _fileStream.WriteLine("stroke = \"#E32017\"");
            _fileStream.WriteLine("strokeWidth ={ 3}");
            _fileStream.WriteLine("strokeLinecap = \"round\"");
            _fileStream.WriteLine("/>");

            _fileStream.WriteLine(", ");
        }

        public void EndWriting()
        {
            _fileStream.WriteLine("]");
            _fileStream.Close();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~JsonFileWriter()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
