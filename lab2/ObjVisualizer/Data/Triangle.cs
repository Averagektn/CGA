using System.Numerics;
using System.Windows.Media.Media3D;

namespace ObjVisualizer.Data
{
    internal readonly struct Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        public readonly Vector3 A = a;
        public readonly Vector3 B = b;
        public readonly Vector3 C = c;

        public IEnumerable<LineSegment> GetHorizontalLines()
        {
            float minY = Math.Min(Math.Min(A.Y, B.Y), C.Y);
            float maxY = Math.Max(Math.Max(A.Y, B.Y), C.Y);

            for (float y = minY; y <= maxY; y += 0.1f)
            {
                yield return FindIntersectingSegment(A, B, C, y);
            }
        }

        public IEnumerable<LineSegment> GetVerticalLines()
        {
            float minX = Math.Min(Math.Min(A.X, B.X), C.X);
            float maxX = Math.Max(Math.Max(A.X, B.X), C.X);

            for (float x = minX; x <= maxX; x += 0.1f)
            {
                yield return FindIntersectingSegment(A, B, C, x);
            }
        }


        public static LineSegment FindIntersectingSegment(Vector3 point1, Vector3 point2, Vector3 point3, float y)
        {
            Vector3[] trianglePoints = { point1, point2, point3 };
            LineSegment intersectingSegment = new LineSegment(Vector3.Zero, Vector3.Zero);

            for (int i = 0; i < 3; i++)
            {
                Vector3 currentPoint = trianglePoints[i];
                Vector3 nextPoint = trianglePoints[(i + 1) % 3];

                if ((currentPoint.Y <= y && nextPoint.Y >= y) || (currentPoint.Y >= y && nextPoint.Y <= y))
                {
                    float t = (y - currentPoint.Y) / (nextPoint.Y - currentPoint.Y);
                    Vector3 intersectionPoint = currentPoint + t * (nextPoint - currentPoint);

                    if (intersectingSegment.Left == Vector3.Zero)
                    {
                        intersectingSegment.Left = intersectionPoint;
                    }
                    else
                    {
                        intersectingSegment.Right = intersectionPoint;
                        break;
                    }
                }
            }

            return intersectingSegment;
        }
    }
}
