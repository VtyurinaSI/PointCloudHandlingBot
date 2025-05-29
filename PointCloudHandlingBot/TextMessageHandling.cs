using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using PointCloudHandlingBot.PointCloudProcesses.AnalyzePipelineSteps;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PointCloudHandlingBot
{

    class TextMessageHandling
    {
        private IAnalyzePipelineSteps step;
        public delegate Task KeyboardDelegate(ITelegramBotClient bot, long chatId);
        public event KeyboardDelegate? OpenAnalizeKeyboardEvent;
        public event KeyboardDelegate? OpenColorKeyboardEvent;
        public event KeyboardDelegate? OpenMainEvent;
        public delegate Task SendImageDelegate(User user);
        public event SendImageDelegate? SendImageEvent;
        public delegate Task SendTextDelegate(User user, string msg);
        public event SendTextDelegate? SendTextEvent;
        private const string hello = """
                            Привет! Я умею обрабатывать объемные облака точек!
                            Рисую в палитрах:
                            • Spring (по умолчанию)
                            • Jet
                            • Plasma                            
                            • Cool
                            Чтобы их применить, перед отображением напиши мне /colorMap<палитра>, например, /colorMapCool.
                            """;
        public readonly Keyboards keyboards = new();
        private void GoPipe(User user)
        {
            FileHandling file = new();
            user.Pipe.Condition = AnalyzePipeLine.PipeCondition.None;
            user.CurrentPcl ??= new();
            user.CurrentPcl.PointCloud = new(user.OrigPcl.PointCloud);
            user.Pipe.Execute(user.CurrentPcl);
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            user.Pipe = new();
            user.Pipe.Condition = AnalyzePipeLine.PipeCondition.None;
        }

        public void WhatDoYouWant(ITelegramBotClient botClient, User user, string textMsg)
        {

            string answer = string.Empty;
            FileHandling file = new();
            textMsg = textMsg.Trim();
            switch (user.Pipe.Condition)
            {
                case AnalyzePipeLine.PipeCondition.SettingStageType:

                    switch (textMsg)
                    {
                        case "gopipe":
                            GoPipe(user);
                            Task.Run(() =>
                                MsgPipeLine.SendAll(botClient, user,
                                    new ImageMsg(Drawing.Make3dImg),
                                    new KeyboardMsg(keyboards.MainMenu)
                                ));
                            break;
                        case "resetpipe":

                            user.Pipe = new();
                            answer = """
                        Составление порядка обработки отменено. 
                        Напоминаю, как выглядит твое сырое облако точек. 
                        Что теперь будем делать?
                        """;

                            Task.Run(() =>
                                MsgPipeLine.SendAll(botClient, user,
                                    new TextMsg(answer),
                                    new ImageMsg(Drawing.Make3dImg),
                                    new KeyboardMsg(keyboards.MainMenu)
                                ));
                            break;
                        default:
                            user.Pipe.StageName = textMsg;
                            answer = "Ок, теперь введи параметры";
                            Task.Run(() =>
                                MsgPipeLine.SendAll(botClient, user,
                                    new TextMsg(answer)
                                ));
                            user.Pipe.Condition = AnalyzePipeLine.PipeCondition.SettingStageParams;
                            break;
                    }

                    break;
                case AnalyzePipeLine.PipeCondition.SettingStageParams:
                    {
                        user.Pipe.Condition = AnalyzePipeLine.PipeCondition.SettingStageType;
                        user.Pipe.StageParams = textMsg.Replace('.', ',').Split(':')
                               .Select(d => double.Parse(d))
                               .ToList();
                        user.Pipe.CreateStep();
                        Task.Run(() =>
                                MsgPipeLine.SendAll(botClient, user,
                                    new KeyboardMsg(keyboards.Analyze)
                                ));
                    }
                    break;
                default:
                    switch (textMsg)
                    {
                        case "/start": answer = hello; break;
                        case string m when m.StartsWith("/colorMap"):
                            string colormap = m.Substring(9);
                            answer = SetColorMap(user, colormap);
                            if (user.OrigPcl.PointCloud is null) break;
                            var pcl = GetActualPcl(user);
                            pcl.Colors = Drawing.Coloring(pcl, user.ColorMap);
                            Task.Run(() =>
                            MsgPipeLine.SendAll(botClient, user,
                                new ImageMsg(Drawing.Make3dImg),
                                new KeyboardMsg(keyboards.MainMenu)
                            ));
                            break;
                        case "/analyze":
                            user.Pipe = new();
                            user.Pipe.Condition = AnalyzePipeLine.PipeCondition.SettingStageType;
                            OpenAnalizeKeyboardEvent?.Invoke(botClient, user.ChatId);
                            answer = "Пошли в анализ";
                            Task.Run(() =>
                            MsgPipeLine.SendAll(botClient, user,
                                new KeyboardMsg(keyboards.Analyze)
                            ));
                            break;
                        case "/setColor":
                            answer = "Пошли в покрас";
                            Task.Run(() =>
                            MsgPipeLine.SendAll(botClient, user,
                                new KeyboardMsg(keyboards.ColorMap)
                            ));
                            break;
                        default:

                            answer = "че :/";
                            Task.Run(() =>
                            MsgPipeLine.SendAll(botClient, user,
                                new TextMsg(answer)
                            ));
                            break;
                    }
                    break;
            }

        }

        private static UserPclFeatures GetActualPcl(User user)
        {
            UserPclFeatures pcl = user.OrigPcl;
            if (user.CurrentPcl is not null)
                pcl = user.CurrentPcl;
            return pcl;
        }
        private string SetColorMap(User user, string colormap)
        {
            string mapInfo = $"Теперь буду рисовать так";
            user.ColorMap = colormap switch
            {
                "Jet" => Drawing.MapJet,
                "Cool" => Drawing.MapCool,
                "Plasma" => Drawing.MapPlasma,
                "Spring" => Drawing.MapSpring,
                _ => Drawing.MapSpring,
            };
            SendTextEvent?.Invoke(user, mapInfo);
            return mapInfo;
        }
    }
}
