using System.Numerics;

namespace ObjVisualizer.Data
{
    internal readonly struct LineSegment(Vector3 left, Vector3 right)
    {
        public readonly Vector3 Left = left;
        public readonly Vector3 Right = right;
    }
}
