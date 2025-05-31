using OxyPlot;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    public class PclFeatures
    {
        private List<Vector3>? _pointCloud;
        public List<Vector3>? PointCloud;
        /*{
            get => _pointCloud;
            set
            {
                _pointCloud = value;
                if (_pointCloud is not null)
                    UpdLims(_pointCloud);
            }
        }*/
        public List<OxyColor>? Colors { get; set; }
        public PclLims PclLims { get; set; } = new();

        public List<Cluster>? Clusters { get; set; }
        public void UpdLims()
        {
            PclLims = new();
            foreach (var point in PointCloud)
            {
                PclLims.UpdMin(point.X, ref PclLims.xMin);
                PclLims.UpdMax(point.X, ref PclLims.xMax);

                PclLims.UpdMin(point.Y, ref PclLims.yMin);
                PclLims.UpdMax(point.Y, ref PclLims.yMax);

                PclLims.UpdMin(point.Z, ref PclLims.zMin);
                PclLims.UpdMax(point.Z, ref PclLims.zMax);

            }
        }
    }
}
