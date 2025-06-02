using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;

namespace PointCloudHandlingBot.Commands
{
    internal class StopFilterCmd : CommandBase
    {
        public StopFilterCmd(Logger logger) : base("/stopFilter", logger, 0)
        {
        }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            return new List<IMsgPipelineSteps>
            {
                new TextMsg("Преодбработка выполнена"),
                new ImageMsg(Drawing.Make3dImg),
                new KeyboardMsg(Keyboards.MainMenu)
            };
        }
    }
}