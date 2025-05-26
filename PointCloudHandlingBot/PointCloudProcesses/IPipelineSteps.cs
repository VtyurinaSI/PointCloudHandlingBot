using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    interface IPipelineSteps
    {
        UserPclFeatures Process(UserPclFeatures pcl);

    }
}
