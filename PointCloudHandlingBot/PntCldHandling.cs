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

        internal (List<Vector3>, PclLims) ReadPointCloud(string[] lines)
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

        

    }
}
