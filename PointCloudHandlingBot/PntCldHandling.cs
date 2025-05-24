using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace PointCloudHandlingBot
{
    class PntCldHandling
    {
        internal (List<Vector3> points, List<Vector3> colors, PclLims lims) ReadPointCloud_ply(string[] lines)
        {
            var positions = new List<Vector3>();
            var colors = new List<Vector3>();
            PclLims lims = new();
            int isPrevEven = 1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var parts = lines[i].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;
                if (lines[i].Contains("end_header")) break;
                if (lines[i].Contains("ply")) continue;
                if (lines[i].Contains("format")) continue;
                if (lines[i].Contains("element")) continue;
                if (lines[i].Contains("property")) continue;
                if (isPrevEven == 1)
                {
                    float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
                    lims.UpdMin(x, ref lims.xMin);
                    lims.UpdMax(x, ref lims.xMax);
                    float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    lims.UpdMin(y, ref lims.yMin);
                    lims.UpdMax(y, ref lims.yMax);
                    float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    lims.UpdMin(z, ref lims.zMin);
                    lims.UpdMax(z, ref lims.zMax);

                    positions.Add(new Vector3(x, y, z));
                }
                else
                {
                    byte x = byte.Parse(parts[0], CultureInfo.InvariantCulture);
                    byte y = byte.Parse(parts[1], CultureInfo.InvariantCulture);
                    byte z = byte.Parse(parts[2], CultureInfo.InvariantCulture);

                    colors.Add(new Vector3(x, y, z));
                }
                isPrevEven = i % 2;

            }

            return (positions, colors, lims);
        }

        internal (List<Vector3>, PclLims) ReadPointCloud_txt(string[] lines)
        {
            var positions = new List<Vector3>();
            PclLims lims = new();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;

                float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
                lims.UpdMin(x, ref lims.xMin);
                lims.UpdMax(x, ref lims.xMax);

                float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
                lims.UpdMin(y, ref lims.yMin);
                lims.UpdMax(y, ref lims.yMax);

                float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
                lims.UpdMin(z, ref lims.zMin);
                lims.UpdMax(z, ref lims.zMax);

                positions.Add(new Vector3(x, y, z));
            }
            return (positions, lims);
        }
        public List<Vector3> VoxelFilter(List<Vector3> originalPoints, double voxelSize)
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
