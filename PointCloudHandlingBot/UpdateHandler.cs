using System.Numerics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PointCloudHandlingBot
{

    class UpdateHandler : IUpdateHandler
    {
        private async Task SendLogToBot(long id, string msg)
        {
            await botClient.SendMessage(id,
                                msg,
                                cancellationToken: token);
        }
        ITelegramBotClient botClient = null!;
        CancellationToken token;
        public delegate Task MessageHandler(string msg);
        public event MessageHandler? OnHandleUpdateCompleted;

        public event MessageHandler? OnHandleUpdateStarted;
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine($"Ошибка: {exception.Message}"), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            this.botClient = botClient;
            this.token = token;

            if (update.Message is null)
            {
                await Task.Run(() => Console.WriteLine("Message is null"));
                return;
            }
            long id = update.Message.Chat.Id;
            //var botText = "Сообщение получено!";
            //await botClient.SendMessage(update.Message.Chat.Id, botText, cancellationToken: token);
            PntCldHandling pcl = new();
            string? textMsg = update.Message.Text;
            if (textMsg is not null)
            {
                OnHandleUpdateStarted?.Invoke(textMsg);
                switch (textMsg)
                {
                    case "/start":
                        await botClient.SendMessage(update.Message.Chat.Id,
                            "Привет! Я умею обрабатывать объемные облака точек!",
                            cancellationToken: token);
                        break;
                    //case "/pcl":
                    //    //await SendProjectionAsync(update.Message.Chat.Id, pcl.ReadPointCloud("pcl.txt"));
                    //    break;
                    default:
                        await botClient.SendMessage(id,
                            "Получил сообщение",
                            cancellationToken: token);
                        break;
                }
                OnHandleUpdateCompleted?.Invoke(textMsg);
            }
            if (update.Message.Document is not null)
            {
                var doc = update.Message.Document;
                if (doc.FileName is null) return;

                string fileName = doc.FileName;

                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (extension == ".txt")
                {
                    await SendLogToBot(id, "Обрабатываю...");
                    var file = await botClient.GetFile(doc.FileId);
                    await using var ms = new MemoryStream();
                    await botClient.DownloadFile(file.FilePath, ms);
                    ms.Position = 0;
                    await SendLogToBot(id, "Скачал файл...");
                    using var sr = new StreamReader(ms);
                    string data = await sr.ReadToEndAsync();
                    var lines = data.Split(new[] { "\r\n","\n" }, StringSplitOptions.None);
                    await SendLogToBot(id, "Подготавливаю результат...");
                    await SendProjectionAsync(update.Message.Chat.Id, pcl.ReadPointCloud(lines));
                }
                else

                    await botClient.SendMessage(update.Message.Chat.Id,
                        "Я не умею работать с такими файлами Т_Т",
                        cancellationToken: token);

            }
        }

        public async Task SendProjectionAsync(
         long chatId,
         IReadOnlyList<Vector3> points,
         int width = 800,
         int height = 600,
         int padding = 20)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Нет точек для проекции", nameof(points));

            float minX = points.Min(p => p.X);
            float maxX = points.Max(p => p.X);
            float minY = points.Min(p => p.Y);
            float maxY = points.Max(p => p.Y);

            float spanX = maxX - minX;
            float spanY = maxY - minY;
            if (spanX == 0) spanX = 1;
            if (spanY == 0) spanY = 1;

            float scaleX = (width - 2 * padding) / spanX;
            float scaleY = (height - 2 * padding) / spanY;
            float scale = MathF.Min(scaleX, scaleY);

            using var image = new Image<Rgba32>(width, height);
            var white = new Rgba32(255, 255, 255, 255);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    image[x, y] = white;

            var blue = new Rgba32(0, 0, 255, 255);
            foreach (var p in points)
            {
                int px = (int)((p.X - minX) * scale + padding);
                int py = height - 1 - (int)((p.Y - minY) * scale + padding);

                if (px >= 0 && px < width && py >= 0 && py < height)
                    image[px, py] = blue;
            }
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            ms.Position = 0;
            var input = InputFile.FromStream(ms, fileName: "projection.png");
            await Program.botClient.SendPhoto(
                chatId: chatId,
                photo: input,
                caption: "Проекция облака точек");
        }

        private string MakeResponseToText(string msg)
        {
            if (msg == "/start") return "Привет! Я умею обрабатывать объемные облака точек!";
            return string.Empty;
        }


    }
}
