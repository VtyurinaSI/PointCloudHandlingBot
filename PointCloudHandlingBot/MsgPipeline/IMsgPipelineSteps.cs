using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PointCloudHandlingBot.MsgPipeline
{
    public interface IMsgPipelineSteps
    {
        public Task Send(ITelegramBotClient bot, User user);

    }
}
