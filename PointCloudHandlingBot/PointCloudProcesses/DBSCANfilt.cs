using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    internal class DBSCANfilt //: IAnalyzePipelineSteps
    {
        private readonly double eps;
        private readonly int minPts;
        private readonly int minClustVol;
        internal DBSCANfilt(double _eps, int _minPts, int _minClustVol)
        {
            eps = _eps;
            minPts = _minPts;
            minClustVol = _minClustVol;
        }

        public PclFeatures Process(PclFeatures pcl)
        {
            var clusters = DBSCAN.ComputeClusters(pcl.PointCloud, eps, minPts, minClustVol);

            List<Vector3> result = [];
            clusters.Remove(-1);
            foreach (var key in clusters.Keys.ToList())
                if (clusters[key].Count > minClustVol)
                    foreach (var point in clusters[key])
                        result.Add(point);

            pcl.PointCloud = result;
            return pcl;
        }
    }
}