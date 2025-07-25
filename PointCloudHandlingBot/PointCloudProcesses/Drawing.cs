﻿using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    public static class Drawing
    {
        public static (PngExporter, PlotModel) Make3dImg(UserData user)
        {
            var pcl = user.CurrentPcl;
            var lims = pcl.PclLims;

            var model = CreatePlotModel(user.FileName);
            AddAxes(model, lims);
            AddColorAxis(model, pcl.Colors);

            AddScatterSeries(model, pcl);

            if (pcl.Clusters is not null)
                AddClustersAnnotations(model, pcl.Clusters, lims);

            var (hImage, wImage) = CalculateImageSize(lims);

            var exporter = new PngExporter() { Height = hImage, Width = wImage };
            return (exporter, model);
        }

        private static PlotModel CreatePlotModel(string title)
        {
            return new PlotModel
            {
                Title = title,
                TitleFontSize = 24,
                SubtitleFontSize = 0,
                TextColor = OxyColors.Black
            };
        }

        private static void AddAxes(PlotModel model, PclLims lims)
        {
            double xMin = lims.xMin, xMax = lims.xMax;
            double yMin = lims.yMin, yMax = lims.yMax;
            double xRange = xMax - xMin;
            double yRange = yMax - yMin;
            int N = 10;
            double step = Math.Max(xRange, yRange) / N;
            double minor = step / 5;

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X",
                TitleFontSize = 18,
                FontSize = 16,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = xMin - xRange * 0.05,
                Maximum = xMax + xRange * 0.05,
                MajorStep = step,
                MinorStep = minor,
                MinimumPadding = 0.1,
                MaximumPadding = 0.1,
                StringFormat = "0.00"
            };
            model.Axes.Add(xAxis);

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Y",
                TitleFontSize = 18,
                FontSize = 16,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = yMin - yRange * 0.05,
                Maximum = yMax + yRange * 0.05,
                MajorStep = step,
                MinorStep = minor,
                MinimumPadding = 0.1,
                MaximumPadding = 0.1,
                StringFormat = "0.00"
            };
            model.Axes.Add(yAxis);
        }

        private static void AddColorAxis(PlotModel model, List<OxyColor>? colors)
        {
            if (colors is null) return;
            var colorAxis = new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Key = "pointColors",
                Palette = new OxyPalette(colors),
                Minimum = 0,
                Maximum = colors.Count - 1,
                HighColor = OxyColors.Undefined,
                LowColor = OxyColors.Undefined,
                IsAxisVisible = false
            };
            model.Axes.Add(colorAxis);
        }

        private static void AddScatterSeries(PlotModel model, PclFeatures pcl)
        {
            if (pcl.PointCloud is null || pcl.Colors is null) return;
            var scatter = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                ColorAxisKey = "pointColors"
            };
            for (int i = 0; i < pcl.PointCloud.Count; i++)
                scatter.Points.Add(new ScatterPoint(pcl.PointCloud[i].X, pcl.PointCloud[i].Y, scatter.MarkerSize, value: i));
            model.Series.Add(scatter);
        }

        private static void AddClustersAnnotations(PlotModel model, List<Cluster> clusters, PclLims lims)
        {
            double yRange = lims.yMax - lims.yMin;
            foreach (var cl in clusters)
            {
                var rect = new RectangleAnnotation
                {
                    MinimumX = cl.Lims.xMin,
                    MaximumX = cl.Lims.xMax,
                    MinimumY = cl.Lims.yMin,
                    MaximumY = cl.Lims.yMax,
                    Stroke = OxyColors.Black,
                    StrokeThickness = 1,
                    Fill = OxyColors.Undefined
                };
                model.Annotations.Add(rect);

                var pt = new PointAnnotation
                {
                    X = cl.Centroid.X,
                    Y = cl.Centroid.Y,
                    Shape = MarkerType.Circle,
                    Size = 6,
                    Fill = OxyColors.Red,
                    Stroke = OxyColors.Black,
                    StrokeThickness = 1
                };
                model.Annotations.Add(pt);

                var label = new TextAnnotation
                {
                    Text = $"Size: {cl.Size.X:0.00}x{cl.Size.Y:0.00}x{cl.Size.Z:0.00}",
                    TextPosition = new DataPoint(cl.Lims.xMin, cl.Lims.yMin - yRange * 0.008),
                    FontSize = 14,
                    TextVerticalAlignment = VerticalAlignment.Top,
                    TextHorizontalAlignment = HorizontalAlignment.Left,
                    Stroke = OxyColors.Undefined,
                    Background = OxyColor.FromAColor(a: 200, OxyColors.White),
                    Padding = new OxyThickness(2)
                };
                model.Annotations.Add(label);
            }
        }

        private static (int hImage, int wImage) CalculateImageSize(PclLims lims)
        {
            double h = lims.yMax - lims.yMin;
            double w = lims.xMax - lims.xMin;
            int hImage = 0, wImage = 0;
            if (w > h)
            {
                wImage = 900;
                double koeff = h / w * 1.1;
                hImage = (int)(wImage * koeff);
            }
            else
            {
                hImage = 900;
                double koeff = w / h * 1.1;
                wImage = (int)(hImage / koeff);
            }
            return (hImage, wImage);
        }

        internal static List<OxyColor> Coloring(PclFeatures pcl, Func<float, float, float, OxyColor> ColorMap)
        {
            int count = pcl.PointCloud.Count;
            OxyColor[] colors = new OxyColor[count];
            float min = pcl.PclLims.zMin, max = pcl.PclLims.zMax;

            for (int i = 0; i < count; i++)
            {
                colors[i] = ColorMap(pcl.PointCloud[i].Z, min, max);
            }

            return [.. colors];
        }

        /// <summary>
        /// «Cool» palette: R = t, G = 1–t, B = 1
        /// </summary>
        internal static OxyColor MapCool(float z, float minZ, float maxZ)
        {
            float t = (z - minZ) / (maxZ - minZ);
            t = Math.Clamp(t, 0f, 1f);

            float r = t;
            float g = 1f - t;
            float b = 1f;

            return OxyColor.FromRgb(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255));
        }

        internal static OxyColor MapJet(float z, float minZ, float maxZ)
        {
            float t = (z - minZ) / (maxZ - minZ);
            t = Math.Clamp(t, 0, 1);

            float r = Math.Clamp(1.5f - MathF.Abs(4f * t - 3f), 0, 1);
            float g = Math.Clamp(1.5f - MathF.Abs(4f * t - 2f), 0, 1);
            float b = Math.Clamp(1.5f - MathF.Abs(4f * t - 1f), 0, 1);

            return OxyColor.FromRgb(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255));
        }

        internal static OxyColor MapPlasma(float z, float minZ, float maxZ)
        {
            float t = (z - minZ) / (maxZ - minZ);
            t = Math.Clamp(t, 0f, 1f);

            var c0 = OxyColor.FromRgb(117, 11, 189);
            var c1 = OxyColor.FromRgb(184, 24, 188);
            var c2 = OxyColor.FromRgb(236, 28, 122);
            var c3 = OxyColor.FromRgb(251, 115, 105);
            var c4 = OxyColor.FromRgb(255, 235, 0);

            static OxyColor Lerp(OxyColor a, OxyColor b, float f)
            {
                byte R = (byte)(a.R + (b.R - a.R) * f);
                byte G = (byte)(a.G + (b.G - a.G) * f);
                byte B = (byte)(a.B + (b.B - a.B) * f);
                return OxyColor.FromRgb(R, G, B);
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
        /// «Spring» palette: R = 1, G = t, B = 1–t
        /// </summary>
        internal static OxyColor MapSpring(float z, float minZ, float maxZ)
        {
            float t = (z - minZ) / (maxZ - minZ);
            t = Math.Clamp(t, 0f, 1f);

            float r = 1f;
            float g = t;
            float b = 1f - t;

            return OxyColor.FromRgb(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255));
        }

        private static float GetScale(PclLims lims,
         int width = 1920,
         int height = 1080,
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

    }
}
