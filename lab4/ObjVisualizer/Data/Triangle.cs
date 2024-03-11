using System.Numerics;

namespace ObjVisualizer.Data
{
    internal struct Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 NormA, Vector3 NormB, Vector3 NormC, Vector3 RealA, Vector3 RealB, Vector3 RealC, Vector2 TextelA, Vector2 TextelB, Vector2 TextelC)
    {
        public Vector3 A { get; set; } = a;
        public Vector3 B { get; set; } = b;
        public Vector3 C { get; set; } = c;

        public Vector2 TextelA { get; set; } = TextelA;
        public Vector2 TextelB { get; set; } = TextelB;
        public Vector2 TextelC { get; set; } = TextelC;

        public Vector3 NormalA { get; set; } = NormA;
        public Vector3 NormalB { get; set; } = NormB;
        public Vector3 NormalC { get; set; } = NormC;

        public Vector3 RealA { get; set; } = RealA;
        public Vector3 RealB { get; set; } = RealB;
        public Vector3 RealC { get; set; } = RealC;

    }
}
