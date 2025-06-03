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
        public List<IMsgPipelineSteps> WhatDoYouWant(UserData user, string textMsg, Logging.Logger logger)
        {
            textMsg = textMsg.Trim();

            if (user.Command is not null)
                return HandleActiveCommand(user, textMsg, logger);

            return HandleNewCommand(user, textMsg, logger);
        }

        private List<IMsgPipelineSteps> HandleActiveCommand(UserData user, string textMsg, Logging.Logger logger)
        {
            if (!user.Command.IsInited)
            {
                string paramResult = user.Command.SetParseParts(textMsg);
                logger.LogBot("Ожидание параметра", LogLevel.Information, user, paramResult);

                if (user.Command.IsInited)
                {
                    var res = user.Command.Process(user);
                    user.Command = null;
                    return res;
                }
            }
            return [];
        }

        private List<IMsgPipelineSteps> HandleNewCommand(UserData user, string textMsg, Logging.Logger logger)
        {
            try
            {
                user.Command = CommandSimpleFactory.CreateCommand(textMsg, logger);
            }
            catch (ArgumentException ex)
            {
                logger.LogBot($"Ошибка создания команды: {ex.Message}", LogLevel.Error, user, textMsg);
                return [new TextMsg("че :/")];
            }

            if (user.Command.ParsePartsNum == 0)
            {
                var res = user.Command.Process(user);
                user.Command = null;
                return res;
            }

            if (textMsg != "/colorMap")
            {
                string? firstParDesc = user.Command.FirstParName;
                logger.LogBot($"Создана команда \"{textMsg}\". Ожидание параметра", LogLevel.Information, user, firstParDesc);
                return [];
            }
            else
            {
                return [
                    new ImageMsg(Drawing.Make3dImg),
                    new KeyboardMsg(Keyboards.ColorMap)
                ];
            }
        }
    }
}
