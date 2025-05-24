using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    public class PclLims
    {
        public float xMin = float.MaxValue;
        public float xMax = float.MinValue;
        public float yMin = float.MaxValue;
        public float yMax = float.MinValue;
        public float zMin = float.MaxValue;
        public float zMax = float.MinValue;

        public void UpdMax(float current, ref float max) => max = current > max ? current : max;
        public void UpdMin(float current, ref float min) => min = current < min ? current : min;

    }
}
