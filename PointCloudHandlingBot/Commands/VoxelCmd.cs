using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using PointCloudHandlingBot.PointCloudProcesses.AnalyzePipelineSteps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class VoxelCmd : CommandBase
    {
        private readonly Logger logger;
        private readonly Keyboards keyboards;
        internal VoxelCmd(ILogger logger,Keyboards keyboards) : base("/voxel")
        {
            this.logger = (Logger)logger;
            this.keyboards = keyboards;
            ParsePartsNum = 1;
            ParamsDescriptions = [..Enumerable.Repeat("Param", ParsePartsNum)];
        }

        public override List<IMsgPipelineSteps> Process(User user)
        {
            logger.LogBot($"Применение воксельного фильтра. Параметры: {string.Join(" ", ParseParts)}",
                LogLevel.Information, user, "Применяю воксельный фильтр...");

            Voxel voxel = new(ParseParts[0]);
            voxel.Process(user.CurrentPcl);

            logger.LogBot($"Воксельный фильтр применен",
                LogLevel.Information, user, "Готово");

            return [new TextMsg("Воксель"),
                new ImageMsg(Drawing.Make3dImg),
                new KeyboardMsg(keyboards.MainMenu)];
        }


    }
}
