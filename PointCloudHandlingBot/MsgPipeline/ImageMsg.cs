using OxyPlot;
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
        public ImageMsg(Func<User, PlotModel> setImg)
        {
            makeImg = setImg;
        }
        private readonly Func<User, PlotModel> makeImg;
        public async Task Send(ITelegramBotClient bot, User user)
        {
            using var ms = new MemoryStream();
            var model = makeImg(user);
            double h = user.CurrentPcl.PclLims.yMax - user.CurrentPcl.PclLims.yMin;
            double w = user.CurrentPcl.PclLims.xMax - user.CurrentPcl.PclLims.xMin;
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
            var exporter = new OxyPlot.SkiaSharp.PngExporter() { Height = hImage, Width = wImage };
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
