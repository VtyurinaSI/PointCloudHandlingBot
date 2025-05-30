using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using PointCloudHandlingBot.PointCloudProcesses.AnalyzePipelineSteps;
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
            ["Внутрикластерное расстояние",
             "Минимальное количество точек в кластере",
             "Минимальное количество точек в кластере для удаления шума"];
        internal ClusterCmd(Logger logger)
            : base("/cluster", logger,  3, paramsDescriptions) { }

    public override List<IMsgPipelineSteps> Process(User user)
    {
        logger.LogBot($"Применение воксельного фильтра. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Делаю кластеризацию...");

            Clustering cl = new();
            user.CurrentPcl.Clusters = cl.ClusteObjects( user, ParseParts[0], (int)ParseParts[1], (int)ParseParts[2]);
        
                

        logger.LogBot($"Воксельный фильтр применен",
            LogLevel.Information, user, "Готово");
        //user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
        return [//new TextMsg("Воксель"),
                //new ImageMsg(Drawing.Make3dImg),
                new KeyboardMsg(Keyboards.MainMenu)
                ];
    }


}
}
