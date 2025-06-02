using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PointCloudHandlingBot.MsgPipeline
{
    internal class TextMsg : IMsgPipelineSteps
    {
        public TextMsg(string msg)
        {
            message = msg;
        }
        private readonly string message;
        public async Task Send(ITelegramBotClient bot,UserData user)
        {
            await bot.SendMessage(user.ChatId, message);
        }
    }
}
