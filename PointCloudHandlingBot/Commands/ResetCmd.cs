using PointCloudHandlingBot.MsgPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class ResetCmd: CommandBase
    {
        public ResetCmd(Logger logger) : base("/reset", logger, 0)
        {
        }
        public override List<IMsgPipelineSteps> Process(User user)
        {
            return [new TextMsg("Предобработка выполнена"),
            new KeyboardMsg(Keyboards.MainMenu)];
        }
    }
}
