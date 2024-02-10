using System.Numerics;

namespace ObjVisualizer.Data
{
    internal readonly struct Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        public readonly Vector3 A = a;
        public readonly Vector3 B = b;
        public readonly Vector3 C = c;

        public IEnumerable<LineSegment> GetVerticalLines()
        {
            return [];
        }

        public IEnumerable<LineSegment> GetHorizontalLines()
        {
            return [];
        }
    }
}
