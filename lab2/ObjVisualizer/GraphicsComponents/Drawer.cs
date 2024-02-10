using ObjVisualizer.Data;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Drawer(int width, int height, IntPtr drawBuffer, int stride)
    {
        private readonly int _width = width;
        private readonly int _height = height;

        private readonly List<List<int>> ZBuffer =
            Enumerable.Repeat(Enumerable.Repeat(int.MaxValue, height).ToList(), width).ToList();

        private readonly IntPtr Buffer = drawBuffer;

        private readonly int Stride = stride;

        public unsafe void Rasterize(IList<Vector4> vertices)
        {
            // uncomment after triangulation implemented
            /*            foreach (var triangle in GetTriangles(vertices))
                        {
                            RasterizeTriangle(triangle);
                        }*/

            RasterizeTriangle(new(
                new(vertices[0].X, vertices[0].Y, vertices[0].Z), 
                new(vertices[1].X, vertices[1].Y, vertices[1].Z), 
                new(vertices[2].X, vertices[2].Y, vertices[2].Z)
                ));
        }

        private unsafe void RasterizeTriangle(Triangle triangle)
        {
            foreach(var line in triangle.GetHorizontalLines())
            {
                if (line.Left.X > 0 && line.Left.Y > 0 &&
                    line.Right.X > 0 && line.Right.Y > 0 &&
                    line.Left.X < _width && line.Left.Y < _height &&
                    line.Right.X < _width && line.Right.Y < _height)
                {
                    DrawLine((int)line.Left.X, (int)line.Left.Y, (int)line.Right.X, (int)line.Right.Y, 
                        (byte*)Buffer.ToPointer());
                }
            }
        }

        private IEnumerable<Triangle> GetTriangles(IEnumerable<Vector4> points)
        {
            throw new NotImplementedException();
        }

        private unsafe void DrawLine(int x0, int y0, int x1, int y1, byte* data)
        {
            bool step = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (step)
            {
                (x0, y0) = (y0, x0);
                (x1, y1) = (y1, x1);
            }

            if (x0 > x1)
            {
                (x0, x1) = (x1, x0);
                (y0, y1) = (y1, y0);
            }

            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            int var1, var2;

            for (int x = x0; x <= x1; x++)
            {
                if (step)
                {
                    var1 = x;
                    var2 = y;
                }
                else
                {
                    var1 = y;
                    var2 = x;
                }

                byte* pixelPtr = data + var1 * Stride + var2 * 3;

                *pixelPtr++ = 255;
                *pixelPtr++ = 255;
                *pixelPtr = 255;

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
