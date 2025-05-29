using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PointCloudHandlingBot.PointCloudProcesses.AnalyzePipelineSteps
{
    internal class Translate : IAnalyzePipelineSteps
    {
        private Vector3 trans;
        private Vector3 rot;

        internal Translate(Vector3 _rot, Vector3 _trans)
        {
            trans = _trans;
            rot = _rot;
        }
        public UserPclFeatures Process(UserPclFeatures pcl)
        {
            int vol = pcl.PointCloud.Count;
            List<Vector3> rotated = new(new Vector3[vol]);

            Matrix4x4 rotationMatrix = CreateRotationMatrix(rot);
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(trans);
            Matrix4x4 transformationMatrix = translationMatrix * rotationMatrix;

            for (int i = 0; i < vol; i++)
            {
                var transformedVector = Vector3.Transform(pcl.PointCloud[i], transformationMatrix);
                rotated[i] = transformedVector;
            }
            pcl.PointCloud = rotated;
            return pcl;
        }

        private static Matrix4x4 CreateRotationMatrix(Vector3 rotationAngles)
        {
            float rx = MathF.PI * rotationAngles.X / 180.0f;
            float ry = MathF.PI * rotationAngles.Y / 180.0f;
            float rz = MathF.PI * rotationAngles.Z / 180.0f;

            var rotationX = Matrix4x4.CreateRotationX(rx);
            var rotationY = Matrix4x4.CreateRotationY(ry);
            var rotationZ = Matrix4x4.CreateRotationZ(rz);

            return rotationX * rotationY * rotationZ;
        }

    }
}
