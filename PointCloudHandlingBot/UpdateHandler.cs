using PointCloudHandlingBot.PointCloudProcesses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static PointCloudHandlingBot.Program;

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
        public delegate Task MessageHandler(User user, string msg);
        public event MessageHandler? OnHandleUpdateCompleted;

        public event MessageHandler? OnHandleUpdateStarted;
        public async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine($"Ошибка: {exception.Message}\n{exception.StackTrace}"), cancellationToken);
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
            PclProcess pclProc = new();
            string ch = update.Message.Chat.Username;
            long id = update.Message.Chat.Id;
            User user = botUsers.GetOrAdd(id, id => new User(id, ch));
            PclReading pcl = new();
            Drawing draw = new();
            string? textMsg = update.Message.Text;
            if (textMsg is not null)
            {
                OnHandleUpdateStarted?.Invoke(user, textMsg);
                switch (textMsg)
                {
                    case "/start":
                        await botClient.SendMessage(ch,
                            "Привет! Я умею обрабатывать объемные облака точек!",
                            cancellationToken: token);

                        break;
                    //case string v when v.StartsWith("/voxel"):
                    //    string rest = v.Substring(6).Replace('.', ',');
                    //    if (double.TryParse(rest, out double voxelSize))
                    //    {

                    //        List<Vector3> voxeled = pclProc.VoxelFilter(user.PointCloud, voxelSize);
                    //        user.Colors = draw.Coloring(user.PointCloud, user.PclLims, MapJet);
                    //        var image = draw.DrawProjection(voxeled, user.Colors, user.PclLims);
                    //        await SendProjectionAsync(image, user);
                    //    }
                    //    break;
                    default:

                        await botClient.SendMessage(id,
                            "Получил сообщение",
                            cancellationToken: token);
                        break;
                }
            }
            if (update.Message.Document is not null)
            {
                var doc = update.Message.Document;
                if (doc.FileName is null) return;

                string fileName = doc.FileName;
                OnHandleUpdateStarted?.Invoke(user, $"Received file \"{fileName}\"");
                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (extension == ".txt" || extension == ".ply")
                {
                    await SendLogToBot(id, "Обрабатываю...");
                    var file = await botClient.GetFile(doc.FileId);
                    await using var ms = new MemoryStream();
                    await botClient.DownloadFile(file.FilePath, ms);
                    ms.Position = 0;
                    await SendLogToBot(id, "Скачал файл...");
                    using var sr = new StreamReader(ms);
                    string data = await sr.ReadToEndAsync();
                    var lines = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    await SendLogToBot(id, "Подготавливаю результат...");
                    Image<Rgba32> image;
                    if (extension == ".txt")
                    {
                        (user.PointCloud, user.PclLims) = pcl.ReadPointCloud_txt(lines);


                        //user.Colors = draw.Coloring(user.PointCloud, user.PclLims, MapCool);

                        //image = draw.DrawProjection(user.PointCloud, user.Colors, user.PclLims);
                        //await SendProjectionAsync(image, user);

                        //user.Colors = draw.Coloring(user.PointCloud, user.PclLims, MapJet);
                        //image = draw.DrawProjection(user.PointCloud, user.Colors, user.PclLims);
                        //await SendProjectionAsync(image, user);

                        user.Colors = draw.Coloring(user.PointCloud, user.PclLims, Drawing.MapSpring);
                        //image = draw.DrawProjection(user.PointCloud, user.Colors, user.PclLims);
                        //await SendProjectionAsync(image, user);

                        //user.Colors = draw.Coloring(user.PointCloud, user.PclLims, MapPlasma);
                    }
                    else
                        (user.PointCloud, user.Colors, user.PclLims) = pcl.ReadPointCloud_ply(lines);

                     image = draw.DrawProjection(user.PointCloud, user.Colors, user.PclLims);
                    await SendProjectionAsync(image, user);
                }
                else

                    await botClient.SendMessage(update.Message.Chat.Id,
                        "Я не умею работать с такими файлами Т_Т",
                        cancellationToken: token);

            }
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
