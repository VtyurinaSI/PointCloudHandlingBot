using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PointCloudHandlingBot.MsgPipeline
{
    internal class KeyboardMsg : IMsgPipelineSteps
    {
        public KeyboardMsg(InlineKeyboardMarkup kb)
        {
            keyboardMarkup = kb;
        }
        private readonly InlineKeyboardMarkup keyboardMarkup;
        public async Task Send(ITelegramBotClient bot, User user)
        {
            await bot.SendMessage(
                    chatId: user.ChatId,
                    text: "Выберите вариант:",
                    replyMarkup: keyboardMarkup);
        }
    }
}
