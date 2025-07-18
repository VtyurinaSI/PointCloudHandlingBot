﻿using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PointCloudHandlingBot.Commands
{
    internal class VoxelCmd : CommandBase
    {
        internal VoxelCmd(Logging.Logger logger)
            : base("/voxel", logger,1, [.. Enumerable.Repeat("Введите размер вокселя (положительное число)", 1)]) { }

        public override List<IMsgPipelineSteps> Process(UserData user)
        {
            logger.LogBot($"Применение воксельного фильтра. Параметры: {string.Join(" ", ParseParts)}",
                LogLevel.Information, user, "Применяю воксельный фильтр...");

            Voxel voxel = new(ParseParts[0]);
            voxel.Process(user.CurrentPcl);

            logger.LogBot($"Воксельный фильтр применен",
                LogLevel.Information, user, "Готово");
            user.CurrentPcl.UpdLims(); 
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            return [new TextMsg("Воксель"),
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)];
        }


    }
}
