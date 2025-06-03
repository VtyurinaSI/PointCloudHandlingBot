using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class ResetCmd: CommandBase
    {
        public ResetCmd(Logging.Logger logger) : base("/reset", logger, 0)
        {
        }
        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            ResetPcl.ResetPclHandle(user);
            return [new TextMsg("Обработка сброшена, вот сырое облако точек"),
                    new ImageMsg(Drawing.Make3dImg),
                    new HtmlMsg(),
            new KeyboardMsg(Keyboards.MainMenu)];
        }
    }
}
