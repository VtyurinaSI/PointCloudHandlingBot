using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PointCloudHandlingBot
{
    public class Keyboards
    {
        public InlineKeyboardMarkup Analyze { get; set; } = new(new[] {
                    new[] {InlineKeyboardButton.WithCallbackData("Воксельный фильтр", "voxel") },
                    new[] {InlineKeyboardButton.WithCallbackData("DBSCAN", "dbscan")},
                    new[] {InlineKeyboardButton.WithCallbackData("Сглаживание по Гауссу", "gauss")},
                    new[] {InlineKeyboardButton.WithCallbackData("Поворот и перемещение", "transform")},
                    new[] {InlineKeyboardButton.WithCallbackData("Начать расчет", "gopipe")},
                    new[] {InlineKeyboardButton.WithCallbackData("Отменить обработку", "resetpipe")}}
                    );
        public InlineKeyboardMarkup ColorMap { get; set; } = new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Cool", "/colorMapCool") },
                new[] { InlineKeyboardButton.WithCallbackData("Spring", "/colorMapSpring") },
                new[] { InlineKeyboardButton.WithCallbackData("Plasma", "/colorMapPlasma") },
                new[] { InlineKeyboardButton.WithCallbackData("Jet", "/colorMapJet") },
            });

        public InlineKeyboardMarkup MainMenu { get; set; } = new(new[] {
                    new[] {InlineKeyboardButton.WithCallbackData("Выбрать цветовую карту", "/setColor") },
                    new[] {InlineKeyboardButton.WithCallbackData("Обработать изображение", "/analyze")}}
                    );
    }
}
