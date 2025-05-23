using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot
{
    internal class PclLims
    {
        internal float xMin = float.MaxValue;
        internal float xMax = float.MinValue;
        internal float yMin = float.MaxValue;
        internal float yMax = float.MinValue;
        internal float zMin = float.MaxValue;
        internal float zMax = float.MinValue;

        internal void UpdMax(float current, ref float max) => max = current > max ? current : max;
        internal void UpdMin(float current, ref float min) => min = current < min ? current : min;

    }
}
