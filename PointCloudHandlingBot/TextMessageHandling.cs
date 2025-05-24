using PointCloudHandlingBot.PointCloudProcesses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PointCloudHandlingBot
{
    class TextMessageHandling
    {
        private const string hello = """
                            Привет! Я умею обрабатывать объемные облака точек!
                            Рисую в палитрах:
                            • Spring (по умолчанию)
                            • Jet
                            • Plasma
                            • Cool

                            Чтобы их применить, перед отображением напиши мне /colorMap<палитра>, например, /colorMapCool.
                            """;

        public static (string?, Image<Rgba32>?) WhatDoYouWant(User user, string textMsg)
        {
            (string? text, Image<Rgba32>? img) answer = (null, null);
            switch (textMsg)
            {
                case "/start": answer = (hello, null); break;

                case string v when v.StartsWith("/voxel"):
                    string rest = v.Substring(6).Replace('.', ',');
                    if (double.TryParse(rest, out double voxelSize))
                    {
                        var (newPoints, newColors) = MakeVoxel(user, voxelSize);
                        FileHandling file = new();
                        answer = file.MakeResultPcl(user, newPoints, newColors);
                    }
                    else answer = ("Не смог распарсить(", null);
                    break;

                case string m when m.StartsWith("/colorMap"):
                    string colormap = m.Substring(9);
                    answer.text = SetColorMap(user, colormap);
                    if (user.PointCloud is not null)
                    {
                        user.Colors = Drawing.Coloring(user.PointCloud, user.PclLims, user.ColorMap);
                        FileHandling file = new();
                        answer = file.MakeResultPcl(user, user.PointCloud, user.Colors);
                    }

                    break;

                default:
                    answer = ("че :/", null);
                    break;
            }
            return answer;
        }
        private static string SetColorMap(User user, string colormap)
        {
            string mapInfo = $"Ок, теперь буду рисовать палитрой {colormap}";
            switch (colormap)
            {
                case "Jet": user.ColorMap = Drawing.MapJet; break;
                case "Cool": user.ColorMap = Drawing.MapCool; break;
                case "Plasma": user.ColorMap = Drawing.MapPlasma; break;
                case "Spring": user.ColorMap = Drawing.MapSpring; break;
                default:
                    user.ColorMap = Drawing.MapSpring;
                    mapInfo = $"Не знаю, что за {colormap}, будет Spring";
                    break;
            }
            return mapInfo;
        }
        private static (List<Vector3>, List<Rgba32>) MakeVoxel(User user, double voxelSize)
        {
            PclProcess pclProc = new();
            List<Vector3> voxeled = pclProc.VoxelFilter(user.PointCloud, voxelSize);
            List<Rgba32> newColors = Drawing.Coloring(voxeled, user.PclLims, user.ColorMap);
            return (voxeled, newColors);
        }
    }
}
