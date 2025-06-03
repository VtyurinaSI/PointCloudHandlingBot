using PointCloudHandlingBot.MsgPipeline;

namespace PointCloudHandlingBot.Commands
{
    public abstract class CommandBase
    {
        public CommandBase(string name, Logging.Logger logger,
            int parsePartsNum, List<string>? paramsDescriptions = null)
        {
            this.logger = logger;
            CommandName = name;
            ParsePartsNum = parsePartsNum;
            ParamsDescriptions = paramsDescriptions;
        }

        private protected Logging.Logger logger;
        public int ParsePartsNum { get; set; }
        public string CommandName { get; set; }
        private protected List<double> ParseParts { get; set; } = [];
        private protected List<string>? ParamsDescriptions { get; set; }
        public bool IsInited => ParseParts.Count == ParsePartsNum;
        public string? FirstParName => ParamsDescriptions == null ? null : ParamsDescriptions[0];
        public virtual string SetParseParts(string textMsg)
        {
            if (double.TryParse(textMsg.Trim().Replace('.', ','), out double result))
            {
                ParseParts.Add(result);
                return ParseParts.Count < ParsePartsNum ? ParamsDescriptions[ParseParts.Count] : "Параметры установлены";
            }
            else
                return "Неверный формат параметра. Пожалуйста, введи число";
        }

        public abstract List<IMsgPipelineSteps> Process(UserData user);
    }
}