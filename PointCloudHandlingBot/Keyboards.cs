using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PointCloudHandlingBot
{
    public static class Keyboards
    {
        public static InlineKeyboardMarkup Analyze { get; set; } = new(new[] {
                    new[] {InlineKeyboardButton.WithCallbackData("Воксельный фильтр", "/voxel") },
                    new[] {InlineKeyboardButton.WithCallbackData("DBSCAN", "/DBSCANfilt")},
                    new[] {InlineKeyboardButton.WithCallbackData("Сглаживание по Гауссу", "/gauss")},
                    new[] {InlineKeyboardButton.WithCallbackData("Поворот и перемещение", "transform")},
                    new[] {InlineKeyboardButton.WithCallbackData("Начать расчет", "/go")},
                    new[] {InlineKeyboardButton.WithCallbackData("Сбросить всю обработку", "/reset")}}
                    );
        public static InlineKeyboardMarkup ColorMap { get; set; } = new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Spring", "0") },
                new[] { InlineKeyboardButton.WithCallbackData("Cool", "1") },
                new[] { InlineKeyboardButton.WithCallbackData("Plasma", "2") },
                new[] { InlineKeyboardButton.WithCallbackData("Jet", "3") },
            });

        public static InlineKeyboardMarkup MainMenu { get; set; } = new(new[] {
                    new[] {InlineKeyboardButton.WithCallbackData("Выбрать цветовую карту", "/colorMap") },
                    new[] {InlineKeyboardButton.WithCallbackData("Обработать изображение", "/analyze")},
                    new[] {InlineKeyboardButton.WithCallbackData("Кластеризировать объекты", "/cluster")},
        });
    }
}
