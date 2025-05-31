using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using PointCloudHandlingBot.PointCloudProcesses.AnalyzePipelineSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class DelFloorCmd : CommandBase
    {
        public DelFloorCmd(Logger logger) : base("RANSACFloor", logger, 1, 
            ["Какая точность у камеры? (в тех единицах измерения, в оторых сохранено облако точек)"])
        {
        }

        public override List<IMsgPipelineSteps> Process(User user)
        {
            logger.LogBot($"Удаляю плоскость XZ. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Ищу пол...");

                user.CurrentPcl.PointCloud=RANSACPlate.RemoveFloor(user.CurrentPcl.PointCloud,distanceThreshold: (float)ParseParts[0]);
            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);

            logger.LogBot($"Плоскость XZ удалена",
            LogLevel.Information, user, "Вот что получилось");
            return new List<IMsgPipelineSteps>
            {
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)
            };
        }
    }
}
