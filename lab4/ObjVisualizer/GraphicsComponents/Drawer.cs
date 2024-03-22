using ObjVisualizer.Data;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Drawer(int width, int height, IntPtr drawBuffer, int stride)
    {
        private readonly Random Random = new();
        private readonly List<List<double>> ZBuffer = Enumerable.Range(0, height)
            .Select(_ => Enumerable.Repeat(double.MaxValue, width).ToList())
            .ToList();
        private readonly List<List<Object>> SpincLocker = Enumerable.Range(0, height)
           .Select(_ => Enumerable.Repeat(new Object(), width).ToList())
           .ToList();
        private bool[,] SpincLockerBoolean = new bool[height, width];

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
                    new(),
                    new(),
                    new(),
                    new(),
                    new(),
                    new(),
                    new()), color);
            }
        }

        public unsafe void Rasterize(IList<Vector4> vertices, IList<Vector3> normales, IList<Vector4> originalVertexes, Scene scene)
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
                    new(originalVertexes[i + 1].X, originalVertexes[i + 1].Y, originalVertexes[i + 1].Z),
                    new(),
                    new(),
                    new(),
                    new(),
                    new(),
                    new()), scene);
            }
        }

        public unsafe void Rasterize(IList<Vector4> vertices, IList<Vector4> originalVertexes, IList<Vector3> textels,IList<Vector4> view, Scene scene)
        {
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                MyRasterizeTriangleTexture(new(
                    new(vertices[0].X, vertices[0].Y, vertices[0].Z),
                    new(vertices[i].X, vertices[i].Y, vertices[i].Z),
                    new(vertices[i + 1].X, vertices[i + 1].Y, vertices[i + 1].Z),
                    new(),
                    new(),
                    new(),
                    new(originalVertexes[0].X, originalVertexes[0].Y, originalVertexes[0].Z),
                    new(originalVertexes[i].X, originalVertexes[i].Y, originalVertexes[i].Z),
                    new(originalVertexes[i + 1].X, originalVertexes[i + 1].Y, originalVertexes[i + 1].Z),
                    new(textels[0].X, textels[0].Y),
                    new(textels[i].X, textels[i].Y),
                    new(textels[i + 1].X, textels[i + 1].Y),
                    view[0],
                    view[i],
                    view[i+1]), scene);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<float> InterpolateTexture(int i0, float d0, int i1, float d1)
        {
            if (i0 == i1)
            {
                return [d0];
            }

            var values = new List<float>();

            float a = (d1 - d0) / (i1 - i0);
            float d = d0;

            for (int i = i0; i < i1; i++)
            {
                values.Add(d);
                d += a;
            }
            values.Add(d);
            d += a;

            return values;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


                (var x02, var x012) = TraingleInterpolation(A, triangle.A.X, B, triangle.B.X, C, triangle.C.X);
                (var z02, var z012) = TraingleInterpolation(A, triangle.A.Z, B, triangle.B.Z, C, triangle.C.Z);

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

        private unsafe void MyRasterizeTriangleTexture(Triangle triangle, Scene scene)
        {
            if ((triangle.A.X > 0 && triangle.A.Y > 0 && triangle.A.X < _width && triangle.A.Y < _height) ||
            (triangle.B.X > 0 && triangle.B.Y > 0 && triangle.B.X < _width && triangle.B.Y < _height) ||
            (triangle.C.X > 0 && triangle.C.Y > 0 && triangle.C.X < _width && triangle.C.Y < _height))
            {
                Color baseColor = Color.White;
                byte* data = (byte*)Buffer.ToPointer();
                if (triangle.B.Y < triangle.A.Y)
                {
                    (triangle.B, triangle.A) = (triangle.A, triangle.B);
                    (triangle.RealB, triangle.RealA) = (triangle.RealA, triangle.RealB);
                    (triangle.TextelB, triangle.TextelA) = (triangle.TextelA, triangle.TextelB);
                    (triangle.ViewB, triangle.ViewA) = (triangle.ViewA, triangle.ViewB);

                }
                if (triangle.C.Y < triangle.A.Y)
                {
                    (triangle.C, triangle.A) = (triangle.A, triangle.C);
                    (triangle.RealC, triangle.RealA) = (triangle.RealA, triangle.RealC);
                    (triangle.TextelC, triangle.TextelA) = (triangle.TextelA, triangle.TextelC);
                    (triangle.ViewC, triangle.ViewA) = (triangle.ViewA, triangle.ViewC);


                }
                if (triangle.C.Y < triangle.B.Y)
                {
                    (triangle.B, triangle.C) = (triangle.C, triangle.B);
                    (triangle.RealB, triangle.RealC) = (triangle.RealC, triangle.RealB);
                    (triangle.TextelB, triangle.TextelC) = (triangle.TextelC, triangle.TextelB);
                    (triangle.ViewB, triangle.ViewC) = (triangle.ViewC, triangle.ViewB);


                }
                int YA = (int)float.Round(triangle.A.Y, 0);
                int YB = (int)float.Round(triangle.B.Y, 0);
                int YC = (int)float.Round(triangle.C.Y, 0);

                float ZInvA = 1 / triangle.ViewA.W;
                float ZInvB = 1 / triangle.ViewB.W;
                float ZInvC = 1 / triangle.ViewC.W;

                (var z02, var z012) = TraingleInterpolation(YA, triangle.A.Z, YB, triangle.B.Z, YC, triangle.C.Z);
                (var x02, var x012) = TraingleInterpolation(YA, triangle.A.X, YB, triangle.B.X, YC, triangle.C.X);

                //(var rx02, var rx012) = TraingleInterpolation(YA, triangle.RealA.X, YB, triangle.RealB.X, YC, triangle.RealC.X);
                //(var ry02, var ry012) = TraingleInterpolation(YA, triangle.RealA.Y, YB, triangle.RealB.Y, YC, triangle.RealC.Y);
               
                //(var rz02, var rz012) = TraingleInterpolation(YA, triangle.RealA.Z, YB, triangle.RealB.Z, YC, triangle.RealC.Z);

                (var vz02, var vz012) = TraingleInterpolation(YA, ZInvA, YB, ZInvB, YC, ZInvC);


                (var u02, var u012) = TraingleInterpolationTexture(YA, triangle.TextelA.X*ZInvA, YB, triangle.TextelB.X*ZInvB, YC, triangle.TextelC.X*ZInvC);
                (var v02, var v012) = TraingleInterpolationTexture(YA, triangle.TextelA.Y*ZInvA, YB, triangle.TextelB.Y*ZInvB, YC, triangle.TextelC.Y*ZInvC);

                var m = (int)float.Floor(x012.Count / 2.0f);
                List<float> x_left;
                List<float> x_right;
                List<float> z_left;
                List<float> z_right;
                List<float> u_left;
                List<float> u_right;
                List<float> v_left;
                List<float> v_right; 
                List<float> vz_left;
                List<float> vz_right;


                //List<float> rx_right, ry_right, rz_right;
                //List<float> rx_left, ry_left, rz_left;

                if ((int)float.Round(x02[m]) <= (int)float.Round(x012[m]))
                {
                    (x_left, x_right) = (x02, x012);
                    (z_left, z_right) = (z02, z012);

                    (u_left, u_right) = (u02, u012);
                    (v_left, v_right) = (v02, v012);
                    (vz_left, vz_right) = (vz02, vz012);



                    //(rx_left, rx_right) = (rx02, rx012);
                    //(ry_left, ry_right) = (ry02, ry012);
                    //(rz_left, rz_right) = (rz02, rz012);

                }
                else
                {
                    (x_left, x_right) = (x012, x02);
                    (z_left, z_right) = (z012, z02);

                    (u_left, u_right) = (u012, u02);
                    (v_left, v_right) = (v012, v02);
                    (vz_left, vz_right) = (vz012, vz02);



                    //(rx_left, rx_right) = (rx012, rx02);
                    //(ry_left, ry_right) = (ry012, ry02);
                    //(rz_left, rz_right) = (rz012, rz02);

                }
                int YDiffTop = 0;
                int YDiffTopI = 0;
                int TopY = (int)float.Round(triangle.A.Y);
                if (triangle.A.Y < 0)
                {
                    YDiffTop = -(int)float.Round(triangle.A.Y);
                    YDiffTopI = (int)float.Round(triangle.C.Y);
                    TopY = 0;
                }
                for (int y = TopY; y <= (int)float.Round(triangle.C.Y); y++)
                {
                    if (y < 0 || y >= _height)
                        continue;
                    var index = (y - TopY + YDiffTop);
                    {
                        var xl = (int)float.Round(x_left[index] );
                        var xr = (int)float.Round(x_right[index]);
                        var zl = z_left[index];
                        var zr = z_right[index];


                        //(var rxl, var rxr) = (rx_left[index], rx_right[index]);
                        //(var ryl, var ryr) = (ry_left[index], ry_right[index]);
                        //(var rzl, var rzr) = (rz_left[index], rz_right[index]);

                        (var vzl, var vzr) = (vz_left[index], vz_right[index]);

                        (var ul, var ur) = (u_left[index], u_right[index]);
                        (var vl, var vr) = (v_left[index], v_right[index]);

                        var zscan = Interpolate(xl, zl, xr, zr);
                        if (zscan.Count == 0)
                            continue;
                        var uscan = InterpolateTexture(xl, ul, xr, ur);
                        var vscan = InterpolateTexture(xl, vl, xr, vr);

                        var vzscan = Interpolate(xl, vzl, xr, vzr);

                        //var rxscan = Interpolate(xl, rxl, xr, rxr);
                        //var ryscan = Interpolate(xl, ryl, xr, ryr);
                        //var rzscan = Interpolate(xl, rzl, xr, rzr);


                        for (int x = xl; x <= xr; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            var z = zscan[x - xl];
                            var vz = vzscan[x - xl];

                            lock (SpincLocker[y][x])
                            {
                                if (z < ZBuffer[y][x])
                                {
                                   
                                    ZBuffer[y][x] = z;
                                    byte* pixelPtr = data + y * _stride + x * 3;
                                    //var vertex = new Vector3(rxscan[x - xl], ryscan[x - xl], rzscan[x - xl]);
                                    var tx = float.Abs((uscan[x-xl]/vz) * scene.GraphicsObjects.KdMap.Width)% scene.GraphicsObjects.KdMap.Width;
                                    var ty = float.Abs((1-vscan[x-xl] / vz) * scene.GraphicsObjects.KdMap.Height)% scene.GraphicsObjects.KdMap.Height;
                                    //int textureByteKd = (int)((1 - vscan[x - xl]) * scene.GraphicsObjects.KdMap.Height) * scene.GraphicsObjects.KdMap.Stride + (int)(uscan[x - xl] * scene.GraphicsObjects.KdMap.Width) * scene.GraphicsObjects.KdMap.ColorSize / 8;
                                    Vector3 newColor = GetNewTextel(tx, ty, scene.GraphicsObjects.KdMap);
                                    ////Vector3 newColor = new(scene.GraphicsObjects.KdMap.MapData[textureByteKd], scene.GraphicsObjects.KdMap.MapData[textureByteKd + 1], scene.GraphicsObjects.KdMap.MapData[textureByteKd + 2]);
                                    //int textureByteNorm = (int)((1 - vscan[x - xl]) * scene.GraphicsObjects.NormMap.Height) * scene.GraphicsObjects.NormMap.Stride + (int)(uscan[x - xl] * scene.GraphicsObjects.NormMap.Width) * scene.GraphicsObjects.NormMap.ColorSize / 8;
                                    //int textureByteMrao = (int)((1 - vscan[x - xl]) * scene.GraphicsObjects.MraoMap.Height) * scene.GraphicsObjects.MraoMap.Stride + (int)(uscan[x - xl] * scene.GraphicsObjects.MraoMap.Width) * scene.GraphicsObjects.MraoMap.ColorSize / 8;
                                    //Vector3 normal = new Vector3((scene.GraphicsObjects.NormMap.MapData[textureByteNorm + 2] / 255.0f) * 2 - 1, (scene.GraphicsObjects.NormMap.MapData[textureByteNorm + 1] / 255.0f) * 2 - 1, (scene.GraphicsObjects.NormMap.MapData[textureByteNorm + 0] / 255.0f) * 2 - 1);
                                    //Vector3 lightResult = new(0, 0, 0);
                                    //for (int i = 0; i < scene.Light.Count; i++)
                                    //    lightResult += scene.Light[i].CalculateLightWithMaps(vertex, normal, scene.Camera.Eye, scene.GraphicsObjects.MraoMap.MapData[textureByteMrao + 0]);
                                    Vector3 lightResult = new(1f, 1f, 1f);
                                    *pixelPtr++ = (byte)(newColor.X * (lightResult.X > 1.0 ? 1 : lightResult.X));
                                    *pixelPtr++ = (byte)(newColor.Y * (lightResult.Y > 1 ? 1 : lightResult.Y));
                                    *pixelPtr = (byte)(newColor.Z * (lightResult.Z > 1 ? 1 : lightResult.Z));
                                }
                            }

                        }

                    }


                }
            }
        }

        public Vector2 GetTextureCoords(Vector2[] texture, float alpha, float beta, float gamma)
        {

            var tem1 = Vector2.Multiply(alpha, texture[0]);
            var tem2 = Vector2.Multiply(beta, texture[1]);
            var tem3 = Vector2.Multiply(gamma, texture[2]);
            return Vector2.Add(tem1, Vector2.Add(tem2, tem3));
           
        }
        public List<float> calculateBarycentricCoordinates(Triangle triangle, float x, float y)
        {
            var barycentricCoordinates = new List<float>(3);

            float x1 = triangle.A.X;
            float y1 = triangle.A.Y;
            float x2 = triangle.B.X;
            float y2 = triangle.B.Y;
            float x3 = triangle.C.X;
            float y3 = triangle.C.Y;

            float denominator = ((y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3));

            barycentricCoordinates.Add(((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) / denominator);
            barycentricCoordinates.Add(((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) / denominator);
            barycentricCoordinates.Add( 1 - barycentricCoordinates[0] - barycentricCoordinates[1]);

            return barycentricCoordinates;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 GetNewTextel(float tx, float ty, ImageData image)
        {
            var fx = tx - float.Floor(tx);
            var fy = ty - float.Floor(ty);
            tx = float.Floor(tx);
            ty = float.Floor(ty);

            var TLIndex = (int)(ty * image.Stride + tx * image.ColorSize / 8);
            var TRIndex = (int)(ty * image.Stride + (tx + 1) * image.ColorSize / 8);
            var BLIndex = (int)((ty + 1) * image.Stride + tx * image.ColorSize / 8);
            var BRIndex = (int)((ty + 1) * image.Stride + (tx + 1) * image.ColorSize / 8);
            if (TLIndex >= image.MapData.Length || TRIndex >= image.MapData.Length || BLIndex >= image.MapData.Length || BRIndex >= image.MapData.Length )
            {
                return new Vector3(1,1,1);
            }
            var TL = new Vector3(image.MapData[TLIndex], image.MapData[TLIndex+1], image.MapData[TLIndex+2]);
            var TR = new Vector3(image.MapData[TRIndex], image.MapData[TRIndex+1], image.MapData[TRIndex+2]);
            var BL = new Vector3(image.MapData[BLIndex], image.MapData[BLIndex+1], image.MapData[BLIndex+2]);
            var BR = new Vector3(image.MapData[BRIndex], image.MapData[BRIndex+1], image.MapData[BRIndex+2]);


            var CT = fx * TR + (1 - fx) * TL;
            var CB = fx * BR + (1 - fx) * BL;

            return fy * CB + (1 - fy) * CT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color CalculateColor(Vector3 point, Vector3 normal, Scene scene, Color baseColor)
        {
            var light = new Vector3(0,0,0);
            for (int i =0; i < scene.Light.Count; i++)
             light += scene.Light[i].CalculateLightWithSpecular(point, normal, scene.Camera.Eye);
            var color = Color.FromArgb(
                (byte)(light.X * baseColor.R > 255 ? 255 : light.X * baseColor.R),
                (byte)(light.Y * baseColor.G > 255 ? 255 : light.Y * baseColor.G),
                (byte)(light.Z * baseColor.B > 255 ? 255 : light.Z * baseColor.B));
            return color;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (List<float>, List<float>) TraingleInterpolation(int y0, float v0, int y1, float v1, int y2, float v2)
        {
            var v01 = Interpolate(y0, v0, y1, v1);
            var v12 = Interpolate(y1, v1, y2, v2);
            var v02 = Interpolate(y0, v0, y2, v2);
            v01.RemoveAt(v01.Count - 1);
            var v012 = v01.Concat(v12).ToList();
            return (v02, v012);
        }


        private (List<float>, List<float>) TraingleInterpolationTexture(int y0, float v0, int y1, float v1, int y2, float v2)
        {
            var v01 = InterpolateTexture(y0, v0, y1, v1);
            var v12 = InterpolateTexture(y1, v1, y2, v2);
            var v02 = InterpolateTexture(y0, v0, y2, v2);
            v01.RemoveAt(v01.Count - 1);
            var v012 = v01.Concat(v12).ToList();
            return (v02, v012);
        }
        private unsafe void MyRasterizeTrianglePhong(Triangle triangle, Scene scene)
        {
            if ((triangle.A.X > 0 && triangle.A.Y > 0 && triangle.A.X < _width && triangle.A.Y < _height) ||
            (triangle.B.X > 0 && triangle.B.Y > 0 && triangle.B.X < _width && triangle.B.Y < _height) ||
            (triangle.C.X > 0 && triangle.C.Y > 0 && triangle.C.X < _width && triangle.C.Y < _height))
            {
                Color baseColor = Color.Gold;
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

                (var x02, var x012) = TraingleInterpolation(YA, triangle.A.X, YB, triangle.B.X, YC, triangle.C.X);
                (var z02, var z012) = TraingleInterpolation(YA, triangle.A.Z, YB, triangle.B.Z, YC, triangle.C.Z);


                (var nx02, var nx012) = TraingleInterpolation(YA, triangle.NormalA.X, YB, triangle.NormalB.X, YC, triangle.NormalC.X);
                (var ny02, var ny012) = TraingleInterpolation(YA, triangle.NormalA.Y, YB, triangle.NormalB.Y, YC, triangle.NormalC.Y);
                (var nz02, var nz012) = TraingleInterpolation(YA, triangle.NormalA.Z, YB, triangle.NormalB.Z, YC, triangle.NormalC.Z);

                (var rx02, var rx012) = TraingleInterpolation(YA, triangle.RealA.X, YB, triangle.RealB.X, YC, triangle.RealC.X);
                (var ry02, var ry012) = TraingleInterpolation(YA, triangle.RealA.Y, YB, triangle.RealB.Y, YC, triangle.RealC.Y);
                (var rz02, var rz012) = TraingleInterpolation(YA, triangle.RealA.Z, YB, triangle.RealB.Z, YC, triangle.RealC.Z);

                var m = (int)float.Floor(x012.Count / 2.0f);
                List<float> x_left;
                List<float> x_right;
                List<float> z_left;
                List<float> z_right;
                List<float> nx_right, ny_right, nz_right;
                List<float> nx_left, ny_left, nz_left;

                List<float> rx_right, ry_right, rz_right;
                List<float> rx_left, ry_left, rz_left;

                if (x02[m] < x012[m])
                {
                    (x_left, x_right) = (x02, x012);
                    (z_left, z_right) = (z02, z012);

                    (nx_left, nx_right) = (nx02, nx012);
                    (ny_left, ny_right) = (ny02, ny012);
                    (nz_left, nz_right) = (nz02, nz012);
                    (rx_left, rx_right) = (rx02, rx012);
                    (ry_left, ry_right) = (ry02, ry012);
                    (rz_left, rz_right) = (rz02, rz012);

                }
                else
                {
                    (x_left, x_right) = (x012, x02);
                    (z_left, z_right) = (z012, z02);

                    (nx_left, nx_right) = (nx012, nx02);
                    (ny_left, ny_right) = (ny012, ny02);
                    (nz_left, nz_right) = (nz012, nz02);

                    (rx_left, rx_right) = (rx012, rx02);
                    (ry_left, ry_right) = (ry012, ry02);
                    (rz_left, rz_right) = (rz012, rz02);

                }
                int YDiffTop = 0;
                int YDiffTopI = 0;
                int TopY = (int)float.Round(triangle.A.Y);
                if (triangle.A.Y < 0)
                {
                    YDiffTop = -(int)float.Round(triangle.A.Y);
                    YDiffTopI = (int)float.Round(triangle.C.Y);
                    TopY = 0;
                }
                for (int y = TopY; y <= (int)float.Round(triangle.C.Y); y++)
                {
                    if (y < 0 || y >= _height)
                        continue;
                    var index = (y - TopY + YDiffTop);
                    //if (index < x_left.Count && index < x_right.Count)
                    {
                        var xl = (int)float.Round(x_left[index]);
                        var xr = (int)float.Round(x_right[index]);
                        var zl = z_left[index];
                        var zr = z_right[index];
                        (var nxl, var nxr) = (nx_left[index], nx_right[index]);
                        (var nyl, var nyr) = (ny_left[index], ny_right[index]);
                        (var nzl, var nzr) = (nz_left[index], nz_right[index]);

                        (var rxl, var rxr) = (rx_left[index], rx_right[index]);
                        (var ryl, var ryr) = (ry_left[index], ry_right[index]);
                        (var rzl, var rzr) = (rz_left[index], rz_right[index]);
                        var zscan = Interpolate(xl, zl, xr, zr);
                        if (zscan.Count == 0)
                            continue;

                        var nxscan = Interpolate(xl, nxl, xr, nxr);
                        var nyscan = Interpolate(xl, nyl, xr, nyr);
                        var nzscan = Interpolate(xl, nzl, xr, nzr);
                        var rxscan = Interpolate(xl, rxl, xr, rxr);
                        var ryscan = Interpolate(xl, ryl, xr, ryr);
                        var rzscan = Interpolate(xl, rzl, xr, rzr);


                        for (int x = xl; x <= xr; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            var z = zscan[x - xl];


                            lock (SpincLocker[y][x])
                            {
                                if (z < ZBuffer[y][x])
                                {
                                    ZBuffer[y][x] = z;
                                    byte* pixelPtr = data + y * _stride + x * 3;
                                    var vertex = new Vector3(rxscan[x - xl], ryscan[x - xl], rzscan[x - xl]);
                                    var normal = new Vector3(nxscan[x - xl], nyscan[x - xl], nzscan[x - xl]);
                                    Color color = CalculateColor(vertex, Vector3.Normalize(normal), scene, baseColor);
                                    *pixelPtr++ = color.B;
                                    *pixelPtr++ = color.G;
                                    *pixelPtr = color.R;
                                }
                            }


                        }

                    }


                }
            }
        }



    }
}
