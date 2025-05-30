using PointCloudHandlingBot.MsgPipeline;

namespace PointCloudHandlingBot.Commands
{
    internal class AnalyzeCmd : CommandBase
    {
        public AnalyzeCmd(Logger logger) : base("/analyze", logger, 0)
        {
        }

        public override List<IMsgPipelineSteps> Process(User user)
        {
            return [new KeyboardMsg(Keyboards.Analyze)];
        }
    }
}