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
    internal class StatisticalCmd:CommandBase
    {
        public StatisticalCmd(Logger logger) : base("/statistical", logger, 2,
            ["Сколько ближайших соседей смотреть (8–16 нормально для обычного облака)?",
            "Множитель стандартного отклонения. Чем больше, тем меньше фильтрация (оставляет больше точек). Обычно 1.0–2.0"])
        {}
        public override List<IMsgPipelineSteps> Process(User user)
        {
            logger.LogBot($"Применение Statistical Outlier Removal. Параметры: {string.Join(" ", ParseParts)}",
            LogLevel.Information, user, "Считаю...");
            user.CurrentPcl.PointCloud = Filter(user.CurrentPcl.PointCloud);
            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = Drawing.Coloring(user.CurrentPcl,user.ColorMap);
            logger.LogBot($"Применен Statistical Outlier Removal",
            LogLevel.Information, user, "Вот что получилось");

            return [
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.Analyze)
                ];
        }
        private List<Vector3> Filter(List<Vector3> points) { 
            int n = points.Count;
            var meanDistances = new float[n];

            for (int i = 0; i < n; i++)
            {
                var distances = new List<float>(n - 1);
                var p = points[i];
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    distances.Add(Vector3.Distance(p, points[j]));
                }
                distances.Sort();
                float mean = distances.Take((int)ParseParts[0]).Average();
                meanDistances[i] = mean;
            }

            float globalMean = meanDistances.Average();
            float globalStd = (float)Math.Sqrt(meanDistances.Average(d => (d - globalMean) * (d - globalMean)));

            var filtered = new List<Vector3>();
            for (int i = 0; i < n; i++)
            {
                if (meanDistances[i] <= globalMean + ParseParts[1] * globalStd)
                    filtered.Add(points[i]);
            }
            return filtered;
        }
    }
}
