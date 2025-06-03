using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;

namespace PointCloudHandlingBot.Commands
{
    internal class DBSCANfiltCmd : CommandBase
    {
        private static readonly List<string> paramsDescriptions =
            ["Введи радиус поиска соседей для точки(например, 0.1). Определяет, насколько далеко алгоритм будет искать соседние точки",
             "Минимальное количество точек для образования кластера",
             "Мминимальный объём кластера (например, 10). Кластеры меньшего объёма будут считаться шумом и удаляться"];
        internal DBSCANfiltCmd(Logging.Logger logger)
            : base("/DBSCANfilt", logger, 3, paramsDescriptions) { }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            logger.LogBot($"Применение фильтрации шума. Параметры: {string.Join(" ", ParseParts)}",
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
