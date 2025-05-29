using PointCloudHandlingBot.PointCloudProcesses;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static PointCloudHandlingBot.Program;
namespace PointCloudHandlingBot
{

    class UpdateHandler : IUpdateHandler
    {
        public UpdateHandler()
        {
            text = new();
            text.OpenAnalizeKeyboardEvent += OpenPipelineKeyboard;
            text.OpenColorKeyboardEvent += OpenColorKeyboard;
            text.SendImageEvent += SendPlot;
            text.OpenMainEvent += OpenBaseKeyboard;
            text.SendTextEvent += SendLogToBot;
            file = new();
        }
        ITelegramBotClient bot;
        private readonly FileHandling file;
        

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
            this.bot = bot;
            string reply = string.Empty;


            User user = new(0);
            string? textMsg;
            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                textMsg = callbackQuery.Data;
                user = botUsers.GetOrAdd(update.CallbackQuery.Message.Chat.Id, id => new User(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Chat.Username));

                text.WhatDoYouWant(bot, user, textMsg);
            }

            if (update.Message is not null)
            {
                user = botUsers.GetOrAdd(update.Message.Chat.Id, id => new User(update.Message.Chat.Id, update.Message.Chat.Username));

                textMsg = update.Message.Text;
                if (textMsg is not null)
                {
                    OnHandleUpdateStarted?.Invoke(user, textMsg); ;
                    text.WhatDoYouWant(bot, user, textMsg);
                }
                if (update.Message.Document is not null)
                {
                    var doc = update.Message.Document;
                    if (doc.FileName is null) return;
                    OnHandleUpdateStarted?.Invoke(user, $"Received file \"{doc.FileName}\"");
                    reply = await file.ReadFile(bot, user, doc);
                    await SendPlot(user);
                    await OpenBaseKeyboard(bot, user.ChatId);
                }
            }
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
        private async Task SendLogToBot(User user, string msg)
        {
            await bot.SendMessage(user.ChatId, msg);
        }

        public async Task SendPlot(User user)
        {

            using var ms = new MemoryStream();
            var model = Drawing.Make3dImg(user);
            var exporter = new OxyPlot.SkiaSharp.PngExporter() { Height = 600, Width = 900 };
            exporter.Export(model, ms);
            ms.Seek(0, SeekOrigin.Begin);
            await botClient.SendPhoto(
                chatId: user.ChatId,
                photo: InputFile.FromStream(ms, fileName: "projection.png"),
                caption: "Вот ваш график"
            );
        }
        private async Task OpenBaseKeyboard(ITelegramBotClient bot, long chatId)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[] {
                    new[] {InlineKeyboardButton.WithCallbackData("Выбрать цветовую карту", "/setColor") },
                    new[] {InlineKeyboardButton.WithCallbackData("Обработать изображение", "/analyze")}}
                    );

            await bot.SendMessage(
                    chatId: chatId,
                    text: "Выберите вариант:",
                    replyMarkup: inlineKeyboard);
        }

        private async Task OpenPipelineKeyboard(ITelegramBotClient bot, long chatId)
        {
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