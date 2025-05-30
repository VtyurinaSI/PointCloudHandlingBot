using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.Commands;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using SixLabors.ImageSharp;
using System.Globalization;

namespace PointCloudHandlingBot
{

    class TextMessageHandling
    {
        public List<IMsgPipelineSteps> WhatDoYouWant(User user, string textMsg, Logger logger)
        {
            List<IMsgPipelineSteps> res = [];
            textMsg = textMsg.Trim();
            if (user.Command is not null)
            {
                if (!user.Command.IsInited)
                {
                    logger.LogBot("Ожидание параметра", LogLevel.Information, user,
                        user.Command.SetParseParts(textMsg));
                    if (user.Command.IsInited)
                    {
                        res = user.Command.Process(user);
                        user.Command = null;
                        return res;
                    }
                }
            }
            else
            {
                try
                {
                    user.Command = CommandSimpleFactory.CreateCommand(textMsg, logger);
                    if (user.Command.ParsePartsNum == 0)
                    {
                        res = user.Command.Process(user);
                        user.Command = null;
                        return res;
                    }
                }
                catch (ArgumentException ex)
                {
                    logger.LogBot($"Ошибка создания команды: {ex.Message}", LogLevel.Error, user, textMsg);
                    res = [new TextMsg("че :/")];
                    return res;
                }
                if (textMsg != "/colorMap")
                {
                    string? firstParDesc = user.Command.FirstParName;
                    logger.LogBot($"Создана команда \"{textMsg}\". Ожидание параметра", LogLevel.Information, user,
                        firstParDesc);
                }
                else
                    res = [
                            new ImageMsg(Drawing.Make3dImg),
                            new KeyboardMsg(Keyboards.ColorMap)
                        ];
                return res;

            }
            return res;

        }
    }
}
