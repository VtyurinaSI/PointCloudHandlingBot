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
        internal VoxelCmd() : base("/voxel") { }
        public override List<IMsgPipelineSteps> Process(User user)
        {
            InvokeSendConditionEvent(user, "Применяю воксельный фильтр...");

            Voxel voxel = new(double.Parse(ParseParts[0]));
            voxel.Process(user.CurrentPcl);

            InvokeSendConditionEvent(user, "Готово");
            return [new TextMsg("Воксель"),
                new ImageMsg(Drawing.Make3dImg),
                new KeyboardMsg(keyboards.MainMenu)];
        }

        
    }
}
