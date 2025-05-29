using OxyPlot;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    internal class Clustering
    {
        public List<Cluster>? ClusteObjects(User user, double eps, int minPts, int minClustVol)
        {
            List<Cluster> clustersList = new();
            List<OxyColor> colors = [];
            List<Vector3> points = [];
            var clusters = DBSCAN.ComputeClusters(user.CurrentPcl.PointCloud, eps, minPts, minClustVol);
            foreach (var cloud in clusters.Values.ToList())
            {
                var (curCluster, curPclFeatures) = GetClusterFeatures(cloud);
                curCluster.Lims = curPclFeatures.PclLims;

                clustersList.Add(curCluster);
                colors.AddRange(curPclFeatures.Colors);
                points.AddRange(curPclFeatures.PointCloud);
            }
            user.CurrentPcl.PointCloud = points;
            user.CurrentPcl.Colors = colors;
            return clustersList.Count > 0 ? clustersList : null;

        }

        private (Cluster, PclFeatures) GetClusterFeatures(List<Vector3> points)
        {
            int count = points.Count;
            PclFeatures feature = new();
            feature.PointCloud = points;
            Random rand = new Random();
            var color = OxyPlot.OxyColor.FromRgb((byte)rand.Next(0, 255), (byte)rand.Next(0, 255), (byte)rand.Next(0, 255));
            feature.Colors = Enumerable.Repeat(color, count).ToList();
            Cluster clust = new();
            clust.Centroid = new(
                (feature.PclLims.xMin + feature.PclLims.xMax) / 2,
                (feature.PclLims.yMin + feature.PclLims.yMax) / 2,
                (feature.PclLims.zMin + feature.PclLims.zMax) / 2);

            clust.Size = new(
                feature.PclLims.xMax - feature.PclLims.xMin,
                feature.PclLims.yMax - feature.PclLims.yMin,
                feature.PclLims.zMax - feature.PclLims.zMin);
            return (clust, feature);
        }
    }
}
