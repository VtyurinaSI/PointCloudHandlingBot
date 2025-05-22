using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static Telegram.Bot.TelegramBotClient;

namespace PointCloudHandlingBot
{
    class UpdateHandler : IUpdateHandler
    {
        record CatFactDto(string Fact, int Length);
        public delegate Task MessageHandler(string msg);
        public event MessageHandler? OnHandleUpdateStarted;
        public event MessageHandler? OnHandleUpdateCompleted;
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine($"Ошибка: {exception.Message}"), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {

            if (update.Message is null)
            {
                await Task.Run(() => Console.WriteLine("Message is null"));
                return;
            }
            OnHandleUpdateStarted?.Invoke(update.Message.Text);
            var botText = "Сообщение получено!";
            await botClient.SendMessage(update.Message.Chat.Id, botText, cancellationToken: token);

            OnHandleUpdateCompleted?.Invoke(update.Message.Text);
        }


    }
}
