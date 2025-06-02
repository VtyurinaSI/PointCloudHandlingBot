using PointCloudHandlingBot.PointCloudProcesses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PointCloudHandlingBot
{
    class FileHandling
    {
        public delegate Task PclProcessMessage(long id, string msg);
        public event PclProcessMessage? PclProcessMessageEvent;

        public async Task<string> ReadFile(ITelegramBotClient bot, UserData user, Document doc)
        {
            string extension = Path.GetExtension(doc.FileName).ToLowerInvariant();
            if (extension == ".txt" || extension == ".ply")
            {
                PclProcessMessageEvent?.Invoke(user.ChatId, "Скачиваю...");
                var file = await bot.GetFile(doc.FileId);
                await using var ms = new MemoryStream();
                await bot.DownloadFile(file.FilePath, ms);
                ms.Position = 0;

                PclProcessMessageEvent?.Invoke(user.ChatId, "Читаю...");
                using var sr = new StreamReader(ms);
                string fileData = await sr.ReadToEndAsync();
                string[] lines = fileData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                PclReading pcl = new();
                user.CurrentPcl = null;
                user.OrigPcl = new();
                if (extension == ".txt")
                {
                    user.OrigPcl.PointCloud = pcl.ReadPointCloud_txt(lines);
                    user.OrigPcl.UpdLims();
                    user.OrigPcl.Colors = Drawing.Coloring(user.OrigPcl, user.ColorMap);
                }
                else
                {
                    (user.OrigPcl.PointCloud, user.OrigPcl.Colors) = pcl.ReadPointCloud_ply(lines);
                    user.OrigPcl.UpdLims();
                }
                return "Данные загружены!";
            }
            else
                return "Я не умею работать с такими файлами Т_Т";

        }
    }
}
