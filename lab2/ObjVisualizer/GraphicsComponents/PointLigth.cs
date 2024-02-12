using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ObjVisualizer.GraphicsComponents
{
    internal readonly struct PointLigth(float x, float y, float z, float intency)
    {
        public readonly float X = x;
        public readonly float Y = y;
        public readonly float Z = z;
        public readonly float Intency = intency;

        public float CalculateLight(Vector3 Point, Vector3 Normal)
        {
            float LightResult = 0.0f;
            Vector3 L = new Vector3(X, Y, Z) - Point;
            float Angle = Vector3.Dot(Normal,L);
            if (Angle > 0)
            {
                LightResult = Intency *  Angle / (L.Length() * Normal.Length());
            }

            return LightResult;
        }
    }
}
