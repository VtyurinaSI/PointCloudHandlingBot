using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PointCloudHandlingBot.Commands
{
    internal class ClusterCmd : CommandBase
    {
        private static readonly List<string> paramsDescriptions =
            ["Введи радиус поиска соседей для точки (например, 0.1). Определяет, насколько далеко алгоритм будет искать соседние точки"];
        internal ClusterCmd(Logging.Logger logger)
            : base("/cluster", logger, 1, paramsDescriptions) { }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            logger.LogBot($"Кластеризация. Параметры: {string.Join(" ", ParseParts)}",
                LogLevel.Information, user, "Делаю кластеризацию...");
            user.CurrentPcl.Clusters = null;
            Clustering cl = new();
            user.CurrentPcl.Clusters = cl.ClusteObjects(user, ParseParts[0], 10, 70);


            logger.LogBot($"Кластеризация выполнена",
                LogLevel.Information, user, "Готово");
            return [
                    new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.MainMenu)
                    ];
        }


    }
}
