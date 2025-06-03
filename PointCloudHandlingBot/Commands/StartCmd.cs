using PointCloudHandlingBot.MsgPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class StartCmd: CommandBase
    {
        public StartCmd(Logging.Logger logger) : base("/start", logger, 0)
        {}

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            return [new TextMsg("Добро пожаловать в PointCloudHandlingBot! Используйте команды для работы с облаками точек."),
            new KeyboardMsg(Keyboards.MainMenu),
            new TextMsg("Или посмотри что я могу в /help")
            ];
        }

    }
}
