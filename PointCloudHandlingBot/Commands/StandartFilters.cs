using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal static class StandartFilters
    {
        internal static List<Vector3> Statistical(List<Vector3> points, List<double> parms)
        {
            int n = points.Count;
            var meanDistances = new float[n];

            for (int i = 0; i < n; i++)
            {
                var distances = new List<float>(n - 1);
                var p = points[i];
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    distances.Add(Vector3.Distance(p, points[j]));
                }
                distances.Sort();
                float mean = distances.Take((int)parms[0]).Average();
                meanDistances[i] = mean;
            }

            float globalMean = meanDistances.Average();
            float globalStd = (float)Math.Sqrt(meanDistances.Average(d => (d - globalMean) * (d - globalMean)));

            var filtered = new List<Vector3>();
            for (int i = 0; i < n; i++)
            {
                if (meanDistances[i] <= globalMean + parms[1] * globalStd)
                    filtered.Add(points[i]);
            }
            return filtered;
        }

        internal static List<Vector3> Median(List<Vector3> points, List<double> parms)
        {
            var result = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
            {
                var distances = new List<float>();
                for (int j = 0; j < points.Count; j++)
                {
                    if (i == j) continue;
                    distances.Add(Vector3.Distance(points[i], points[j]));
                }
                distances.Sort();
                float medianDist = distances[(int)parms[0]];

                if (medianDist < parms[1])
                    result.Add(points[i]);
            }
            return result;
        }
    }
}
