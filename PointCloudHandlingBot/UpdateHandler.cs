using PointCloudHandlingBot.PointCloudProcesses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Numerics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static PointCloudHandlingBot.Program;
using static System.Net.Mime.MediaTypeNames;

namespace PointCloudHandlingBot
{

    class UpdateHandler : IUpdateHandler
    {
        private async Task SendLogToBot(long id, string msg)
        {
            await bot.SendMessage(id,
                                msg,
                                cancellationToken: token);
        }
        ITelegramBotClient bot = null!;
        CancellationToken token;
        public delegate Task MessageHandler(User user, string msg);
        public event MessageHandler? OnHandleUpdateCompleted;

        public event MessageHandler? OnHandleUpdateStarted;
        public async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine($"Ошибка: {exception.Message}\n{exception.StackTrace}"), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            this.bot = bot;
            this.token = token;

            if (update.Message is null)
            {
                await Task.Run(() => Console.WriteLine("Message is null"));
                return;
            }
            PclProcess pclProc = new();
            User user = botUsers.GetOrAdd(update.Message.Chat.Id, id => new User(update.Message.Chat.Id, update.Message.Chat.Username));
            (string? answer, Image<Rgba32>? image) reply = (null, null);

            string? textMsg = update.Message.Text;
            if (textMsg is not null)
            {
                OnHandleUpdateStarted?.Invoke(user, textMsg);
                reply = TextMessageHandling.WhatDoYouWant(user, textMsg);
            }
            //update.Message.Caption
            if (update.Message.Document is not null)
            {
                FileHandling file = new();
                file.PclProcessMessageEvent += SendLogToBot;
                var doc = update.Message.Document;
                if (doc.FileName is null) return;
                OnHandleUpdateStarted?.Invoke(user, $"Received file \"{doc.FileName}\"");
                reply.answer = await file.ReadFile(bot, user, doc);
                reply = file.MakeResultPcl(user, user.PointCloud, user.Colors);
            }

            if (reply.answer is not null) await SendLogToBot(user.ChatId, reply.answer);
            if (reply.image is not null) await SendProjectionAsync(reply.image, user);
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
    }
}
