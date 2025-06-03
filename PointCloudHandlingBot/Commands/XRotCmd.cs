using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class XRotCmd : CommandBase
    {
        public XRotCmd(Logging.Logger logger) : base("/xrot", logger, 1, ["Угол поврота вокруг оси X"])
        {
        }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            logger.LogBot($"Поворчаиваю вокруг X. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Поворачиваю...");



            user.CurrentPcl.PointCloud = Rotation.GetRotate(user.CurrentPcl.PointCloud, ParseParts[0], "x");

            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            logger.LogBot($"Поворот выполнен",
            LogLevel.Information, user, "Готово");
            return [ new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)];
        }
    }
}
