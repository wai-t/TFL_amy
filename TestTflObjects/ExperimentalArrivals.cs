using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;

namespace TestTflObjects
{
    //
    // the following classes represent the hierarchy of arrivals predictions returned by the 
    // GenerateArrivalPredictionsAsync method below
    //
    // A Line has many Stations, each Station has several Platforms, each Platform as a list of Arrivals.
    // The Arrival object represents a row on the display board on the Platform which
    // shows the next arriving vehicle, its destination, and time to arrive.
    //
    public record Arrival(
        string VehicleId,
        string Destination,
        int TimeToStation,
        DateTime PredictedArrivalTime,
        string Direction
    );

    public record PlatformArrivals(
        string Platform,
        List<Arrival> Predictions
    );

    public record StationArrivals(
        int StationOrder,
        string StationName,
        List<PlatformArrivals> Platforms
    );

    public record LineArrivals(
        string LineId,
        List<StationArrivals> Stations
    );

    //
    // The purpose of the OrderedStation class is to associate each Station on a Line with an order number
    // This allows us to show a list of Stations in a stable order in a UI rather than risking the order
    // changing each time it is refreshed.
    //
    internal class OrderedStation(string id, string name, int order, HashSet<string> predecessors, HashSet<string> successors)
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public int Order { get; set; } = order;
        public HashSet<string> Predecessors { get; } = predecessors;
        public HashSet<string> Successors { get; } = successors;
    };

    public static class ExperimentalArrivals
    {
        //
        // An algorithm to assign an order number to each Station on a Line.
        // 
        // Ideally, the order would resemble the order of the Stations on the line, but this isn't as
        // simple as it sounds, because the line isn't always linear, it has branches and the Circle line is a loop.
        //
        // Numbering system: every stop has an order number (integer) greater than the previous stop. We need to
        // ensure these variants:
        // - more than one stop follows the stop A: all following stops have a higher order number than stop A
        // - more than one stop precedes the stop A: all preceding stops have a lower order number than stop A
        // - order numbers don't have to be unique: stops on parallel routes can have the same order number
        //
        internal static async Task<IEnumerable<OrderedStation>> BuildIndexedStopsForLineAsync(string line, LineClient client)
        {
            // RouteSequenceAsync returns all the routes on a line, including inbound and outbound routes.
            // To simplify the process, we will assume that the inbound station order is the reverse of the outbound order.
            // So we will restrict the search to inbound routes only.
            //
            // There is a risk that this might not be true all the time, but it is a good starting point.
            //
            var routeSequences = (await client.RouteSequenceAsync(line, Direction.Inbound, [Anonymous6.Regular], null))
                                .StopPointSequences;

            Dictionary<string, OrderedStation> stopSet = [];

            // First, process all the routes (routeSequence) on the line.
            foreach (var routeSequence in routeSequences)
            {
                OrderedStation? precedingStation = null;

                // each routeSequence contains the list of StopPoints in the order they are visited,
                // so here we will accumulate all the StopPoints in the stopSet dictionary,
                // for each StopPoint we will keep track of which StopPoints precede and succeed it.
                foreach (var stop in routeSequence.StopPoint)
                {
                    if (!stopSet.TryGetValue(stop.Id, out var currentNode))
                    {
                        // Create a new node for the stop if it doesn't exist
                        currentNode = new OrderedStation(stop.Id, stop.Name, int.MinValue, new HashSet<string>(), new HashSet<string>());
                        stopSet[stop.Id] = currentNode;
                    }
                    if (precedingStation != null)
                    {
                        // Add the current stop as a successor of the preceding stop
                        precedingStation.Successors.Add(currentNode.Id);
                        currentNode.Predecessors.Add(precedingStation.Id);
                    }
                    precedingStation = currentNode;
                }
            }

            // 
            // stopSet now contains all the stops on the line, with their predecessors and successors.
            //

            //
            // We look for stops that have no predecessors, which means they are at the start of a branch, 
            // and call SetStopOrder which will recursively ensure the all successors will have a higher order
            // number than its predecessors.
            //
            foreach (var stop in stopSet.Values)
            {
                if (stop.Predecessors.Count == 0)
                {
                    SetStopOrder(stop, stopSet, 1, []);
                }
            }

            //
            // The Circle line is a loop, so there may not be a start of the line,
            // so we test for Stops that have no order number assigned yet (Order == int.MinValue)
            // and call SetStopOrder for them as well. We continue until all stops have an order number assigned.
            //
            foreach (var stop in stopSet.Values)
            {
                if (stop.Order == int.MinValue)
                {
                    SetStopOrder(stop, stopSet, 1, []);
                }
            }

            return stopSet.Values.AsEnumerable();
        }

        //
        // Recursive method to set the order number for a stop and all its successors.
        // It ensures that all successors have a higher order number than the stop itself.
        //
        private static void SetStopOrder(OrderedStation stop, Dictionary<string, OrderedStation> stopSet, int v, HashSet<string> visited)
        {
            visited.Add(stop.Id);
            stop.Order = v;
            foreach (var successorId in stop.Successors)
            {
                var successor = stopSet[successorId];
                //
                // if successor has not been assigned an order number yet (Order == int.MinValue) or
                // its order number is less than or equal to the current stop's order number,
                // assign it with v + 1. This ensures that all successors have a higher order number than the current stop.
                // To avoid infinite recursion, we check if the successor has already been visited, quitting if it has.
                if (successor.Order <= v && !visited.Contains(successor.Id))
                {
                    SetStopOrder(stopSet[successorId], stopSet, v + 1, visited);
                }
            }
        }


        internal static async Task<List<IEnumerable<LineArrivals>>> GenerateArrivalPredictionsAsync(List<string> lines, LineClient client)
        {
            List<Task<(string, IEnumerable<OrderedStation>)>> stopPointsTasks = [];

            foreach (var line in lines)
            {
                stopPointsTasks.Add(Task.Run(async () => (line, await ExperimentalArrivals.BuildIndexedStopsForLineAsync(line, client))));
            }

            var lineStations = await Task.WhenAll(stopPointsTasks);


            var predictionGroups = lines.Select(
                async l =>
                {

                    var indexedStops = lineStations
                        .FirstOrDefault(s => s.Item1 == l, (l, []))
                        .Item2.Select((sp, i) => new { i, sp.Id, sp.Name }).ToList();

                    // Get arrivals predictions for a specific line
                    var predictions = await client.ArrivalsAsync([l], "", null, null);

                    var indexedPredictions = predictions.Join(indexedStops, prediction => prediction.NaptanId, stop => stop.Id,
                        (prediction, stop) => new
                        {
                            LineId = prediction.LineId,
                            StationOrder = stop.i,
                            StationName = prediction.StationName,
                            Plaftorm = prediction.PlatformName,
                            Direction = prediction.Direction,
                            PredictedArrivalTime = (DateTime)prediction.ExpectedArrival?.UtcDateTime.ToLocalTime()!,
                            TimeToStation = (int)(prediction.TimeToStation!),
                            VehicleId = prediction.VehicleId,
                            Destination = prediction.DestinationName,
                        }).ToList();

                    //Nested groups by line, station order and direction, then order by TimeToStation descending
                    var groupedPredictions = indexedPredictions
                        .GroupBy(ip => ip.LineId)
                        .Select(l => new LineArrivals
                        (
                            LineId: l.Key,
                            Stations: [.. l.GroupBy(ip => ip.StationOrder)
                                .OrderBy(s => s.Key)
                                .Select(g => new StationArrivals
                                (
                                    StationOrder: g.Key,
                                    StationName: g.First().StationName,
                                    Platforms: [.. g.GroupBy(d => d.Plaftorm)
                                            .OrderBy(d => d.First().Direction)
                                            .Select(d => new PlatformArrivals
                                            (
                                                Platform: d.Key,
                                                Predictions: [.. d.OrderBy(p => p.TimeToStation)
                                                    .Select(p => new Arrival
                                                    (
                                                        p.VehicleId,
                                                        p.Destination,
                                                        p.TimeToStation,
                                                        p.PredictedArrivalTime,
                                                        p.Direction
                                                    ))]
                                            ))]
                                ))]
                        ));

                    return groupedPredictions;
                }
                );

            var groupedPredictions = (await Task.WhenAll(predictionGroups)).ToList();
            return groupedPredictions;
        }

    }
}