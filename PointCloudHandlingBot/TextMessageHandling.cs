using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.Commands;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using SixLabors.ImageSharp;

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
        public readonly Keyboards keyboards = new();


        public List<IMsgPipelineSteps> WhatDoYouWant(User user, string textMsg, Logger logger)
        {

            textMsg = textMsg.Trim();
            if (user.Command is not null)
            {
                if (!user.Command.IsInited)
                {
                    logger.LogBot("Ожидание параметра", LogLevel.Information, user,
                        user.Command.SetParseParts(textMsg));
                    if (user.Command.IsInited)
                    {
                        user.Command.Process(user);
                        return
                            [
                                new ImageMsg(Drawing.Make3dImg),
                                new KeyboardMsg(keyboards.MainMenu)
                            ];
                    }
                }
            }
            else
            {
                user.Command = CommandSimpleFactory.CreateCommand(textMsg, logger, keyboards);
                logger.LogBot($"Создана команда \"{textMsg}\". Ожидание параметра", LogLevel.Information, user,
                    user.Command.FirstParName);
            }
            return [];

            //    user.Pipe.Condition switch
            //{
            //    AnalyzePipeLine.PipeCondition.SettingStageType => HandleSettingStageType(user, textMsg),
            //    AnalyzePipeLine.PipeCondition.SettingStageParams => HandleSettingStageParams(user, textMsg),
            //    _ => HandleDefault(user, textMsg)
            //};
        }

        private List<IMsgPipelineSteps> HandleSettingStageType(User user, string textMsg)
        {
            return textMsg switch
            {
                "gopipe" => HandleGoPipe(user),
                "resetpipe" => HandleResetPipe(user),
                _ => HandleSetStageName(user, textMsg)
            };
        }

        private List<IMsgPipelineSteps> HandleGoPipe(User user)
        {
            FileHandling file = new();

            user.CurrentPcl ??= new();
            user.CurrentPcl.PointCloud = new(user.OrigPcl.PointCloud);
            user.Pipe.Execute(user.CurrentPcl);
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            user.Pipe = new();
            user.Pipe.Condition = AnalyzePipeLine.PipeCondition.None;
            return
            [
                new ImageMsg(Drawing.Make3dImg),
                new KeyboardMsg(keyboards.MainMenu)
            ];
        }

        private List<IMsgPipelineSteps> HandleResetPipe(User user)
        {
            user.Pipe = new();
            string answer = """
        Составление порядка обработки отменено. 
        Напоминаю, как выглядит твое сырое облако точек. 
        Что теперь будем делать?
        """;
            return
            [
                new TextMsg(answer),
                new ImageMsg(Drawing.Make3dImg),
                new KeyboardMsg(keyboards.MainMenu)
            ];
        }

        private List<IMsgPipelineSteps> HandleSetStageName(User user, string textMsg)
        {
            user.Pipe.StageName = textMsg;
            user.Pipe.Condition = AnalyzePipeLine.PipeCondition.SettingStageParams;
            return [new TextMsg("Ок, теперь введи параметры")];
        }

        private List<IMsgPipelineSteps> HandleSettingStageParams(User user, string textMsg)
        {
            user.Pipe.Condition = AnalyzePipeLine.PipeCondition.SettingStageType;
            user.Pipe.StageParams = textMsg.Replace('.', ',').Split(':')
                .Select(d => double.Parse(d))
                .ToList();
            user.Pipe.CreateStep();
            return [new KeyboardMsg(keyboards.Analyze)];
        }

        private List<IMsgPipelineSteps> HandleDefault(User user, string textMsg)
        {
            switch (textMsg)
            {
                case "/start":
                    return [new TextMsg(hello)];

                case "/analyze":
                    user.Pipe = new();
                    user.Pipe.Condition = AnalyzePipeLine.PipeCondition.SettingStageType;
                    return [new KeyboardMsg(keyboards.Analyze)];

                case "/setColor":
                    return [new KeyboardMsg(keyboards.ColorMap)];

                case string c when c.StartsWith("/cluster"):
                    var parm = textMsg.Substring(8).Replace('.', ',').Split(':')
                .Select(d => double.Parse(d))
                .ToList();
                    ObjectsClustering(user, parm[0], (int)parm[1], (int)parm[2]);
                    return [new ImageMsg(Drawing.Make3dImg),
                        new KeyboardMsg(keyboards.MainMenu)];

                case string s when s.StartsWith("/colorMap"):
                    return ApplyColorMap(user, textMsg);

                default:
                    return [new TextMsg("че :/")];
            }
        }

        private void ObjectsClustering(User user, double eps, int minPts, int minClustVol)
        {
            Clustering clustering = new();
            user.CurrentPcl.Clusters = clustering.ClusteObjects(user, eps, minPts, minClustVol);
        }

        private List<IMsgPipelineSteps> ApplyColorMap(User user, string textMsg)
        {
            string colormap = textMsg.Substring(9);
            SetColorMap(user, colormap);

            if (user.OrigPcl.PointCloud is not null)
            {
                var pcl = GetActualPcl(user);
                pcl.Colors = Drawing.Coloring(pcl, user.ColorMap);
                return [
                    new TextMsg("Теперь буду рисовать так:"),
                    new ImageMsg(Drawing.Make3dImg),
                    new KeyboardMsg(keyboards.MainMenu)];
            }
            else
                return [new TextMsg($"Теперь буду рисовать в {colormap}")];

        }
        private static PclFeatures GetActualPcl(User user)
        {
            PclFeatures pcl = user.OrigPcl;
            if (user.CurrentPcl is not null)
                pcl = user.CurrentPcl;
            return pcl;
        }
        private void SetColorMap(User user, string colormap)
        {
            user.ColorMap = colormap switch
            {
                "Jet" => Drawing.MapJet,
                "Cool" => Drawing.MapCool,
                "Plasma" => Drawing.MapPlasma,
                "Spring" => Drawing.MapSpring,
                _ => Drawing.MapSpring,
            };
        }
    }
}
