using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfl_stats.Tfl;
using TflNetworkBuilder;

namespace TestTflNetworkBuilder
{
    [Collection("HttpClientFactory collection")]
    public class StationOrderTests
    {
        public StationOrderTests()
        {
        }

        //
        // first -> second
        //
        [Fact]
        public void TestTwoStationLine()
        {
            HashSet<StationNode> nodes = [];
            StationNode first = NewStation("first", nodes);
            AddStationsAfter(first, ["second"], nodes);

            var stationGraph = LineGraph.StationGraphFromTestData(nodes);

            var orderedStations = stationGraph.OrderedNodes.Select(s => s.StationId).ToList();

            Assert.Equal(orderedStations, ["START", "first", "second", "END"]);

            AssertOrder(stationGraph.OrderedNodes);
        }

        //
        // first -> fork -> l1 -> l2 -> l3  
        //                \-> r1 -> r2
        //
        [Fact]
        public void TestForks()
        {
            HashSet<StationNode> nodes = [];
            StationNode first = NewStation("first", nodes);
            var fork = AddStationsAfter(first, ["fork"], nodes).Last();

            // line splits into two
            var left = AddStationsAfter(fork, ["l1", "l2", "l3"], nodes).Last();
            var right = AddStationsAfter(fork, ["r1", "r2"], nodes).Last();


            var stationGraph = LineGraph.StationGraphFromTestData(nodes);

            var orderedStations = stationGraph.OrderedNodes.Select(s => s.StationId).ToList();

            //
            // The line forks and the algorithm chooses to append the shorter branch first.
            //
            Assert.Equal(orderedStations, ["START", "first", "fork", "r1", "r2", "l1", "l2", "l3", "END"]);

            AssertOrder(stationGraph.OrderedNodes);
        }

        //
        // first -> fork -> l1 -> l2 -> l3 -> ↓
        //                \-> r1 -> r2 ------>merge -> last
        //
        [Fact]
        public void TestForkThenMerge()
        {
            HashSet<StationNode> nodes = [];
            StationNode first = NewStation("first", nodes);
            var fork = AddStationsAfter(first, ["fork"], nodes).Last();

            // line splits into two
            var left = AddStationsAfter(fork, ["l1", "l2", "l3"], nodes).Last();
            var right = AddStationsAfter(fork, ["r1", "r2"], nodes).Last();

            // line rejoins at merge
            var merge = AddStationsAfter(left, ["merge"], nodes).Last();
            right.AddNext(merge);

            AddStationsAfter(merge, ["last"], nodes);

            var stationGraph = LineGraph.StationGraphFromTestData(nodes);

            var orderedStations = stationGraph.OrderedNodes.Select(s => s.StationId).ToList();

            //
            // When the line merges, the algorithm chooses to append the longer branch first.
            // This overrides whatever the earlier fork would have wanted
            //
            Assert.Equal(orderedStations, ["START", "first", "fork", "l1", "l2", "l3", "r1", "r2", "merge", "last","END"]);

            AssertOrder(stationGraph.OrderedNodes);
        }

        //
        //  l1 -> l2 -> l3 -> ↓
        //  r1 -> r2 ------>merge -> last
        //
        [Fact]
        public void TestMerge()
        {
            HashSet<StationNode> nodes = [];

            StationNode l1 = NewStation("l1", nodes);
            var left = AddStationsAfter(l1, ["l2", "l3", "l4"], nodes).Last();

            StationNode r1 = NewStation("r1", nodes);
            var right = AddStationsAfter(r1, ["r2"], nodes).Last();

            // line rejoins at merge
            var merge = AddStationsAfter(left, ["merge"], nodes).Last();
            right.AddNext(merge);

            AddStationsAfter(merge, ["last"], nodes);

            var stationGraph = LineGraph.StationGraphFromTestData(nodes);

            var orderedStations = stationGraph.OrderedNodes.Select(s => s.StationId).ToList();

            //
            // When the line merges, the algorithm chooses to append the longer branch first.
            // This overrides whatever the earlier fork would have wanted
            //
            Assert.Equal(orderedStations, ["START", "l1", "l2", "l3", "l4", "r1", "r2", "merge", "last", "END"]);

            AssertOrder(stationGraph.OrderedNodes);
        }


        //
        // l1 -> l2 -> l3 -> ↓
        // r1 -> r2 ------>merge -> a1 -> a2
        //                   ↓-> b1 -> b2
        [Fact]
        public void TestMergeAndForkOnTheSameStation()
        {
            HashSet<StationNode> nodes = [];
            var l1 = NewStation("l1", nodes);
            var left = AddStationsAfter(l1, ["l2", "l3"], nodes).Last();
            var r1 = NewStation("r1", nodes);
            var right = AddStationsAfter(r1, ["r2"], nodes).Last();

            // line rejoins at merge
            var merge = AddStationsAfter(left, ["merge"], nodes).Last();
            right.AddNext(merge);

            AddStationsAfter(merge, ["a1", "a2"], nodes);
            AddStationsAfter(merge, ["b1", "b2"], nodes);

            var stationGraph = LineGraph.StationGraphFromTestData(nodes);
            var orderedStations = stationGraph.OrderedNodes.Select(s => s.StationId).ToList();

            List<string> expected = ["START", "l1", "l2", "l3", "r1", "r2", "merge", "a1", "a2", "b1", "b2", "END"];
            Assert.Equal(expected, orderedStations);

            AssertOrder(stationGraph.OrderedNodes);
        }

        [Fact]
        public void TestTriangularGrid()
        {
            var nodes = new HashSet<StationNode>();

            List<StationNode> lastrow = [NewStation("c00", nodes)];
            for (var row = 1; row < 4; row++)
            {
                var l = NewStation($"c{row}0", nodes);
                var r = Enumerable.Range(0, row).Select(i => $"c{row}{i + 1}").ToList();
                List<StationNode> thisRow = [l, ..AddStationsAfter(l, r, nodes)];

                for (var col = 0; col < row; col++)
                {
                    lastrow[col].AddNext(thisRow[col]);
                }

                lastrow = thisRow;

            }

            var stationGraph = LineGraph.StationGraphFromTestData(nodes);

            var s = stationGraph.OrderedNodes.Select(s => s.StationId);

            List<string> expected = ["START","c00","c10","c20","c11","c21","c30","c31","c22","c32","c33","END"];

            Assert.Equal(expected, s);

            AssertOrder(stationGraph.OrderedNodes);
        }

        //
        // Unit test generated by ChatGPT
        //
        [Fact]
        public void TestMatrix()
        {
            HashSet<StationNode> nodes = [];

            StationNode first = NewStation("first", nodes);
            var fork = AddStationsAfter(first, ["fork"], nodes).Last();
            // line splits into two
            var left = AddStationsAfter(fork, ["l1", "l2", "l3"], nodes).Last();
            var right = AddStationsAfter(fork, ["r1", "r2"], nodes).Last();
            // line rejoins at merge
            var merge = AddStationsAfter(left, ["merge"], nodes).Last();
            right.AddNext(merge);
            AddStationsAfter(merge, ["last"], nodes);
            // add a second fork
            var secondFork = AddStationsAfter(merge, ["second-fork"], nodes).Last();
            var secondLeft = AddStationsAfter(secondFork, ["sl1", "sl2"], nodes).Last();
            var secondRight = AddStationsAfter(secondFork, ["sr1", "sr2"], nodes).Last();
            // add a third fork
            var thirdFork = AddStationsAfter(secondLeft, ["third-fork"], nodes).Last();
            var thirdLeft = AddStationsAfter(thirdFork, ["tl1", "tl2"], nodes).Last();
            var thirdRight = AddStationsAfter(thirdFork, ["tr1", "tr2"], nodes).Last();
            // add a fourth fork
            var fourthFork = AddStationsAfter(thirdLeft, ["fourth-fork"], nodes).Last();
            var fourthLeft = AddStationsAfter(fourthFork, ["fl1", "fl2"], nodes).Last();
            var fourthRight = AddStationsAfter(fourthFork, ["fr1", "fr2"], nodes).Last();
            // add a fifth fork
            var fifthFork = AddStationsAfter(fourthLeft, ["fifth-fork"], nodes).Last();
            var fifthLeft = AddStationsAfter(fifthFork, ["fifth-l1", "fifth-l2"], nodes).Last();
            var fifthRight = AddStationsAfter(fifthFork, ["fifth-r1", "fifth-r2"], nodes).Last();
            // add a sixth fork
            var sixthFork = AddStationsAfter(fifthLeft, ["sixth-fork"], nodes).Last();
            var sixthLeft = AddStationsAfter(sixthFork, ["sixth-l1", "sixth-l2"], nodes).Last();
            var sixthRight = AddStationsAfter(sixthFork, ["sixth-r1", "sixth-r2"], nodes).Last();

            var stationGraph = LineGraph.StationGraphFromTestData(nodes);

            var orderedStations = stationGraph.OrderedNodes;
            var s = orderedStations.Select(s => s.StationId).ToList();

            List<string> expected = ["START", "first",
                            "fork","l1","l2","l3","r1","r2","merge","last",
                            "second-fork","sr1","sr2","sl1","sl2",
                            "third-fork","tr1","tr2","tl1","tl2",
                            "fourth-fork","fr1","fr2","fl1","fl2",
                            "fifth-fork","fifth-r1","fifth-r2","fifth-l1","fifth-l2",
                            "sixth-fork","sixth-l1","sixth-l2","sixth-r1","sixth-r2","END"];
            Assert.Equal(expected, s);

            // But we can check that no station is above or below its predecessors or successors
            AssertOrder(orderedStations);

        }

        private static void AssertOrder(List<StationNode> orderedStations)
        {
            foreach (var station in orderedStations)
            {
                foreach (var next in station.Next)
                {
                    Assert.True(next.Station.Index > station.Station.Index);
                }

                foreach (var prev in station.Prev)
                {
                    Assert.True(prev.Station.Index < station.Station.Index);
                }
            }
        }

        private static StationNode NewStation(string stationName, HashSet<StationNode> nodes)
        {
            var n = new StationNode { Station = new() { StationId = stationName } };
            nodes.Add(n);
            return n;
        }

        //
        // Adds a line of one or more stations after the given node.
        //
        private List<StationNode> AddStationsAfter(StationNode node, List<string> names, HashSet<StationNode> nodes)
        {
            List<StationNode> addedStations = [];
            var currentNode = node;
            foreach (var name in names)
            {
                var nextNode = NewStation(name, nodes);
                addedStations.Add(nextNode);
                currentNode.AddNext(nextNode);
                currentNode = nextNode;
            }
            return addedStations;
        }

    }


}
