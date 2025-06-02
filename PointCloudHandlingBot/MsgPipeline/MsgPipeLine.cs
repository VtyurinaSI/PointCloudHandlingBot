using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PointCloudHandlingBot.MsgPipeline
{
    public static class MsgPipeLine
    {        

        public static async Task SendAll(ITelegramBotClient bot,UserData user,params IMsgPipelineSteps[] _steps)
        {
            foreach (var step in _steps) await step.Send(bot,user);

        }
    }
}
