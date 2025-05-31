using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal class MedianCmd : CommandBase
    {
        public MedianCmd(Logger logger ) :
            base("/median", logger, 2, 
                ["Количество ближайших соседей, используемых для оценки плотности точки. Рекомендуется значение 4–8",
            "Пороговое расстояние до k-го ближайшего соседа. Если расстояние превышает этот порог, точка считается шумовой и удаляется."])
        {
        }

        public override List<IMsgPipelineSteps> Process(User user)
        {
            logger.LogBot($"Применение медианного фильтра. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Считаю...");
            user.CurrentPcl.PointCloud = Filter(user.CurrentPcl.PointCloud);
            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl, user.ColorMap);
            logger.LogBot($"Применен медианный фильтр",
            LogLevel.Information, user, "Вот что получилось");

            return [
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)
                ];
        }
        private List<Vector3> Filter(List<Vector3> points)
        {
            var result = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
            {
                var distances = new List<float>();
                for (int j = 0; j < points.Count; j++)
                {
                    if (i == j) continue;
                    distances.Add(Vector3.Distance(points[i], points[j]));
                }
                distances.Sort();
                float medianDist = distances[(int)ParseParts[0]]; 

                if (medianDist < ParseParts[1]) 
                    result.Add(points[i]);
            }
            return result;
        }
    }
    
}
