using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    public class Cluster
    {
        public Vector3 Centroid { get; set; }
        public Vector3 Size { get; set; }
        public string? Name { get; set; }
    }
}
