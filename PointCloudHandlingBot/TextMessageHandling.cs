using PointCloudHandlingBot.PointCloudProcesses;
using PointCloudHandlingBot.PointCloudProcesses.PipelineSteps;
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
        private IPipelineSteps step;
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
        private void GoPipe(User user)
        {
            FileHandling file = new();
            user.Pipe.Condition = PipeLine.PipeCondition.None;
            user.CurrentPcl ??= new();
            user.CurrentPcl.PointCloud = new(user.OrigPcl.PointCloud);
            user.Pipe.Execute(user.CurrentPcl);
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            user.Pipe = new();
            user.Pipe.Condition = PipeLine.PipeCondition.None;
            SendImageEvent?.Invoke(user);
            
        }

        public string WhatDoYouWant(ITelegramBotClient botClient, User user, string textMsg)
        {

            string answer = string.Empty;
            FileHandling file = new();
            textMsg = textMsg.Trim();
            if (user.Pipe.Condition == PipeLine.PipeCondition.SettingStageType)
            {
                switch (textMsg)
                {
                    case "gopipe": 
                        GoPipe(user);
                        OpenMainEvent?.Invoke(botClient,user.ChatId); 
                        break;
                    case "resetpipe":

                        user.Pipe = new();
                        answer = """
                        Составление порядка обработки отменено. 
                        Напоминаю, как выглядит твое сырое облако точек. 
                        Что теперь будем делать?
                        """;
                        SendTextEvent?.Invoke(user, answer);
                        SendImageEvent?.Invoke(user);                        
                        OpenMainEvent?.Invoke(botClient, user.ChatId);
                        break;
                    default:
                        user.Pipe.StageName = textMsg;
                        answer = "Ок, теперь введи параметры";
                        SendTextEvent?.Invoke(user, answer);
                        user.Pipe.Condition = PipeLine.PipeCondition.SettingStageParams;
                        break;
                }
            }
            else
            {
                if (user.Pipe.Condition == PipeLine.PipeCondition.SettingStageParams)
                {
                    user.Pipe.Condition = PipeLine.PipeCondition.SettingStageType;
                    user.Pipe.StageParams = textMsg.Replace('.', ',').Split(':')
                           .Select(d => double.Parse(d))
                           .ToList();
                    user.Pipe.CreateStep();
                    OpenAnalizeKeyboardEvent?.Invoke(botClient, user.ChatId);
                }
                else
                    switch (textMsg)
                    {
                        case "/start": answer = hello; break;
                        case string m when m.StartsWith("/colorMap"):
                            string colormap = m.Substring(9);
                            answer = SetColorMap(user, colormap);
                            if (user.OrigPcl.PointCloud is null) break;
                            var pcl = GetActualPcl(user);
                            pcl.Colors = Drawing.Coloring(pcl, user.ColorMap);

                            SendImageEvent?.Invoke(user);
                            OpenMainEvent?.Invoke(botClient, user.ChatId);
                            break;
                        case "/analyze":
                            user.Pipe = new();
                            user.Pipe.Condition = PipeLine.PipeCondition.SettingStageType;
                            OpenAnalizeKeyboardEvent?.Invoke(botClient, user.ChatId);
                            answer = "Пошли в анализ";
                            break;
                        case "/setColor":
                            OpenColorKeyboardEvent?.Invoke(botClient, user.ChatId);
                            answer = "Пошли в покрас";
                            break;
                        default:

                            answer = "че :/";
                            SendTextEvent?.Invoke(user, answer);
                            break;
                    }
            }
            return answer;
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
            SendTextEvent?.Invoke(user, mapInfo);
            return mapInfo;
        }
    }
}
