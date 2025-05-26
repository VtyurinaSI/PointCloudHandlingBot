using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    class PclReading
    {
        internal (List<Vector3> points, List<Rgba32> colors) ReadPointCloud_ply(string[] lines)
        {
            var positions = new List<Vector3>();
            var colors = new List<Rgba32>();
            int r = 0, g = 0, b = 0;
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
                if (i % 2 == 0)
                {
                    float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[2], CultureInfo.InvariantCulture);

                    positions.Add(new Vector3(x, y, z));
                }
                else
                {
                    try
                    {
                        r = int.Parse(parts[0], CultureInfo.InvariantCulture);
                        g = int.Parse(parts[1], CultureInfo.InvariantCulture);
                        b = int.Parse(parts[2], CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing color values: {ex.Message}");
                        continue;
                    }

                    colors.Add(new Rgba32((byte)r, (byte)g, (byte)b, 255));
                }
                //isPrevEven = i % 2;

            }

            return (positions, colors);
        }

        internal List<Vector3> ReadPointCloud_txt(string[] lines)
        {
            var positions = new List<Vector3>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;

                float x = float.Parse(parts[0], CultureInfo.InvariantCulture);

                float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[2], CultureInfo.InvariantCulture);

                positions.Add(new Vector3(x, y, z));
            }

            return positions;
        }




    }
}
