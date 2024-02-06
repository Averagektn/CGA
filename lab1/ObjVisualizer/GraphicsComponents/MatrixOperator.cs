using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal class MatrixOperator
    {
        public static Matrix4x4 GetModelMatrix()
        {
            return new Matrix4x4();
        }
        public static Matrix4x4 GetViewMatrix(Camera camera)
        {
            Vector3 ZAxis = Vector3.Normalize(Vector3.Subtract(camera.Eye, camera.Target));
            Vector3 XAxis = Vector3.Normalize(Vector3.Cross(camera.Up, ZAxis));
            Vector3 YAxis = camera.Up;
            return new Matrix4x4(XAxis.X, XAxis.Y, XAxis.Z, -Vector3.Dot(XAxis, camera.Eye),
                                 YAxis.X, YAxis.Y, YAxis.Z, -Vector3.Dot(YAxis, camera.Eye),
                                 ZAxis.X, ZAxis.Y, ZAxis.Z, -Vector3.Dot(ZAxis, camera.Eye),
                                 0, 0, 0, 1);
        }
        public static Matrix4x4 GetProjectionMatrix(Camera camera)
        {
            return new Matrix4x4(1 / (camera.Aspect * (float)Math.Tan(camera.FOV >> 2)), 0, 0, 0,
                                 0, 1 / ((float)Math.Tan(camera.FOV >> 2)), 0, 0,
                                 0, 0, camera.ZFar / (camera.ZNear - camera.ZFar), (camera.ZNear * camera.ZFar) / (camera.ZNear - camera.ZFar),
                                 0, 0, -1, 0);
        }
        public static Matrix4x4 GetViewPortMatrix(int Width, int Height)
        {
            return new Matrix4x4(Width >> 2, 0, 0, Width >> 2,
                                 0, -(Height >> 2), 0, Height >> 2,
                                 0, 0, 1, 0,
                                 0, 0, 0, 1);
        }

    }
}
