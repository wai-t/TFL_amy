using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TflNetworkBuilder
{
    public class StationNodeDto
    {
        public required Station Station { get; init; }
        public List<string> NextIds { get; init; } = [];
        public List<string> PrevIds { get; init; } = [];
    }

    public static class Conversions
    {
        public static StationNodeDto ToDto(this StationNode node)
        {
            return new StationNodeDto
            {
                Station = node.Station,
                NextIds = node.Next.Select(n => n.StationId).ToList(),
                PrevIds = node.Prev.Select(p => p.StationId).ToList()
            };
        }

        public static List<StationNode> FromDtoList(this List<StationNodeDto> dto)
        {
            var ret = dto.Select(d =>
                new StationNode
                {
                    Station = d.Station
                }
            ).ToList();

            foreach (var d in dto)
            {
                var stationNode = ret.Single(n => n.StationId == d.Station.StationId);
                stationNode.Next = d.NextIds.Select(s => ret.Single(n=>n.StationId == s)).ToList();
                stationNode.Prev = d.PrevIds.Select(s => ret.Single(n => n.StationId == s)).ToList();
            }

            return ret;
        }
    }
}
