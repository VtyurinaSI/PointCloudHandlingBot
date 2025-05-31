using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace PointCloudHandlingBot
{
    internal class Program
    {
        public static ConcurrentDictionary<long, User> botUsers = new();

        public static TelegramBotClient botClient = new TelegramBotClient(BotToken.token);
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            var handler = new UpdateHandler(botClient);

            var me = await botClient.GetMe(cts.Token);

            Console.WriteLine($"{me.FirstName} запущен!");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.CallbackQuery, UpdateType.Message },
                DropPendingUpdates = true
            };

            botClient.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, receiverOptions);


            Console.WriteLine($"Нажмите клавишу A для выхода (в латинской раскладке)");
            while (!cts.IsCancellationRequested)
            {
                if (Console.ReadKey().KeyChar == 'A')
                    await cts.CancelAsync();
                else Console.WriteLine($"\nИнформация о боте:\nID: {me.Id}\nUsername: @{me.Username}\nИмя: {me.FirstName} {me.LastName}");
            }

        }
    }
}

