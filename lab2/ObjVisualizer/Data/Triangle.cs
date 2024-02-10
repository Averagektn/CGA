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
            float minX = Math.Min(Math.Min(A.X, B.X), C.X);
            float maxX = Math.Max(Math.Max(A.X, B.X), C.X);

            for (float x = minX; x <= maxX; x += 1f)
            {
                Vector3 start = GetIntersectionPoint(x);
                Vector3 end = GetIntersectionPoint(x, true);

                yield return new LineSegment(start, end);
            }
        }

        public IEnumerable<LineSegment> GetHorizontalLines()
        {
            float minY = Math.Min(Math.Min(A.Y, B.Y), C.Y);
            float maxY = Math.Max(Math.Max(A.Y, B.Y), C.Y);

            for (float y = minY; y <= maxY; y += 1f)
            {
                Vector3 start = GetIntersectionPoint(y);
                Vector3 end = GetIntersectionPoint(y, true);

                yield return new LineSegment(start, end);
            }
        }

        private Vector3 GetIntersectionPoint(float y, bool reverse = false)
        {
            Vector3 p1 = reverse ? B : A;
            Vector3 p2 = C;

            float t = (y - p1.Y) / (p2.Y - p1.Y);

            t = Math.Clamp(t, 0f, 1f);

            float x = p1.X + t * (p2.X - p1.X);
            float z = p1.Z + t * (p2.Z - p1.Z);

            return new Vector3(x, y, z);
        }
    }
}
