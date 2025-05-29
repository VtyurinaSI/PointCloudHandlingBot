using OxyPlot;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Axes;
using OxyPlot.ImageSharp;
using OxyPlot.Series;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static PointCloudHandlingBot.Program;
using static System.Net.Mime.MediaTypeNames;
namespace PointCloudHandlingBot
{

    class UpdateHandler : IUpdateHandler
    {
        public UpdateHandler()
        {
            text = new();
            text.OpenAnalizeKeyboardEvent += OpenPipelineKeyboard;
            text.OpenColorKeyboardEvent += OpenColorKeyboard;
            text.SendImageDelegateEvent += SendPlot;
            file = new();
        }
        private readonly FileHandling file;
        private async Task SendLogToBot(ITelegramBotClient bot, CancellationToken token, long id, string msg)
        {
            await bot.SendMessage(id,
                                msg,
                                cancellationToken: token);
        }

        public delegate Task MessageHandler(User user, string msg);
        public event MessageHandler? OnHandleUpdateCompleted;

        public event MessageHandler? OnHandleUpdateStarted;
        public async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine($"Ошибка: {exception.Message}\n{exception.StackTrace}"), cancellationToken);
        }
        private TextMessageHandling text;
        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            string reply = string.Empty;


            User user = new(0);
            string? textMsg;
            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                textMsg = callbackQuery.Data;
                user = botUsers.GetOrAdd(update.CallbackQuery.Message.Chat.Id, id => new User(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Chat.Username));

                reply = text.WhatDoYouWant(bot, user, textMsg);
            }

            if (update.Message is not null)
            {
                user = botUsers.GetOrAdd(update.Message.Chat.Id, id => new User(update.Message.Chat.Id, update.Message.Chat.Username));

                textMsg = update.Message.Text;
                if (textMsg is not null)
                {
                    OnHandleUpdateStarted?.Invoke(user, textMsg); ;
                    reply = text.WhatDoYouWant(bot, user, textMsg);
                }
                if (update.Message.Document is not null)
                {
                    var doc = update.Message.Document;
                    if (doc.FileName is null) return;
                    OnHandleUpdateStarted?.Invoke(user, $"Received file \"{doc.FileName}\"");
                    reply = await file.ReadFile(bot, user, doc);
                    await SendPlot(user);
                }
            }
            if (reply != string.Empty) await SendLogToBot(bot, token, user.ChatId, reply);
        }

        private async Task OpenColorKeyboard(ITelegramBotClient bot, long chatId)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Cool", "/colorMapCool") },
                new[] { InlineKeyboardButton.WithCallbackData("Spring", "/colorMapSpring") },
                new[] { InlineKeyboardButton.WithCallbackData("Plasma", "/colorMapPlasma") },
                new[] { InlineKeyboardButton.WithCallbackData("Jet", "/colorMapJet") },
            });
            await bot.SendMessage(
                    chatId: chatId,
                    text: "Выберите вариант:",
                    replyMarkup: inlineKeyboard);
        }

        public async Task SendPlot(User user)
        {
            using var ms = new MemoryStream();

            var pcl = user.CurrentPcl is null ? user.OrigPcl : user.CurrentPcl; ;
            var model = new PlotModel();
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid });

            var colorAxis = new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Key = "pointColors",
                Palette = new OxyPalette(pcl.Colors),
                Minimum = 0,
                Maximum = pcl.Colors.Count - 1,
                HighColor = OxyColors.Undefined,
                LowColor = OxyColors.Undefined,
                IsAxisVisible = false
            };
            model.Axes.Add(colorAxis);
            var scatter = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                ColorAxisKey = colorAxis.Key
            };
            for (int i = 0; i < pcl.PointCloud.Count; i++)
                scatter.Points.Add(new ScatterPoint(pcl.PointCloud[i].X, pcl.PointCloud[i].Y, scatter.MarkerSize, value: i));
            model.Series.Add(scatter);
            var exporter = new OxyPlot.SkiaSharp.PngExporter() { Height = 600, Width = 800 };
            exporter.Export(model, ms);
            ms.Seek(0, SeekOrigin.Begin);
            await botClient.SendPhoto(
                chatId: user.ChatId,
                photo: InputFile.FromStream(ms, fileName: "projection.png"),
                caption: "Вот ваш график"
            );
        }

        private async Task OpenPipelineKeyboard(ITelegramBotClient bot, long chatId)
        {
            OnHandleUpdateCompleted?.Invoke(null, "Открываю клавиатуру для выбора этапов обработки облака точек");
            InlineKeyboardMarkup inlineKeyboard = new(new[] {
                    new[] {InlineKeyboardButton.WithCallbackData("Воксельный фильтр", "voxel") },
                    new[] {InlineKeyboardButton.WithCallbackData("DBSCAN", "dbscan")},
                    new[] {InlineKeyboardButton.WithCallbackData("Сглаживание по Гауссу", "gauss")},
                    new[] {InlineKeyboardButton.WithCallbackData("Поворот и перемещение", "transform")},
                    new[] {InlineKeyboardButton.WithCallbackData("Начать расчет", "gopipe")},
                    new[] {InlineKeyboardButton.WithCallbackData("Отменить обработку", "resetpipe")}}
                    );

            await bot.SendMessage(
                    chatId: chatId,
                    text: "Выберите вариант:",
                    replyMarkup: inlineKeyboard);
        }
    }
}