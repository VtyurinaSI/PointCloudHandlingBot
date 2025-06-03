using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    public static class RANSACPlate
    {
        public class PlaneModel
        {
            public Vector3 Normal;
            public float D;
        }
        public static List<Vector3> RemoveWall(
        List<Vector3> cloud,
        int maxIterations = 1000,
        float distanceThreshold = 0.02f,
        float horizontalToleranceAngleDeg = 15f)
        {
            var model = RansacFitPlane(cloud, maxIterations, distanceThreshold, out var inliers);
            if (model == null || inliers.Count == 0) return cloud;

            float angle = AngleToAxis(model.Normal, Vector3.UnitZ);
            if (angle <= horizontalToleranceAngleDeg)
                return FilterOutIndices(cloud, inliers);
            return cloud;
        }

        /// <summary>
        /// Убирает стену (вертикальную плоскость).
        /// </summary>
        public static List<Vector3> RemoveFloor(
            List<Vector3> cloud,
            int maxIterations = 1000,
            float distanceThreshold = 0.02f,
            float verticalToleranceAngleDeg = 15f)
        {
            var model = RansacFitPlane(cloud, maxIterations, distanceThreshold, out var inliers);
            if (model == null || inliers.Count == 0)
                return cloud;

            float angle = AngleToAxis(model.Normal, Vector3.UnitZ);

            if (MathF.Abs(angle - 90f) <= verticalToleranceAngleDeg)
                return FilterOutIndices(cloud, inliers);
            return cloud;
        }

        private static PlaneModel RansacFitPlane(
            List<Vector3> cloud,
            int maxIterations,
            float distanceThreshold,
            out List<int> bestInliers)
        {
            var rand = new Random();
            bestInliers = new List<int>();
            PlaneModel bestModel = null;

            for (int iter = 0; iter < maxIterations; iter++)
            {
                int i1 = rand.Next(cloud.Count),
                    i2 = rand.Next(cloud.Count),
                    i3 = rand.Next(cloud.Count);
                if (i1 == i2 || i1 == i3 || i2 == i3) { iter--; continue; }

                var p1 = cloud[i1];
                var p2 = cloud[i2];
                var p3 = cloud[i3];

                var v1 = p2 - p1;
                var v2 = p3 - p1;
                var normal = Vector3.Cross(v1, v2);
                if (normal.LengthSquared() < 1e-6f) continue;

                normal = Vector3.Normalize(normal);
                float d = -Vector3.Dot(normal, p1);
                var model = new PlaneModel { Normal = normal, D = d };

                var inliers = new List<int>();
                for (int i = 0; i < cloud.Count; i++)
                {
                    float dist = MathF.Abs(Vector3.Dot(normal, cloud[i]) + d);
                    if (dist <= distanceThreshold)
                        inliers.Add(i);
                }

                if (inliers.Count > bestInliers.Count)
                {
                    bestInliers = inliers;
                    bestModel = model;
                }
            }

            return bestModel;
        }

        private static List<Vector3> FilterOutIndices(List<Vector3> cloud, List<int> inliers)
        {
            var set = new HashSet<int>(inliers);
            return cloud
                .Where((pt, idx) => !set.Contains(idx))
                .ToList();
        }

        private static float AngleToAxis(Vector3 normal, Vector3 axis)
        {
            float cos = MathF.Abs(Vector3.Dot(Vector3.Normalize(normal), axis));
            return MathF.Acos(cos) * (180f / MathF.PI);
        }
    }
}