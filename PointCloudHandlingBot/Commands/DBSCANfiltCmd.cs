using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using PointCloudHandlingBot.PointCloudProcesses.AnalyzePipelineSteps;

namespace PointCloudHandlingBot.Commands
{
    internal class DBSCANfiltCmd : CommandBase
    {
        private static readonly List<string> paramsDescriptions =
            ["Внутрикластерное расстояние",
             "Минимальное количество точек в кластере",
             "Минимальное количество точек в кластере для удаления шума"];
        internal DBSCANfiltCmd(Logger logger)
            : base("/DBSCANfilt", logger, 3, paramsDescriptions) { }

        public override List<IMsgPipelineSteps> Process(User user)
        {
            logger.LogBot($"Применение воксельного фильтра. Параметры: {string.Join(" ", ParseParts)}",
                LogLevel.Information, user, "Удаляю шум...");

            DBSCANfilt dbscan = new(ParseParts[0], (int)ParseParts[1], (int)ParseParts[2]);
            dbscan.Process(user.CurrentPcl);
            user.CurrentPcl.UpdLims();
            
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            logger.LogBot($"Шум удален",
                LogLevel.Information, user, "Готово");
            return [
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)];
        }
    }
}
