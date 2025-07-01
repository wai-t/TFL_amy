using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace ArrivalsUI
{
    internal static class ApiClient
    {
        public static LineClient LineClient { get; }  = new LineClient(new HttpClient());
    }
}
