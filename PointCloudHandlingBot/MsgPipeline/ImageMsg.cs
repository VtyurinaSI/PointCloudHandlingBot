using OxyPlot;
using OxyPlot.SkiaSharp;
using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PointCloudHandlingBot.MsgPipeline
{
    internal class ImageMsg : IMsgPipelineSteps
    {
        public ImageMsg(Func<UserData, (PngExporter, PlotModel)> setImg)
        {
            makeExporter = setImg;
        }
        private readonly Func<UserData, (PngExporter, PlotModel)> makeExporter;
        public async Task Send(ITelegramBotClient bot, UserData user)
        {
            using var ms = new MemoryStream();
            var (exporter, model) = makeExporter(user);
            
            exporter.Export(model, ms);
            ms.Seek(0, SeekOrigin.Begin);
            await bot.SendPhoto(
                chatId: user.ChatId,
                photo: InputFile.FromStream(ms, fileName: "projection.png"),
                caption: "Вот ваш график"
            );
        }
    }
}
