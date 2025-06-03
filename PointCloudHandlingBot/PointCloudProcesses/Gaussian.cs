using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    internal class Gaussian 
    {
        private float radius = 0.05f;
        private float sigma = 0.02f;
        internal Gaussian(){}
        internal Gaussian(float _radius, float _sigma)
        {
            radius = _radius;
            sigma = _sigma;
        }

        public PclFeatures Process(PclFeatures pcl)
        {
            int vol = pcl.PointCloud.Count; 
            var result = new List<Vector3>(vol);

            for (int i = 0; i < vol; i++)
            {
                Vector3 pi = pcl.PointCloud[i];
                float weightSum = 0;
                Vector3 filtered = Vector3.Zero;

                for (int j = 0; j < vol; j++)
                {
                    Vector3 pj = pcl.PointCloud[j];
                    float dist = Vector3.Distance(pi, pj);

                    if (dist <= radius)
                    {
                        float w = (float)Math.Exp(-(dist * dist) / (2 * sigma * sigma));
                        filtered += pj * w;
                        weightSum += w;
                    }
                }

                if (weightSum > 0)
                    filtered /= weightSum;
                else
                    filtered = pi;

                result.Add(filtered);
            }
            pcl.PointCloud = result;
            return pcl;
        }
    }
}
