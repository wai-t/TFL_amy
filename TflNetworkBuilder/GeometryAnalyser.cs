using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using tfl_stats.Tfl;
using Xunit;

namespace TflNetworkBuilder
{
    public class GeometryAnalyser
    {
        List<Branch> _branches;
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

        public GeometryAnalyser(List<Branch> branches) { 
            _branches = branches;
            ComputeMajorAxis();
            //ComputeBranchTurns();
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

        //public void ComputeBranchTurns()
        //{
        //    List<(Branch, double)> branches = [];
        //    foreach (Branch branch in _branches)
        //    {
        //        var offset = ComputeBranchOffset(branch);
        //        branches.Add((branch, offset));
        //    }
        //    _orderedBranches = branches.OrderByDescending(p => p.Item2).Select((p,i) => (i,p.Item1)).ToList();
        //}
        //public double ComputeBranchOffset(Branch branch)
        //{
        //    var firstStop = branch.First.Id;
        //    var lastStop = branch.Last.Id;
        //    var sumOffset = 0.0;
        //    var n = 0;
        //    foreach (var stopPoint in branch.StopPointSequence.StopPoint)
        //    {
        //        var offset = ComputeOffsetFromMedian(stopPoint);
        //        sumOffset += offset;
        //        n++;
        //    }
        //    var meanOffset = sumOffset / n;
        //    return meanOffset * _offsetScale;
        //}

        public double ComputeOffsetFromMedian(MatchedStop stopPoint)
        {
            return (stopPoint.Lat!.Value - _meanlat) * _latcoeff + (stopPoint.Lon!.Value - _meanlon) * _loncoeff;
        }
    }
}
