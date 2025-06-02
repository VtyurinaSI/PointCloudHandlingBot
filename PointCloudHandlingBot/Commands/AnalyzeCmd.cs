using PointCloudHandlingBot.MsgPipeline;

namespace PointCloudHandlingBot.Commands
{
    internal class AnalyzeCmd : CommandBase
    {
        public AnalyzeCmd(Logger logger) : base("/analyze", logger, 0)
        {
        }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            user.CurrentPcl.Clusters = null;
            return [new KeyboardMsg(Keyboards.Analyze)];
        }
    }
}