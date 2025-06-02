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
        public XRotCmd(Logger logger) : base("/xrot", logger, 0, ["Угол поврота вокруг оси X"])
        {
        }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            logger.LogBot($"Поворчаиваю вокруг X. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Поворачиваю...");

            int vol = user.CurrentPcl.PointCloud.Count;
            List<Vector3> rotated = new(new Vector3[vol]);

            Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationX((float)ParseParts[0] * MathF.PI / 180.0f);


            for (int i = 0; i < vol; i++)
            {
                var transformedVector = Vector3.Transform(user.CurrentPcl.PointCloud[i], rotationMatrix);
                rotated[i] = transformedVector;
            }

            user.CurrentPcl.PointCloud = rotated;
            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl,user.ColorMap);
            logger.LogBot($"Поворот выполнен",
            LogLevel.Information, user, "Готово");
            return [ new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)];
        }
    }
}
