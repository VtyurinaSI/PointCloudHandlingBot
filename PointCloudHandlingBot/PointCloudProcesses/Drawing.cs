using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    public static class Drawing
    {

        private static float GetScale(PclLims lims,
         int width = 800,
         int height = 600,
         int padding = 20)
        {
            float minX = lims.xMin;
            float maxX = lims.xMax;
            float minY = lims.yMin;
            float maxY = lims.yMax;

            float spanX = maxX - minX;
            float spanY = maxY - minY;
            if (spanX == 0) spanX = 1;
            if (spanY == 0) spanY = 1;

            float scaleX = (width - 2 * padding) / spanX;
            float scaleY = (height - 2 * padding) / spanY;
            return MathF.Min(scaleX, scaleY);
        }

        private static Image<Rgba32> SetEmptyImage(PclLims lims, float scale, int width = 800,
         int height = 600)
        {
            Image<Rgba32> image = new(width, height);
            var white = new Rgba32(255, 255, 255, 255);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    image[x, y] = white;
            return image;
        }
        /// <summary>
        /// Нарисовать изображение с использованием цветов из файла
        /// </summary>
        /// <param name="points">Облако точек</param>
        /// <param name="colors">Цвета точек</param>
        /// <param name="lims">Пределы координат</param>
        /// <param name="width">Ширина изображения</param>
        /// <param name="height">Высота изображение</param>
        /// <param name="padding">Отступы</param>
        /// <returns>Изображение</returns>
        public static Image<Rgba32> DrawProjection(
         IReadOnlyList<Vector3> points, IReadOnlyList<Rgba32> colors, PclLims lims,
         int width = 800,
         int height = 600,
         int padding = 20)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Нет точек для проекции", nameof(points));

            float scale = GetScale(lims);
            var image = SetEmptyImage(lims, scale);

            float minx = lims.xMin, miny = lims.yMin;

            int radius = 1;
            Rgba32 col = new(0, 0, 255, 255);

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                var c = colors[i];
                int px = (int)((p.X - minx) * scale + padding);
                int py = height - 1 - (int)((p.Y - miny) * scale + padding);
                if (px >= 0 && px < width && py >= 0 && py < height)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int nx = px + dx;
                            int ny = py + dy;
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                image[nx, ny] = c;
                        }
                }
            }
            return image;
        }
        internal static List<Rgba32> Coloring(List<Vector3> pcl, PclLims lims, Func<float, float, float, Rgba32> ColorMap)
        {
            int count = pcl.Count;
            Rgba32[] colors = new Rgba32[count];
            float min = lims.zMin, max = lims.zMax;

            for (int i = 0; i < count; i++)
            {
                colors[i] = ColorMap(pcl[i].Z, min, max);
            }

            return [.. colors];
        }


        internal static Rgba32 MapJet(float z, float minZ, float maxZ)
        {
            float t = (z - minZ) / (maxZ - minZ);
            t = Math.Clamp(t, 0, 1);

            float r = Math.Clamp(1.5f - MathF.Abs(4f * t - 3f), 0, 1);
            float g = Math.Clamp(1.5f - MathF.Abs(4f * t - 2f), 0, 1);
            float b = Math.Clamp(1.5f - MathF.Abs(4f * t - 1f), 0, 1);

            return new Rgba32(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255),
                255);
        }
        internal static Rgba32 MapPlasma(float z, float minZ, float maxZ)
        {
            float t = (z - minZ) / (maxZ - minZ);
            t = Math.Clamp(t, 0f, 1f);


            var c0 = new Rgba32(117, 11, 189, 255);
            var c1 = new Rgba32(184, 24, 188, 255);
            var c2 = new Rgba32(236, 28, 122, 255);
            var c3 = new Rgba32(251, 115, 105, 255);
            var c4 = new Rgba32(255, 235, 0, 255);

            static Rgba32 Lerp(Rgba32 a, Rgba32 b, float f)
            {
                byte R = (byte)(a.R + (b.R - a.R) * f);
                byte G = (byte)(a.G + (b.G - a.G) * f);
                byte B = (byte)(a.B + (b.B - a.B) * f);
                return new Rgba32(R, G, B, 255);
            }

            if (t < 0.25f)
                return Lerp(c0, c1, t / 0.25f);
            else if (t < 0.50f)
                return Lerp(c1, c2, (t - 0.25f) / 0.25f);
            else if (t < 0.75f)
                return Lerp(c2, c3, (t - 0.50f) / 0.25f);
            else
                return Lerp(c3, c4, (t - 0.75f) / 0.25f);
        }
        /// <summary>
        /// «Cool» palette: R = t, G = 1–t, B = 1
        /// </summary>
        internal static Rgba32 MapCool(float z, float minZ, float maxZ)
        {
            float t = (z - minZ) / (maxZ - minZ);
            t = Math.Clamp(t, 0f, 1f);

            float r = t;
            float g = 1f - t;
            float b = 1f;

            return new Rgba32(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255),
                255);
        }

        /// <summary>
        /// «Spring» palette: R = 1, G = t, B = 1–t
        /// </summary>
        internal static Rgba32 MapSpring(float z, float minZ, float maxZ)
        {
            float t = (z - minZ) / (maxZ - minZ);
            t = Math.Clamp(t, 0f, 1f);

            float r = 1f;
            float g = t;
            float b = 1f - t;

            return new Rgba32(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255),
                255);
        }
    }
}
