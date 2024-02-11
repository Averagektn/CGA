using ObjVisualizer.Data;
using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Drawer(int width, int height, IntPtr drawBuffer, int stride)
    {
        private readonly int _width = width;
        private readonly int _height = height;

        private readonly List<List<int>> ZBuffer =
            Enumerable.Repeat(Enumerable.Repeat(int.MaxValue, width).ToList(), height).ToList();

        private readonly IntPtr Buffer = drawBuffer;

        private readonly int Stride = stride;

        public unsafe void Rasterize(IList<Vector4> vertices, Vector4[] preProjection)
        {
            // uncomment after triangulation implemented
            /*            foreach (var triangle in GetTriangles(vertices))
                        {
                            RasterizeTriangle(triangle);
                        }*/

            RasterizeTriangle(new(
                new(vertices[0].X, vertices[0].Y, preProjection[0].Z),
                new(vertices[1].X, vertices[1].Y, preProjection[1].Z),
                new(vertices[2].X, vertices[2].Y, preProjection[2].Z)
                ));
        }

        private unsafe void RasterizeTriangle(Triangle triangle)
        {
            foreach (var line in triangle.GetHorizontalLines())
            {
                if (line.Left.X > 0 && line.Left.Y > 0 &&
                    line.Right.X > 0 && line.Right.Y > 0 &&
                    line.Left.X < _width && line.Left.Y < _height &&
                    line.Right.X < _width && line.Right.Y < _height)
                {
                    DrawLine(line.Left, line.Right, (byte*)Buffer.ToPointer());
                }
            }

            /*foreach (var line in triangle.GetVerticalLines())
            {
                if (line.Left.X > 0 && line.Left.Y > 0 &&
                    line.Right.X > 0 && line.Right.Y > 0 &&
                    line.Left.X < _width && line.Left.Y < _height &&
                    line.Right.X < _width && line.Right.Y < _height)
                {
                    DrawLine(line.Left, line.Right, (byte*)Buffer.ToPointer());
                }
            }*/
        }

        private IEnumerable<Triangle> GetTriangles(IEnumerable<Vector4> points)
        {
            throw new NotImplementedException();
        }

        private unsafe void DrawLine(Vector3 p1, Vector3 p2, byte* data)
        {
            var zDiff = p1.Z - p2.Z;
            var distance = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            var zStep = distance == 0 ? 0 : zDiff / distance;

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

            int dx = (int)(p2.X - p1.X);
            int dy = (int)Math.Abs(p2.Y - p1.Y);
            int error = dx / 2;
            int ystep = (p1.Y < p2.Y) ? 1 : -1;
            int y = (int)p1.Y;
            int row, col;

            for (int x = (int)p1.X; x <= p2.X; x++)
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

                if (ZBuffer[row][col] > p1.Z + zStep * (int)(x - p1.X))
                {
                    *pixelPtr++ = 255;
                    *pixelPtr++ = 255;
                    *pixelPtr = 255;

                    ZBuffer[row][col] = (int)(p1.Z + zStep * (int)(x - p1.X));
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
