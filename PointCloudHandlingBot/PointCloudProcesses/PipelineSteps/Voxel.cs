using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses.PipelineSteps
{
    class Voxel : IPipelineSteps
    {
        private double voxelSize;
        internal Voxel(double _voxelSize)
        {
            voxelSize=_voxelSize;
        }
        public UserPclFeatures Process(UserPclFeatures pcl)
        {
            
            int numPoints = pcl.PointCloud.Count;

            var voxelMap = new Dictionary<(int, int, int), List<int>>();

            for (int i = 0; i < numPoints; i++)
            {
                int voxelX = (int)Math.Floor(pcl.PointCloud[i].X / voxelSize);
                int voxelY = (int)Math.Floor(pcl.PointCloud[i].Y / voxelSize);
                int voxelZ = (int)Math.Floor(pcl.PointCloud[i].Z / voxelSize);
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
                    sumX += pcl.PointCloud[i].X;
                    sumY += pcl.PointCloud[i].Y;
                    sumZ += pcl.PointCloud[i].Z;
                }

                filteredPoints.Add(new Vector3(sumX, sumY, sumZ) / count);
            }
            pcl.PointCloud = filteredPoints;
            return pcl;
        }

    }
}
