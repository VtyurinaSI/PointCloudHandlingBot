using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    public interface IPipelineSteps
    {
        public UserPclFeatures Process(UserPclFeatures pcl);

    }
}
