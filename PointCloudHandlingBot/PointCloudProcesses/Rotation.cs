using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.PointCloudProcesses
{
    internal static class Rotation
    {
        internal static List<Vector3> GetRotate(List<Vector3> plc, double ang, string axis)
        {
            Matrix4x4 rotationMatrix = new();
            if (axis == "x") rotationMatrix = RX((float)ang);
            if (axis == "y") rotationMatrix = RY((float)ang);
            if (axis == "z") rotationMatrix = RZ((float)ang);

            int vol = plc.Count;
            List<Vector3> rotated = [.. new Vector3[vol]];
            for (int i = 0; i < vol; i++)
            {
                var transformedVector = Vector3.Transform(plc[i], rotationMatrix);
                rotated[i] = transformedVector;
            }
            return rotated;
        }
        private static Matrix4x4 RX(float ang) => Matrix4x4.CreateRotationX(ang * MathF.PI / 180.0f);
        private static Matrix4x4 RY(float ang) => Matrix4x4.CreateRotationY(ang * MathF.PI / 180.0f);
        private static Matrix4x4 RZ(float ang) => Matrix4x4.CreateRotationZ(ang * MathF.PI / 180.0f);
    }
}
