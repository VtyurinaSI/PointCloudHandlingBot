using PointCloudHandlingBot.MsgPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    public abstract class CommandBase
    {
        public CommandBase(string name)
        {
            CommandName = name;
        }
        public List<string> ParamsDescription;
        public int ParsePartsNum { get; set; }
        public string CommandName { get; set; }
        private protected List<double> ParseParts { get; set; } = [];
        private protected List<string> ParamsDescriptions { get; set; }
        public bool IsInited => ParseParts.Count == ParsePartsNum;

        public string SetParseParts(string textMsg)
        {
            if (double.TryParse(textMsg.Trim().Replace('.', ','), out double result))
            {
                ParseParts.Add(result);
                return ParseParts.Count < ParsePartsNum ? ParamsDescriptions[ParseParts.Count] : "Параметры применены";
            }
            else
                return "Неверный формат параметра. Пожалуйста, введи число";

        }

        public abstract List<IMsgPipelineSteps> Process(User user);
    }
}