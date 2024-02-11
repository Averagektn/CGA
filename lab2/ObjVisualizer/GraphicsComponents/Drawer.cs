﻿using ObjVisualizer.Data;
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

        private Random random = new Random();
        public unsafe void Rasterize(IList<Vector4> vertices)
        {
            MyRasterizeTriangle(new(
                new(vertices[0].X, vertices[0].Y, vertices[0].Z),
                new(vertices[1].X, vertices[1].Y, vertices[1].Z),
                new(vertices[2].X, vertices[2].Y, vertices[2].Z)));
        }

        private List<float> Interpolate(float i0, float d0, float i1, float d1)
        {
            if (i0 == i1) return new List<float>() { d0 };

            var values = new List<float>();
            float a = (d1 - d0) / (i1 - i0);
            float d = d0;
            for (int i = (int)i0; i < i1; i++)
            {
                values.Add(d);
                d += a;
            }
            return values;
        }

        private unsafe void MyRasterizeTriangle(Triangle triangle)
        {
            if (triangle.A.X > 0 && triangle.B.X > 0 && triangle.C.X > 0 && triangle.A.Y > 0 && triangle.B.Y > 0 && triangle.C.Y > 0
                && triangle.A.X < _width && triangle.B.X < _width && triangle.C.X < _width && triangle.A.Y < _height && triangle.B.Y < _height && triangle.C.Y < _height)
            {
                Color color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                //Color color = Color.White;
                byte* data = (byte*)Buffer.ToPointer();
                if (triangle.B.Y < triangle.A.Y)
                {
                    (triangle.B, triangle.A) = (triangle.A, triangle.B);
                }
                if (triangle.C.Y < triangle.A.Y)
                {
                    (triangle.C, triangle.A) = (triangle.A, triangle.C);
                }
                if (triangle.C.Y < triangle.B.Y)
                {
                    (triangle.B, triangle.C) = (triangle.C, triangle.B);
                }

                var x01 = Interpolate(triangle.A.Y, triangle.A.X,triangle.B.Y, triangle.B.X);
                var x12 = Interpolate(triangle.B.Y, triangle.B.X,triangle.C.Y, triangle.C.X);
                var x02 = Interpolate(triangle.A.Y, triangle.A.X,triangle.C.Y, triangle.C.X);

                var z01 = Interpolate(triangle.A.Y, triangle.A.Z,triangle.B.Y, triangle.B.Z);
                var z12 = Interpolate(triangle.B.Y, triangle.B.Z,triangle.C.Y, triangle.C.Z);
                var z02 = Interpolate(triangle.A.Y, triangle.A.Z,triangle.C.Y, triangle.C.Z);

                x01.RemoveAt(x01.Count - 1);
                var x012 = x01.Concat(x12).ToList();
                z01.RemoveAt(z01.Count - 1);
                var z012 = z01.Concat(z12).ToList();

                var m = (int)Math.Floor(x012.Count / 2.0);
                List<float> x_left;
                List<float> x_right;
                List<float> z_left;
                List<float> z_right;
                if (x02[m] < x012[m])
                {
                    (x_left, x_right) = (x02, x012);
                    (z_left, z_right) = (z02, z012);
                }
                else
                {
                    (x_left, x_right) = (x012, x02);
                    (z_left, z_right) = (z012, z02);

                }

                for (int y = (int)triangle.A.Y; y < triangle.C.Y; y++)
                {
                    var index = (int)(y - triangle.A.Y);
                    if (index < x_left.Count && index < x_right.Count)
                    {
                        var xl = (int)x_left[index];
                        var xr = (int)x_right[index];
                        var zl = z_left[index];
                        var zr = z_right[index];
                        var zscan = Interpolate(xl, zl, xr, zr);
                        for (int x = xl; x < xr; x++)
                        {
                            var z = zscan[x - xl];
                            if (z < ZBuffer[y][x])
                            {
                                ZBuffer[y][x] = z;
                                byte* pixelPtr = data + y * Stride + x * 3;
                                *pixelPtr++ = color.R;
                                *pixelPtr++ = color.G;
                                *pixelPtr = color.B;
                            }


                        }

                    }


                }
            }

        }

        private unsafe void RasterizeTriangle(Triangle triangle)
        {
            Color color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
            //if (preProjection[0].Z>0 && preProjection[1].Z > 0 && preProjection[2].Z > 0)
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
