using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot
{
    static internal class DBSCAN
    {
        private static void ExpandCluster(List<Vector3> points, int[] labels, bool[] visited,
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
        public static Dictionary<int, List<Vector3>> ComputeClusters(List<Vector3> pcl,double eps, int minPts,int minClustVol)
        {
            int n = pcl.Count;
            int[] labels = new int[n];
            for (int i = 0; i < n; i++) labels[i] = -9999;

            int clusterId = 0;
            bool[] visited = new bool[n];
            var clusters = new Dictionary<int, List<Vector3>>();

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                visited[i] = true;

                var neighbors = RangeQuery(pcl, i, eps);

                if (neighbors.Count < minPts)
                {
                    labels[i] = -1;
                    if (!clusters.ContainsKey(-1))
                        clusters[-1] = new List<Vector3>();
                    clusters[-1].Add(pcl[i]);
                }
                else
                {
                    labels[i] = clusterId;
                    if (!clusters.ContainsKey(clusterId))
                        clusters[clusterId] = new List<Vector3>();
                    clusters[clusterId].Add(pcl[i]);
                    ExpandCluster(pcl, labels, visited, i, neighbors, clusterId, eps, minPts, clusters);
                    clusterId++;
                }
            }
            return clusters;
        }
    }
}