using ObjVisualizer.Data;
using System.Drawing;
using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Drawer(int width, int height, IntPtr drawBuffer, int stride)
    {
        private readonly int _width = width;
        private readonly int _height = height;

        private readonly Random Random = new();

        private readonly List<List<double>> ZBuffer = Enumerable.Range(0, height)
            .Select(_ => Enumerable.Repeat(double.MaxValue, width).ToList())
            .ToList();

        private readonly IntPtr Buffer = drawBuffer;

        private readonly int Stride = stride;

        public unsafe void Rasterize(IList<Vector4> vertices, IList<Vector4> preProjection)
        {
            RasterizeTriangle(new(
                new(vertices[0].X, vertices[0].Y, vertices[0].Z),
                new(vertices[1].X, vertices[1].Y, vertices[1].Z),
                new(vertices[2].X, vertices[2].Y, vertices[2].Z)), 
                preProjection);
        }

        private unsafe void RasterizeTriangle(Triangle triangle, IList<Vector4> preProjection)
        {
            foreach (var line in triangle.GetHorizontalLines())
            {
                if (line.Left.X > 0 && line.Left.Y > 0 &&
                    line.Right.X > 0 && line.Right.Y > 0 &&
                    line.Left.X < _width && line.Left.Y < _height &&
                    line.Right.X < _width && line.Right.Y < _height)
                {
                    DrawLine(line.Left, line.Right, (byte*)Buffer.ToPointer(), Color.White);
                }
            }
        }

        private IEnumerable<Triangle> GetTriangles(IEnumerable<Vector4> points)
        {
            throw new NotImplementedException();
        }

        public unsafe void DrawLine(Vector3 p1, Vector3 p2, byte* data, Color color)
        {
            int x1 = (int)p1.X;
            int y1 = (int)p1.Y;
            var z1 = p1.Z;
            int x2 = (int)p2.X;
            int y2 = (int)p2.Y;
            var z2 = p2.Z;

            var zDiff = z1 - z2;
            var distance = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            var zStep = distance == 0 ? 0 : zDiff / distance;

            bool step = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);

            if (step)
            {
                (x1, y1) = (y1, x1);
                (x2, y2) = (y2, x2);
            }

            if (x1 > x2)
            {
                (x1, x2) = (x2, x1);
                (y1, y2) = (y2, y1);
            }

            int dx = x2 - x1;
            int dy = Math.Abs(y2 - y1);
            int error = dx / 2;
            int ystep = (y1 < y2) ? 1 : -1;
            int y = y1;
            int row, col;

            for (int x = x1; x <= x2; x++)
            {
                if (step)
                {
                    row = x;
                    col = y;
                }
                else
                {
                    row = y;
                    col = x;
                }

                if (ZBuffer[row][col] > z1 + zStep * (x - x1))
                {
                    byte* pixelPtr = data + row * Stride + col * 3;

                    ZBuffer[row][col] = z1 + zStep * (x - x1);

                    *pixelPtr++ = color.B;
                    *pixelPtr++ = color.G;
                    *pixelPtr = color.R;
                }

                error -= dy;

                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }
    }
}
