using PointCloudHandlingBot.PointCloudProcesses.PipelineSteps;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    static internal class PipelineManager
    {
        static public PipeLine ParseAndSet(string _stagesStr)
        {
            List<string> _stages = _stagesStr.Split('/').ToList();

            string rest;
            PipeLine pipe = new();
            foreach (var stage in _stages)
            {
                switch (stage)
                {
                    case string tr when tr.StartsWith("transform"):
                        rest = tr.Substring(9).Replace('.', ',');
                        List<double> paramTr = rest.Split(':')
                           .Select(d => double.Parse(d))
                           .ToList();
                        Translate transl = new(
                            new((float)paramTr[0], (float)paramTr[1], (float)paramTr[2]),
                            new((float)paramTr[3], (float)paramTr[4], (float)paramTr[5]));
                        pipe.AddStep(transl);
                        break;

                    case string v when v.StartsWith("voxel"):
                        rest = v.Substring(5).Replace('.', ',');
                        double.TryParse(rest, out double voxelSize);
                        List<double> paramVox = [];
                        Voxel voxel = new(voxelSize);
                        pipe.AddStep(voxel);

                        break;
                    case string g when g.StartsWith("gauss"):
                        rest = g.Substring(5).Replace('.', ',');
                        if (rest.Length > 0)
                        {
                            List<double> paramG = rest.Split(':')
                               .Select(d => double.Parse(d))
                               .ToList();
                            Gaussian gauss = new((float)paramG[0], (float)paramG[1]);
                            pipe.AddStep(gauss);
                        }
                        else
                        {
                            Gaussian gauss = new();
                            pipe.AddStep(gauss);
                        }
                        break;
                    case string d when d.StartsWith("dbscan"):
                        rest = d.Substring(6).Replace('.', ',');
                        List<double> paramDBS = rest.Split(':')
                           .Select(d => double.Parse(d))
                           .ToList();
                        DBSCAN dbscan = new(paramDBS[0], (int)paramDBS[1], (int)paramDBS[2]);
                        pipe.AddStep(dbscan);
                        break;
                }
            }
            return pipe;
        }
    }
}
