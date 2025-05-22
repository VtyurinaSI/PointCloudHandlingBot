using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace PointCloudHandlingBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var botClient = new TelegramBotClient(BotToken.token);
            var handler = new UpdateHandler();
            try
            {
                var me = await botClient.GetMe(cts.Token);

                Console.WriteLine($"{me.FirstName} запущен!");
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = [UpdateType.Message],
                    DropPendingUpdates = true
                };

                handler.OnHandleUpdateStarted += StartHandling;
                handler.OnHandleUpdateCompleted += CompletedHandling;
                botClient.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, receiverOptions);


                Console.WriteLine($"Нажмите клавишу A для выхода (в латинской раскладке)");
                while (!cts.IsCancellationRequested)
                {
                    if (Console.ReadKey().KeyChar == 'A')
                        await cts.CancelAsync();
                    else Console.WriteLine($"\nИнформация о боте:\nID: {me.Id}\nUsername: @{me.Username}\nИмя: {me.FirstName} {me.LastName}");
                }
            }
            finally
            {
                handler.OnHandleUpdateStarted -= StartHandling;
                handler.OnHandleUpdateCompleted -= CompletedHandling;
                cts.Dispose();
            }
        }
        private static async Task StartHandling(string msg) => await Task.Run(() => Console.WriteLine($"Началась обработка сообщения \"{msg}\""));
        private static async Task CompletedHandling(string msg) => await Task.Run(() => Console.WriteLine($"Закончилась обработка сообщения \"{msg}\""));
    }
}

