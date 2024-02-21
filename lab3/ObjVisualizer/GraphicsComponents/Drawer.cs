using ObjVisualizer.Data;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Media.Media3D;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Drawer(int width, int height, IntPtr drawBuffer, int stride)
    {
        private readonly Random Random = new();
        private readonly List<List<double>> ZBuffer = Enumerable.Range(0, height)
            .Select(_ => Enumerable.Repeat(double.MaxValue, width).ToList())
            .ToList();

        private readonly IntPtr Buffer = drawBuffer;

        private readonly int _width = width;
        private readonly int _height = height;
        private readonly int _stride = stride;

        public unsafe void Rasterize(IList<Vector4> vertices, Color color)
        {
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                MyRasterizeTriangle(new(
                    new(vertices[0].X, vertices[0].Y, vertices[0].Z),
                    new(vertices[i].X, vertices[i].Y, vertices[i].Z),
                    new(vertices[i + 1].X, vertices[i + 1].Y, vertices[i + 1].Z),
                    new(),
                    new(),
                    new(),
                    new(),
                    new(),
                    new()), color);
            }
        }

        public unsafe void Rasterize(IList<Vector4> vertices, IList<Vector3> normales,IList<Vector4> originalVertexes, Scene scene)
        {
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                MyRasterizeTrianglePhong(new(
                    new(vertices[0].X, vertices[0].Y, vertices[0].Z),
                    new(vertices[i].X, vertices[i].Y, vertices[i].Z),
                    new(vertices[i + 1].X, vertices[i + 1].Y, vertices[i + 1].Z),
                    new(normales[0].X, normales[0].Y, normales[0].Z),
                    new(normales[i].X, normales[i].Y, normales[i].Z),
                    new(normales[i + 1].X, normales[i + 1].Y, normales[i + 1].Z),
                    new(originalVertexes[0].X, originalVertexes[0].Y, originalVertexes[0].Z),
                    new(originalVertexes[i].X, originalVertexes[i].Y, originalVertexes[i].Z),
                    new(originalVertexes[i+1].X, originalVertexes[i+1].Y, originalVertexes[i+1].Z)), scene);
            }
        }

        private static List<float> Interpolate(int i0, float d0, int i1, float d1)
        {
            if (i0 == i1)
            {
                return [d0];
            }

            var values = new List<float>();

            float a = (d1 - d0) / (i1 - i0);
            float d = d0;

            for (int i = i0; i <= i1; i++)
            {
                values.Add(d);
                d += a;
            }

            return values;
        }

        private unsafe void MyRasterizeTriangle(Triangle triangle, Color color)
        {
            //if (triangle.A.X > 0 && triangle.B.X > 0 && triangle.C.X > 0 && triangle.A.Y > 0 && triangle.B.Y > 0 && triangle.C.Y > 0
            //    && triangle.A.X < _width && triangle.B.X < _width && triangle.C.X < _width && triangle.A.Y < _height && triangle.B.Y < _height && triangle.C.Y < _height)
            if ((triangle.A.X > 0 && triangle.A.Y > 0 && triangle.A.X < _width && triangle.A.Y < _height) ||
            (triangle.B.X > 0 && triangle.B.Y > 0 && triangle.B.X < _width && triangle.B.Y < _height) ||
            (triangle.C.X > 0 && triangle.C.Y > 0 && triangle.C.X < _width && triangle.C.Y < _height))
            {
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
                int A = (int)float.Round(triangle.A.Y, 0);
                int B = (int)float.Round(triangle.B.Y, 0);
                int C = (int)float.Round(triangle.C.Y, 0);

                var x01 = Interpolate(A, triangle.A.X, B, triangle.B.X);
                var x12 = Interpolate(B, triangle.B.X, C, triangle.C.X);
                var x02 = Interpolate(A, triangle.A.X, C, triangle.C.X);

                var z01 = Interpolate(A, triangle.A.Z, B, triangle.B.Z);
                var z12 = Interpolate(B, triangle.B.Z, C, triangle.C.Z);
                var z02 = Interpolate(A, triangle.A.Z, C, triangle.C.Z);

                x01.RemoveAt(x01.Count - 1);
                var x012 = x01.Concat(x12).ToList();
                z01.RemoveAt(z01.Count - 1);
                var z012 = z01.Concat(z12).ToList();

                var m = (int)Math.Floor(x012.Count / 2.0f);
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
                int YDiffTop = 0;
                int YDiffTopI = 0;
                int TopY = (int)float.Round(triangle.A.Y, 0);
                if (triangle.A.Y < 0)
                {
                    YDiffTop = -(int)float.Round(triangle.A.Y, 0);
                    YDiffTopI = (int)float.Round(triangle.C.Y, 0);
                    TopY = 0;
                }

                for (int y = TopY; y <= (int)float.Round(triangle.C.Y, 0); y++)
                {
                    if (y < 0 || y >= _height)
                        continue;
                    var index = (y - TopY + YDiffTop);
                    if (index < x_left.Count && index < x_right.Count)
                    {
                        var xl = (int)float.Round(x_left[index], 0);
                        var xr = (int)float.Round(x_right[index], 0);
                        var zl = z_left[index];
                        var zr = z_right[index];
                        var zscan = Interpolate(xl, zl, xr, zr);
                        for (int x = xl; x < xr; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            var z = zscan[x - xl];
                            if (z < ZBuffer[y][x])
                            {
                                ZBuffer[y][x] = z;
                                byte* pixelPtr = data + y * _stride + x * 3;
                                *pixelPtr++ = color.B;
                                *pixelPtr++ = color.G;
                                *pixelPtr = color.R;
                            }

                        }

                    }


                }
            }
        }

        public List<Vector3> InterpolateNormals(Vector3 normal1, Vector3 normal2, int numPoints)
        {
            List<Vector3> interpolatedNormals = new List<Vector3>();
            float step = 1f / (numPoints);

            for (int i = 0; i < numPoints; i++)
            {
                float t = i * step;
                Vector3 interpolatedNormal = Vector3.Lerp(normal1, normal2, t);
                interpolatedNormal = Vector3.Normalize(interpolatedNormal);

                interpolatedNormals.Add(interpolatedNormal);
            }

            return interpolatedNormals;
        }

        public List<Vector3> InterpolateVerteces(Vector3 vertex1, Vector3 vertex2, int numPoints)
        {
            List<Vector3> interpolatedVerteces = new List<Vector3>();
            float step = 1f / (numPoints);

            for (int i = 0; i < numPoints; i++)
            {
                float t = i * step;
                Vector3 interpolatedVertex = Vector3.Lerp(vertex1, vertex1, t);
                interpolatedVerteces.Add(interpolatedVertex);
            }

            return interpolatedVerteces;
        }

        public Color CalculateColor(Vector3 point, Vector3 normal, Scene scene, Color baseColor)
        {
            var light = scene.Light.CalculateLight(point, normal);
            var color = Color.FromArgb(
                (byte)(light * baseColor.R> 255 ? 255 : light * baseColor.R),
                (byte)(light * baseColor.G > 255 ? 255 : light * baseColor.G),
                (byte)(light * baseColor.B > 255 ? 255 : light * baseColor.B));
            return color;
        }
        private unsafe void MyRasterizeTrianglePhong(Triangle triangle, Scene scene)
        {
            if ((triangle.A.X > 0 && triangle.A.Y > 0 && triangle.A.X < _width && triangle.A.Y < _height) ||
            (triangle.B.X > 0 && triangle.B.Y > 0 && triangle.B.X < _width && triangle.B.Y < _height) ||
            (triangle.C.X > 0 && triangle.C.Y > 0 && triangle.C.X < _width && triangle.C.Y < _height))
            {
                Color baseColor = Color.Green;
                byte* data = (byte*)Buffer.ToPointer();
                if (triangle.B.Y < triangle.A.Y)
                {
                    (triangle.B, triangle.A) = (triangle.A, triangle.B);
                    (triangle.NormalB, triangle.NormalA) = (triangle.NormalA, triangle.NormalB);
                    (triangle.RealB, triangle.RealA) = (triangle.RealA, triangle.RealB);
                }
                if (triangle.C.Y < triangle.A.Y)
                {
                    (triangle.C, triangle.A) = (triangle.A, triangle.C);
                    (triangle.NormalC, triangle.NormalA) = (triangle.NormalA, triangle.NormalC);
                    (triangle.RealC, triangle.RealA) = (triangle.RealA, triangle.RealC);
                }
                if (triangle.C.Y < triangle.B.Y)
                {
                    (triangle.B, triangle.C) = (triangle.C, triangle.B);
                    (triangle.NormalB, triangle.NormalC) = (triangle.NormalC, triangle.NormalB);
                    (triangle.RealB, triangle.RealC) = (triangle.RealC, triangle.RealB);
                }
                int YA = (int)float.Round(triangle.A.Y, 0);
                int YB = (int)float.Round(triangle.B.Y, 0);
                int YC = (int)float.Round(triangle.C.Y, 0);
                
                var x01 = Interpolate(YA, triangle.A.X, YB, triangle.B.X);
                var x12 = Interpolate(YB, triangle.B.X, YC, triangle.C.X);
                var x02 = Interpolate(YA, triangle.A.X, YC, triangle.C.X);

                var z01 = Interpolate(YA, triangle.A.Z, YB, triangle.B.Z);
                var z12 = Interpolate(YB, triangle.B.Z, YC, triangle.C.Z);
                var z02 = Interpolate(YA, triangle.A.Z, YC, triangle.C.Z);

                var nx01 = InterpolateNormals(triangle.NormalA, triangle.NormalB, x01.Count);
                var nx12 = InterpolateNormals(triangle.NormalB, triangle.NormalC, x12.Count);
                var nx02 = InterpolateNormals(triangle.NormalA, triangle.NormalC, x02.Count);

                var r01 = InterpolateVerteces(triangle.RealA, triangle.RealB, x01.Count);
                var r12 = InterpolateVerteces(triangle.RealB, triangle.RealC, x12.Count);
                var r02 = InterpolateVerteces(triangle.RealA, triangle.RealC, x02.Count);

                x01.RemoveAt(x01.Count - 1);
                
                var nx012 = nx01.Concat(nx12).ToList();
                var x012 = x01.Concat(x12).ToList();

              

                var r012 = r01.Concat(r12).ToList();


                z01.RemoveAt(z01.Count - 1);
                var z012 = z01.Concat(z12).ToList();

                var m = (int)Math.Floor(x012.Count / 2.0f);
                List<float> x_left;
                List<float> x_right;
                List<float> z_left;
                List<float> z_right;
                List<Vector3> n_right;
                List<Vector3> n_left;
                List<Vector3> r_right;
                List<Vector3> r_left;
                if (x02[m] < x012[m])
                {
                    (x_left, x_right) = (x02, x012);
                    (z_left, z_right) = (z02, z012);
                    (n_left, n_right) = (nx02, nx012);
                    (r_left, r_right) = (r02, r012);

                }
                else
                {
                    (x_left, x_right) = (x012, x02);
                    (z_left, z_right) = (z012, z02);
                    (n_left, n_right) = (nx012, nx02);
                    (r_left, r_right) = (r012, r02);

                }
                int YDiffTop = 0;
                int YDiffTopI = 0;
                int TopY = (int)float.Round(triangle.A.Y, 0);
                if (triangle.A.Y < 0)
                {
                    YDiffTop = -(int)float.Round(triangle.A.Y, 0);
                    YDiffTopI = (int)float.Round(triangle.C.Y, 0);
                    TopY = 0;
                }
                List<Vector3> normales = new List<Vector3>();
                List<Vector3> lineReal = new List<Vector3>();
                for (int y = TopY; y <= (int)float.Round(triangle.C.Y, 0); y++)
                {
                    if (y < 0 || y >= _height)
                        continue;
                    var index = (y - TopY + YDiffTop);
                    //if (index < x_left.Count && index < x_right.Count)
                    {
                        var xl = (int)float.Round(x_left[index], 0);
                        var xr = (int)float.Round(x_right[index], 0);
                        var zl = z_left[index];
                        var zr = z_right[index];
                        var zscan = Interpolate(xl, zl, xr, zr);
                        normales = InterpolateNormals(n_left[index], n_right[index], xr - xl);
                        lineReal = InterpolateVerteces(r_left[index], r_right[index], xr - xl);
                        for (int x = xl; x < xr; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            var z = zscan[x - xl];
                            if (z < ZBuffer[y][x])
                            {
                                ZBuffer[y][x] = z;
                                byte* pixelPtr = data + y * _stride + x * 3;
                                Color color = CalculateColor(lineReal[x-xl], normales[x-xl], scene, baseColor);
                                *pixelPtr++ = color.B;
                                *pixelPtr++ = color.G;
                                *pixelPtr = color.R;
                            }

                        }

                    }


                }
            }
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
                    DrawLine(line.Left, line.Right, (byte*)Buffer.ToPointer(), Color.White);
                }
            }
        }

        public unsafe void DrawLine(Vector3 p1, Vector3 p2, byte* data, Color color)
        {
            int x1 = (int)p1.X;
            int y1 = (int)p1.Y;
            float z1 = p1.Z;
            int x2 = (int)p2.X;
            int y2 = (int)p2.Y;
            float z2 = p2.Z;

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
                    byte* pixelPtr = data + row * _stride + col * 3;

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
