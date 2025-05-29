
using PointCloudHandlingBot.PointCloudProcesses.AnalyzePipelineSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    public class AnalyzePipeLine
    {
        public enum PipeCondition
        {
            None,
            SettingStageType,
            SettingStageParams,            
        }
        private readonly IList<IAnalyzePipelineSteps> _steps = [];
        public string StageName { get; set; } = string.Empty;
        public List<double> StageParams { get; set; } = [];
        public PipeCondition Condition { get; set; } = PipeCondition.None;
        public  void CreateStep()
        {
            IAnalyzePipelineSteps step = StageName switch
            {
                "transform" => new Translate(
                    new System.Numerics.Vector3((float)StageParams[0], (float)StageParams[1], (float)StageParams[2]),
                    new System.Numerics.Vector3((float)StageParams[3], (float)StageParams[4], (float)StageParams[5])),
                "voxel" => new Voxel((double)StageParams[0]),
                "gauss" => StageParams.Count == 2 ? new Gaussian((float)StageParams[0], (float)StageParams[1]) : new Gaussian(),
                "dbscan" => new DBSCANfilt(StageParams[0], (int)StageParams[1], (int)StageParams[2]),
                _ => throw new ArgumentException($"Неподдерживаемый этап '{StageName}'")
            };

            _steps.Add(step);
        }

        public void Execute(PclFeatures usPpcl)
        {
            var curPcl = usPpcl;
            foreach (var step in _steps) curPcl = step.Process(curPcl);

        }
    }
}
