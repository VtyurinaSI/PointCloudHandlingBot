using PointCloudHandlingBot.PointCloudProcesses.PipelineSteps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Numerics;
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
            (string? answer, Image<Rgba32>? image) reply = (null, null);


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
                    OnHandleUpdateStarted?.Invoke(user, textMsg);
                    reply = text.WhatDoYouWant(bot, user, textMsg);
                }
                if (update.Message.Document is not null)
                {
                    //file.PclProcessMessageEvent += SendLogToBot;
                    var doc = update.Message.Document;
                    if (doc.FileName is null) return;
                    OnHandleUpdateStarted?.Invoke(user, $"Received file \"{doc.FileName}\"");
                    reply.answer = await file.ReadFile(bot, user, doc);
                    reply = file.MakeResultPcl(user.ChatId, user.OrigPcl);
                }


            }
            if (reply.answer is not null) await SendLogToBot(bot, token, user.ChatId, reply.answer);
            if (reply.image is not null) await SendProjectionAsync(reply.image, user);

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

        public async Task SendProjectionAsync(Image<Rgba32> image, User user)
        {
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            ms.Position = 0;
            var input = InputFile.FromStream(ms, fileName: "projection.png");
            await Program.botClient.SendPhoto(
                chatId: user.ChatId,
                photo: input,
                caption: "Проекция облака точек");
            image.Dispose();
        }

        private async Task OpenPipelineKeyboard(ITelegramBotClient bot, long chatId)
        {
            OnHandleUpdateCompleted?.Invoke(null, "Открываю клавиатуру для выбора этапов обработки облака точек");
            InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {

                    new[] {InlineKeyboardButton.WithCallbackData("Воксельный фильтр", "voxel") },
                   new[] { InlineKeyboardButton.WithCallbackData("DBSCAN", "dbscan")},
                    new[] {InlineKeyboardButton.WithCallbackData("Сглаживание по Гауссу", "gauss")},
                    new[] {InlineKeyboardButton.WithCallbackData("Поворот и перемещение", "transform")},
                    new[] {InlineKeyboardButton.WithCallbackData("Начать расчет", "gopipe")},
                    new[] {InlineKeyboardButton.WithCallbackData("Отменить обработку", "resetpipe")},

            });
            await bot.SendMessage(
                    chatId: chatId,
                    text: "Выберите вариант:",
                    replyMarkup: inlineKeyboard);
        }
    }
}