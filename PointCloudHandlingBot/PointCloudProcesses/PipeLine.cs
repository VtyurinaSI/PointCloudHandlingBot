using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    internal class PipeLine
    {
        private readonly IList<IPipelineSteps> _steps = new List<IPipelineSteps>();

        internal PipeLine AddStep(IPipelineSteps step)
        {
            _steps.Add(step);
            return this;
        }

        public void Execute(UserPclFeatures usPpcl)
        {
            var curPcl = usPpcl;
            foreach (var step in _steps) curPcl = step.Process(curPcl);

        }
    }
}
