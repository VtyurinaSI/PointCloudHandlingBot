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
    internal class StatisticalCmd : CommandBase
    {
        public StatisticalCmd(Logging.Logger logger) : base("/statistical", logger, 2,
            ["Сколько ближайших соседей смотреть (8–16 нормально для обычного облака)?",
            "Множитель стандартного отклонения. Чем больше, тем меньше фильтрация (оставляет больше точек). Обычно 1.0–2.0"])
        { }
        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            logger.LogBot($"Применение Statistical Outlier Removal. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Считаю...");
            user.CurrentPcl.PointCloud = StandartFilters.Statistical(user.CurrentPcl.PointCloud, ParseParts);
            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            logger.LogBot($"Применен Statistical Outlier Removal",
            LogLevel.Information, user, "Вот что получилось");

            return [
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)
                ];
        }        
    }
}
