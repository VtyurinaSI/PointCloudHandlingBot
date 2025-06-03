using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class MedianCmd : CommandBase
    {
        public MedianCmd(Logging.Logger logger) :
            base("/median", logger, 2,
                ["Количество ближайших соседей, используемых для оценки плотности точки. Рекомендуется значение 4–8",
            "Пороговое расстояние до k-го ближайшего соседа. Если расстояние превышает этот порог, точка считается шумовой и удаляется."])
        {
        }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            logger.LogBot($"Применение медианного фильтра. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Считаю...");
            user.CurrentPcl.PointCloud = StandartFilters.Median(user.CurrentPcl.PointCloud, ParseParts);
            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            logger.LogBot($"Применен медианный фильтр",
            LogLevel.Information, user, "Вот что получилось");

            return [
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)
                ];
        }
    }

}
