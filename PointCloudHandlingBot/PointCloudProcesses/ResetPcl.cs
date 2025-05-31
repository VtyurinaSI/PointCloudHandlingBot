using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    internal class ResetPcl
    {
        internal static void ResetPclHandle(User user)
        {
            user.CurrentPcl = new();
            user.CurrentPcl.PointCloud = [.. user.OrigPcl.PointCloud.Select(vect => new Vector3(vect.X, vect.Y, vect.Z))];
            user.CurrentPcl.UpdLims();
            user.CurrentPcl.Colors = [.. user.OrigPcl.Colors];
        }
    }
}