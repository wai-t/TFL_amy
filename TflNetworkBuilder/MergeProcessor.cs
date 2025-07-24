using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TflNetworkBuilder
{
    // When Ordering, maintain a list of all merges that are
    // waiting to be completed. A merge is completed when all
    // of the branches feeding into it are complete. Before
    // that, it is pending
    //
    internal class MergeProcessor
    {
        private Dictionary<string, PendingMerge> _pendingMerges = [];

        public bool ProcessMerge(StationNode node, StationNode pred, ref List<StationNode> stationsOnBranch)
        {
            if (!_pendingMerges.TryGetValue(node.StationId, out var pendingJoin))
            {
                pendingJoin = new PendingMerge()
                {
                    StationAtMergeOfBranch = node,
                    MergingBranches = node.Prev.ToDictionary(n => n, n => new BranchStatus() { Complete = false, BranchNodes = [] })
                };
                _pendingMerges.Add(node.StationId, pendingJoin);
            }

            pendingJoin.MergingBranches[pred] = new BranchStatus()
            {
                Complete = true,
                BranchNodes = stationsOnBranch
            };

            if (pendingJoin.MergingBranches.Values.All(t => t.Complete))
            {
                var ret = pendingJoin.MergingBranches.Values
                   .OrderByDescending(t => t.BranchNodes.Count)
                   .SelectMany(t => t.BranchNodes).ToList();
                _pendingMerges.Remove(node.StationId);

                stationsOnBranch = ret;
                return true;
            }
            return false;
        }
    }
    internal class PendingMerge
    {
        public required StationNode StationAtMergeOfBranch;
        public required Dictionary<StationNode, BranchStatus> MergingBranches; // Key = Penultimate station on merging branch.
    }

    internal class BranchStatus
    {
        public bool Complete = false;               // Complete is set true whenever the merging branch is complete.
        public List<StationNode> BranchNodes = [];  // List of Stations on the merging branch. These will be collected
                                                    // together when the merge is complete.
    }

}
