using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses.PipelineSteps
{
    class Voxel
    {
        public List<Vector3> Process(List<Vector3> originalPoints, double voxelSize)
        {
            int numPoints = originalPoints.Count;

            var voxelMap = new Dictionary<(int, int, int), List<int>>();

            for (int i = 0; i < numPoints; i++)
            {
                int voxelX = (int)Math.Floor(originalPoints[i].X / voxelSize);
                int voxelY = (int)Math.Floor(originalPoints[i].Y / voxelSize);
                int voxelZ = (int)Math.Floor(originalPoints[i].Z / voxelSize);
                var voxelKey = (voxelX, voxelY, voxelZ);

                if (!voxelMap.ContainsKey(voxelKey))
                    voxelMap[voxelKey] = new List<int>();

                voxelMap[voxelKey].Add(i);
            }

            var filteredPoints = new List<Vector3>();

            foreach (var indices in voxelMap.Values)
            {
                float sumX = 0, sumY = 0, sumZ = 0;
                int count = indices.Count;

                foreach (var i in indices)
                {
                    sumX += originalPoints[i].X;
                    sumY += originalPoints[i].Y;
                    sumZ += originalPoints[i].Z;
                }

                filteredPoints.Add(new Vector3(sumX, sumY, sumZ) / count);
            }
            return filteredPoints;
        }
    }
}
