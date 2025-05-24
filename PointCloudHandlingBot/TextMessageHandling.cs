using PointCloudHandlingBot.PointCloudProcesses;
using PointCloudHandlingBot.PointCloudProcesses.PipelineSteps;
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
        private static UserPclFeatures GetActualPcl(User user)
        {
            UserPclFeatures pcl = user.OrigPcl;
            if (user.CurrentPcl is not null) 
                pcl = user.CurrentPcl;
            return pcl;
        }
        public static (string?, Image<Rgba32>?) WhatDoYouWant(User user, string textMsg)
        {
            (string? text, Image<Rgba32>? img) answer = (null, null);
            FileHandling file = new();
            //UserPclFeatures pcl;
            switch (textMsg)
            {
                case "/start": answer = (hello, null); break;

                case string v when v.StartsWith("/voxel"):
                    string rest = v.Substring(6).Replace('.', ',');

                    if (double.TryParse(rest, out double voxelSize))
                    {
                        var actPcl = GetActualPcl(user);
                        MakeVoxel(user, actPcl, voxelSize);

                        answer = file.MakeResultPcl(user.ChatId, user.CurrentPcl);
                    }
                    else answer = ("Не смог распарсить(", null);
                    break;

                case string m when m.StartsWith("/colorMap"):
                    string colormap = m.Substring(9);
                    answer.text = SetColorMap(user, colormap);
                    if (user.OrigPcl.PointCloud is null) break;
                    var pcl = GetActualPcl(user);
                    pcl.Colors = Drawing.Coloring(pcl, user.ColorMap);

                    answer = file.MakeResultPcl(user.ChatId, pcl);


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
        private static void MakeVoxel(User user, UserPclFeatures pcl, double voxelSize)
        {
            Voxel voxel = new();

            user.CurrentPcl ??= new();
            user.CurrentPcl.PointCloud = voxel.Process(pcl.PointCloud, voxelSize);
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
        }
    }
}
