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
    internal class TransformCmd : CommandBase
    {
        public TransformCmd(Logger logger) : base("/transform", logger, 6,
            ["Поворот воркуг X", "Поворот воркуг Y", "Поворот воркуг Z",
            "Перенос вдоль X", "Перенос вдоль Y", "Перенос вдоль Z"
        ])
        {
        }
        public override List<IMsgPipelineSteps> Process(User user)
        {
            logger.LogBot($"Трансформация матрицы. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Делаю поворот/перенос...");

            Translate tr = new(new((float)ParseParts[0], (float)ParseParts[1], (float)ParseParts[2]),
               new((float)ParseParts[3], (float)ParseParts[4], (float)ParseParts[5]));
            user.CurrentPcl = tr.Process(user.CurrentPcl);
            user.CurrentPcl.UpdLims(); 
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl,user.ColorMap);
            logger.LogBot($"Трансформация выполнена. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "готово");
            return [
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)
                ];

        }
    }
}
