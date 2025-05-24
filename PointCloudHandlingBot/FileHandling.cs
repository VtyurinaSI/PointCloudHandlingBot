using PointCloudHandlingBot.PointCloudProcesses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PointCloudHandlingBot
{
    class FileHandling
    {
        public delegate Task PclProcessMessage(long id, string msg);
        public event PclProcessMessage? PclProcessMessageEvent;
        public (string?, Image<Rgba32>?) MakeResultPcl(User user, List<Vector3> pcl, List<Rgba32> color)
        {
            Image<Rgba32> image;
            PclProcessMessageEvent?.Invoke(user.ChatId, "Подготавливаю результат...");
            image = Drawing.DrawProjection(pcl, color, user.PclLims);
            return ("Готово!", image);
        }

        public async Task<string> ReadFile(ITelegramBotClient bot, User user, Document doc)
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
                if (extension == ".txt")
                {
                    (user.PointCloud, user.PclLims) = pcl.ReadPointCloud_txt(lines);
                    user.Colors = Drawing.Coloring(user.PointCloud, user.PclLims, user.ColorMap);
                }
                else
                    (user.PointCloud, user.Colors, user.PclLims) = pcl.ReadPointCloud_ply(lines);
                return "Данные загружены!";
            }
            else
                return "Я не умею работать с такими файлами Т_Т";

        }
    }
}
