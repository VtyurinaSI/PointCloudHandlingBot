using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class ColorMapCmd : CommandBase
    {
        public ColorMapCmd(Logger logger) : base("/colormap", logger, 1, ["SetecolorMap"])
        {
        }
        public override List<IMsgPipelineSteps> Process(User user)
        {
            switch ((int)ParseParts[0])
            {
                case 1:
                    user.ColorMap = Drawing.MapCool;
                    break;
                case 2:
                    user.ColorMap = Drawing.MapPlasma;
                    break;
                case 3:
                    user.ColorMap = Drawing.MapJet;
                    break;
                default:
                    user.ColorMap = Drawing.MapSpring;
                    break;

            }
            if (user.OrigPcl != null)
            {
                
                user.OrigPcl.Colors = Drawing.Coloring(user.OrigPcl, user.ColorMap);
                return [new TextMsg("Теперь буду рисовать в новых цветах"),
                    new ImageMsg(Drawing.Make3dImg),
                    new KeyboardMsg(Keyboards.MainMenu)
                    ];
            }
            return [new TextMsg("Теперь буду рисовать в новых цветах")];
        }
    }
}
