using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ArrivalsUI
{
    public static class MetaData
    {
        public static List<string> Lines => LinesAndColours.Keys.ToList();
        public static Dictionary<string, Color> LinesAndColours => new()
        {
            { "bakerloo", Color.FromRgb(179, 99, 5) },
            { "central", Color.FromRgb(227, 32, 23) },
            { "circle", Color.FromRgb(255, 211, 0) },
            { "district", Color.FromRgb(0, 120, 42) },
            { "hammersmith-city", Color.FromRgb(243, 169, 187) },
            { "jubilee", Color.FromRgb(160, 165, 169) },
            { "metropolitan", Color.FromRgb(155, 0, 86) },
            { "northern", Color.FromRgb(0, 0, 0) },
            { "piccadilly", Color.FromRgb(0, 54, 136) },
            { "victoria", Color.FromRgb(0, 152, 212) },
            { "waterloo-city", Color.FromRgb(149, 205, 186) },
            { "dlr", Color.FromRgb(0, 164, 167) },
            { "elizabeth", Color.FromRgb(113, 86, 165) }
        };
    }
}
