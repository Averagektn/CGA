using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal readonly struct PointLight(float x, float y, float z, float intency)
    {
        public readonly float X = x;
        public readonly float Y = y;
        public readonly float Z = z;
        public readonly float Intency = intency;

        public float CalculateLightLaba2(Vector3 point, Vector3 normal)
        {
            Vector3 l = new Vector3(X, Y, Z) - point;
            int s = 100;
            float lightResult = 0f;
            float angle = Vector3.Dot(normal, l);

            if (angle > 0)
            {
                lightResult = Intency * angle / (l.Length() * normal.Length());
            }

           
            return lightResult;
        }
        public float CalculateLightLaba3(Vector3 point, Vector3 normal, Vector3 eye)
        {
            Vector3 l = new Vector3(X, Y, Z) - point;
            int s = 100;
            float lightResult = .1f;
            float angle = Vector3.Dot(normal, l);

            if (angle > 0)
            {
                lightResult = Intency * angle / (l.Length() * normal.Length());
            }
            Vector3 R = 2 * normal * angle - l;
            Vector3 V = eye - point;
            float r_dot_v = Vector3.Dot(R, V);
            if (r_dot_v > 0)
            {
                lightResult += Intency * float.Pow(r_dot_v / (R.Length() * V.Length()), s);
            }


            return lightResult;
        }
    }
}
