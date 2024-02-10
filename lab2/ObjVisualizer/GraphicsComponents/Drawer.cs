using ObjVisualizer.Data;
using System.Drawing;
using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Drawer(int width, int height, IntPtr drawBuffer, int stride)
    {
        private readonly List<List<int>> ZBuffer =
            Enumerable.Repeat(Enumerable.Repeat(int.MaxValue, height).ToList(), width).ToList();

        private IntPtr Buffer = drawBuffer;

        private readonly int Stride = stride;

        public unsafe void Rasterize(IList<Vector4> vertices)
        {
            foreach (var triangle in GetTriangles(vertices))
            {
                RasterizeTriangle(triangle);
            }
        }

        private void RasterizeTriangle(Triangle triangle)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Triangle> GetTriangles(IEnumerable<Vector4> points)
        {
            throw new NotImplementedException();
        }

        private unsafe void DrawLine(Point p1, Point p2, byte* data)
        {
            bool step = Math.Abs(p2.Y - p1.Y) > Math.Abs(p2.X - p1.X);

            if (step)
            {
                (p1.X, p1.Y) = (p1.Y, p1.X);
                (p2.X, p2.Y) = (p2.Y, p2.X);
            }

            if (p1.X > p2.X)
            {
                (p1.X, p2.X) = (p2.X, p1.X);
                (p1.Y, p2.Y) = (p2.Y, p1.Y);
            }

            int dx = p2.X - p1.X;
            int dy = Math.Abs(p2.Y - p1.Y);
            int error = dx / 2;
            int ystep = (p1.Y < p2.Y) ? 1 : -1;
            int y = p1.Y;
            int row, col;

            for (int x = p1.X; x <= p2.X; x++)
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

                byte* pixelPtr = data + row * Stride + col * 3;

                *pixelPtr++ = 255;
                *pixelPtr++ = 255;
                *pixelPtr++ = 255;

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
