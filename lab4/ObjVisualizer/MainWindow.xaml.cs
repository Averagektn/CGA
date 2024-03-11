using ObjVisualizer.GraphicsComponents;
using ObjVisualizer.Parser;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace ObjVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Scene MainScene;
        private readonly Image Image;
        private readonly DispatcherTimer Timer;
        private readonly TextBlock TextBlock;

        private readonly IObjReader Reader;

        private Point LastMousePosition;

        private int WindowWidth;
        private int WindowHeight;
        private int FrameCount;

        private int _windowScale = 2;

        private readonly IMtlParser MtlParserKnight = new MtlParser("Objects\\Shovel Knight\\shovel_low.mtl");


        public MainWindow()
        {
            Reader = ObjReader.GetObjReader("Objects\\Shovel Knight\\shovel_low.obj");
            
            InitializeComponent();

            SizeChanged += Resize;
            PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            WindowWidth = (int)Width * _windowScale;
            WindowHeight = (int)Height * _windowScale;

            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            Timer.Tick += Timer_Tick;
            Timer.Start();

            var grid = new Grid();
            Image = new Image
            {
                Width = Width,
                Height = Height,
                Stretch = Stretch.Fill,


            };
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.HighQuality);

            TextBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                FontSize = 12,
                Foreground = Brushes.White
            };

            grid.Children.Add(Image);
            grid.Children.Add(TextBlock);

            Grid.SetRow(Image, 0);
            Grid.SetColumn(Image, 0);
            Panel.SetZIndex(Image, 0);

            Grid.SetRow(TextBlock, 0);
            Grid.SetColumn(TextBlock, 0);
            Panel.SetZIndex(TextBlock, 1);

            Content = grid;

            MainScene = Scene.GetScene();
            MainScene.GraphicsObjects = new GraphicsObject(_mtlParser.GetMapKdBytes(),_mtlParser.GetMapMraoBytes(), _mtlParser.GetNormBytes()) ;
           
            MainScene.Stage = Stage.Stage1;

            MainScene.Camera = new Camera(new Vector3(0, 0f, -1f), new Vector3(0, 1, 0), new Vector3(0, 0, 0),
                WindowWidth/ (float)WindowHeight, 70.0f * ((float)Math.PI / 180.0f), 10.0f, 0.1f);


            MainScene.ModelMatrix = Matrix4x4.Transpose(MatrixOperator.Scale(new Vector3(1f, 1f, 1f)) * MatrixOperator.Move(new Vector3(0, 0f, 0)));
            MainScene.ChangeStatus = true;
            MainScene.Camera.Eye = new Vector3(
                        MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraPhi) * (float)Math.Sin(MainScene.Camera.CameraZeta),
                        MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraZeta),
                        MainScene.Camera.Radius * (float)Math.Sin(MainScene.Camera.CameraPhi) * (float)Math.Sin(MainScene.Camera.CameraZeta));
            MainScene.Light.Add(new PointLight(0, 10, 10, 0.8f, false,false, new Vector3(1f,0.8f,0f),new Vector3(1f, 1f, 1f)));
            MainScene.Light.Add(new PointLight(0, 10, 10, 0f, false,false, new Vector3(0f,0f,1f),new Vector3(1f, 1f, 1f)));
            //MainScene.Light.Add(new PointLight(0, 4, -10, 0.8f, false,false, new Vector3(1f,1f,1f),new Vector3(1f, 1f, 1f)));
            MainScene.ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(MainScene.Camera));
            MainScene.ProjectionMatrix = Matrix4x4.Transpose(MatrixOperator.GetProjectionMatrix(MainScene.Camera));
            MainScene.ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(WindowWidth, WindowHeight));

            Frame();
        }


        private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainScene.Camera.Radius += -e.Delta / 100;
            if (MainScene.Camera.Radius < 0.1f)
                MainScene.Camera.Radius = 0.1f;
            if (MainScene.Camera.Radius > 5 * MainScene.Camera.Radius)
                MainScene.Camera.Radius = 5 * MainScene.Camera.Radius;

            e.Handled = true;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    MainScene.Stage = Stage.Stage1;
                    break;
                case Key.D2:
                    MainScene.Stage = Stage.Stage2;
                    break;
                case Key.D3:
                    MainScene.Stage = Stage.Stage3;
                    break;
                case Key.D4:
                    MainScene.Stage = Stage.Stage4;
                    break;
                case Key.D5:
                    MainScene.Stage = Stage.Stage5;
                    break;
                case Key.D6:
                    MainScene.Ambient = true ;
                    break;
                case Key.D7:
                    MainScene.Specular = true;
                    break;
                case Key.D8:
                    MainScene.Specular = false;
                    MainScene.Ambient = false;
                    break;
                case Key.A:
                    MainScene.Camera.Target += new Vector3(-1f, 0, 0);
                    break;
                case Key.D:
                    MainScene.Camera.Target += new Vector3(1f, 0, 0);
                    break;
                case Key.S:
                    MainScene.Camera.Target += new Vector3(0, -1f, 0);

                    break;
                case Key.W:
                    MainScene.Camera.Target += new Vector3(0, 1f, 0);

                    break;
                default:
                    break;
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);

                float xoffset = (float)(currentPosition.X - LastMousePosition.X);
                float yoffset = (float)(LastMousePosition.Y - currentPosition.Y);

                MainScene.Camera.CameraZeta += yoffset * 0.005f;
                MainScene.Camera.CameraPhi += xoffset * 0.005f;
                if (MainScene.Camera.CameraZeta > Math.PI)
                    MainScene.Camera.CameraZeta = (float)Math.PI - 0.01f;
                if (MainScene.Camera.CameraZeta < 0)
                    MainScene.Camera.CameraZeta = 0.01f;

                LastMousePosition = currentPosition;
            }
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            LastMousePosition = e.GetPosition(this);

        private void Resize(object sender, SizeChangedEventArgs e)
        {
            Image.Width = (int)e.NewSize.Width;
            Image.Height = (int)e.NewSize.Height;

            WindowWidth = (int)Width * _windowScale;
            WindowHeight = (int)Height * _windowScale;

            MainScene.SceneResize(WindowWidth, WindowHeight);
        }

        private async Task Frame()
        {
            var Vertexes = Reader.Vertices.ToList();
            var Normales = Reader.VertexNormals.ToList();
            var Textels = Reader.VertexTextures.ToList();

            while (true)
            {
                var writableBitmap = new WriteableBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Bgr24, null);
                var rect = new Int32Rect(0, 0, WindowWidth, WindowHeight);

                IntPtr buffer = writableBitmap.BackBuffer;

                int stride = writableBitmap.BackBufferStride;
                writableBitmap.Lock();

                MainScene.Camera.Eye = new Vector3(
                       MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraPhi) *
                       (float)Math.Sin(MainScene.Camera.CameraZeta),
                       MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraZeta),
                       MainScene.Camera.Radius * (float)Math.Sin(MainScene.Camera.CameraPhi) *
                       (float)Math.Sin(MainScene.Camera.CameraZeta));

                MainScene.Light[0] = new PointLight(MainScene.Camera.Eye.X, MainScene.Camera.Eye.Y, MainScene.Camera.Eye.Z,0.8f, MainScene.Ambient, MainScene.Specular, new Vector3(1f, 0.8f, 0f), new Vector3(1f, 0.8f, 0));
                MainScene.Light[1] = new PointLight(20, 10, 20, 0.0f, MainScene.Ambient, MainScene.Specular, new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 1f));


                MainScene.UpdateViewMatix();

                var drawer = new Drawer(WindowWidth, WindowHeight, buffer, stride);

                unsafe
                {
                    byte* pixels = (byte*)buffer.ToPointer();
                    if (MainScene.ChangeStatus)
                    {
                        for (int i = 0; i < Vertexes.Count; i++)
                        {
                            Vertexes[i] = Vector4.Transform(Vertexes[i], MainScene.ModelMatrix);
                        }
                    }

                    Parallel.ForEach(Reader.Faces, (Action<Data.Face>)(face =>
                    //foreach (var face in Reader.Faces)
                    {
                        var FaceVertexes = face.VertexIds.ToList();
                        var FaceNormales = face.NormalIds.ToList();
                        var FaceTextels = face.TextureIds.ToList();
                        var ZeroVertext = Vertexes[FaceVertexes[0] - 1];

                        bool isNeed = true;
                        for (int i = 0; i < FaceVertexes.Count; i++)
                        {
                            var Temp = MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i] - 1], out isNeed);

                        }
                        if (isNeed)
                        {


                            Vector3 PoliNormal = Vector3.Zero;
                            if (MainScene.Stage == Stage.Stage1)
                            {
                                Vector4 TempVertexI = MainScene.GetTransformedVertex(Vertexes[FaceVertexes[0] - 1]);
                                Vector4 TempVertexJ = MainScene.GetTransformedVertex(Vertexes[FaceVertexes.Last() - 1]);

                                if ((int)TempVertexI.X > 0 && (int)TempVertexJ.X > 0 &&
                                            (int)TempVertexI.Y > 0 && (int)TempVertexJ.Y > 0 &&
                                            (int)TempVertexI.X < WindowWidth && (int)TempVertexJ.X < WindowWidth &&
                                            (int)TempVertexI.Y < WindowHeight && (int)TempVertexJ.Y < WindowHeight)
                                {
                                    DrawLine((int)TempVertexI.X, (int)TempVertexI.Y, (int)TempVertexJ.X, (int)TempVertexJ.Y,
                                        pixels, stride);
                                }

                                for (int i = 0; i < FaceVertexes.Count - 1; i++)
                                {
                                    TempVertexI = MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i] - 1]);
                                    TempVertexJ = MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i + 1] - 1]);

                                    if ((int)TempVertexI.X > 0 && (int)TempVertexJ.X > 0 &&
                                        (int)TempVertexI.Y > 0 && (int)TempVertexJ.Y > 0 &&
                                        (int)TempVertexI.X < WindowWidth && (int)TempVertexJ.X < WindowWidth &&
                                        (int)TempVertexI.Y < WindowHeight && (int)TempVertexJ.Y < WindowHeight)
                                    {
                                        DrawLine((int)TempVertexI.X, (int)TempVertexI.Y, (int)TempVertexJ.X, (int)TempVertexJ.Y,
                                            pixels, stride);
                                    }
                                }
                            }

                            for (int i = 0; i < FaceNormales.Count; i++)
                            {
                                PoliNormal += Normales[FaceNormales[i] - 1];
                            }
                            if (MainScene.Stage == Stage.Stage2)
                            {
                                if (Vector3.Dot(PoliNormal / FaceNormales.Count, -new Vector3(Vertexes[FaceVertexes[0] - 1].X,
                              Vertexes[FaceVertexes[0] - 1].Y, Vertexes[FaceVertexes[0] - 1].Z) + MainScene.Camera.Eye) > 0)
                                {
                                    // RASTRIZATION
                                    var triangle = Enumerable.Range(0, FaceVertexes.Count)
                                        .Select(i => MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i] - 1]))
                                        .ToList();
                                    float light = MainScene.Light[0].CalculateLightDiffuse(new Vector3(Vertexes[FaceVertexes[0] - 1].X,
                                        Vertexes[FaceVertexes[0] - 1].Y, Vertexes[FaceVertexes[0] - 1].Z), PoliNormal);
                                    drawer.Rasterize(triangle,
                                        Color.FromArgb(
                                            (byte)(light * 255 > 255 ? 255 : light * 255),
                                            (byte)(light * 255 > 255 ? 255 : light * 255),
                                            (byte)(light * 255 > 255 ? 255 : light * 255)));
                                }
                            }

                            if (MainScene.Stage == Stage.Stage3)
                            {
                                if (Vector3.Dot(PoliNormal / FaceNormales.Count, -new Vector3(Vertexes[FaceVertexes[0] - 1].X,
                              Vertexes[FaceVertexes[0] - 1].Y, Vertexes[FaceVertexes[0] - 1].Z) + MainScene.Camera.Eye) > 0)
                                {
                                    var triangleVertexes = Enumerable.Range(0, FaceVertexes.Count)
                                       .Select(i => MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i] - 1]))
                                       .ToList();
                                    var triangleNormales = Enumerable.Range(0, FaceVertexes.Count)
                                        .Select(i => Normales[FaceNormales[i] - 1])
                                        .ToList();
                                    var originalVertexes = Enumerable.Range(0, FaceVertexes.Count)
                                       .Select(i => Vertexes[FaceVertexes[i] - 1])
                                       .ToList();
                                    drawer.Rasterize(triangleVertexes, triangleNormales, originalVertexes, MainScene);
                                }
                            }

                            if (MainScene.Stage is Stage.Stage4)
                            {
                                if (Vector3.Dot(PoliNormal / FaceNormales.Count, -new Vector3(Vertexes[FaceVertexes[0] - 1].X,
                              Vertexes[FaceVertexes[0] - 1].Y, Vertexes[FaceVertexes[0] - 1].Z) + MainScene.Camera.Eye) > 0)
                                {
                                    var triangleVertexes = Enumerable.Range(0, FaceVertexes.Count)
                                      .Select(i => MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i] - 1]))
                                      .ToList();
                                    var originalVertexes = Enumerable.Range(0, FaceVertexes.Count)
                                      .Select(i => Vertexes[FaceVertexes[i] - 1])
                                      .ToList();
                                    var triangleTextels = Enumerable.Range(0, FaceTextels.Count)
                                        .Select(i => Textels[FaceTextels[i] - 1])
                                        .ToList();

                                    var triangleView= Enumerable.Range(0, FaceVertexes.Count)
                                        .Select(i => MainScene.GetViewVertex(Vertexes[FaceVertexes[i] - 1]))
                                        .ToList();

                                    drawer.Rasterize(triangleVertexes, originalVertexes, triangleTextels,triangleView, MainScene);


                                }
                            }
                        }
                    }));
                //}
                
                }

                writableBitmap.AddDirtyRect(rect);
                writableBitmap.Unlock();

                MainScene.ModelMatrix = Matrix4x4.Transpose(MatrixOperator.GetModelMatrix());
                MainScene.ChangeStatus = false;

                Image.Source = writableBitmap;

                FrameCount++;

                await Task.Delay(1);
            }
        }

        public unsafe void DrawLine(int x0, int y0, int x1, int y1, byte* data, int stride)
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

                byte* pixelPtr = data + var1 * stride + var2 * 3;

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

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TextBlock.Text = $"{FrameCount} fps";
            FrameCount = 0;
        }
    }
}