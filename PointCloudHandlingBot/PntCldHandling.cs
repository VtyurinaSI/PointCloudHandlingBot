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
        internal (IReadOnlyList<Vector3> points, IReadOnlyList<Vector3> colors, PclLims lims) ReadPointCloud_ply(string[] lines)
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



    }
}
