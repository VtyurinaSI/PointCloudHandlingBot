using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses.PipelineSteps
{
    internal class DBSCAN: IPipelineSteps
    {
        private readonly double eps;
        private readonly int minPts;
        private readonly int minClustVol;
        internal DBSCAN(double _eps, int _minPts, int _minClustVol)
        {
            eps = _eps;
            minPts = _minPts;
            minClustVol = _minClustVol;
        }
        

        private void ExpandCluster(List<Vector3> points, int[] labels, bool[] visited,
            int pointIdx, List<int> neighbors, int clusterId, double eps, int minPts, Dictionary<int, List<Vector3>> clusters)
        {
            var queue = new Queue<int>(neighbors);

            while (queue.Count > 0)
            {
                int idx = queue.Dequeue();
                if (!visited[idx])
                {
                    visited[idx] = true;
                    var newNeighbors = RangeQuery(points, idx, eps);
                    if (newNeighbors.Count >= minPts)
                    {
                        foreach (var nIdx in newNeighbors)
                            if (!queue.Contains(nIdx))
                                queue.Enqueue(nIdx);
                    }
                }

                if (labels[idx] == -9999 || labels[idx] == -1)
                {
                    labels[idx] = clusterId;
                    if (!clusters.ContainsKey(clusterId))
                        clusters[clusterId] = new List<Vector3>();
                    clusters[clusterId].Add(points[idx]);
                }
            }
        }

        private static List<int> RangeQuery(List<Vector3> points, int idx, double eps)
        {
            var result = new List<int>();
            Vector3 pi = points[idx];
            for (int i = 0; i < points.Count; i++)
                if (Vector3.Distance(pi, points[i]) <= eps)
                    result.Add(i);
            return result;
        }

        public UserPclFeatures Process(UserPclFeatures pcl)
        {
            int n = pcl.PointCloud.Count;
            int[] labels = new int[n];
            for (int i = 0; i < n; i++) labels[i] = -9999;

            int clusterId = 0;
            bool[] visited = new bool[n];
            var clusters = new Dictionary<int, List<Vector3>>();

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                visited[i] = true;

                var neighbors = RangeQuery(pcl.PointCloud, i, eps);

                if (neighbors.Count < minPts)
                {
                    labels[i] = -1;
                    if (!clusters.ContainsKey(-1))
                        clusters[-1] = new List<Vector3>();
                    clusters[-1].Add(pcl.PointCloud[i]);
                }
                else
                {
                    labels[i] = clusterId;
                    if (!clusters.ContainsKey(clusterId))
                        clusters[clusterId] = new List<Vector3>();
                    clusters[clusterId].Add(pcl.PointCloud[i]);
                    ExpandCluster(pcl.PointCloud, labels, visited, i, neighbors, clusterId, eps, minPts, clusters);
                    clusterId++;
                }
            }

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