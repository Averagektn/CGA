using ObjVisualizer.GraphicsComponents;
using ObjVisualizer.MouseHandlers;
using ObjVisualizer.Parser;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        public MainWindow()
        {
            Reader = ObjReader.GetObjReader("Objects\\Shrek.obj");

            InitializeComponent();

            SizeChanged += Resize;
            PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;

            WindowWidth = (int)Width;
            WindowHeight = (int)Height;

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
                Stretch = Stretch.Fill
            };

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

            MainScene.Camera = new Camera(new Vector3(0, 2f, 2f), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                WindowWidth / (float)WindowHeight, 70.0f * ((float)Math.PI / 180.0f), 10.0f, 0.1f);
            //MainScene.ModelMatrix = Matrix4x4.Transpose(MatrixOperator.Scale(
            //    new Vector3(0.01f, 0.01f, 0.01f)) * MatrixOperator.RotateY(-20f * ((float)Math.PI / 180.0f))
            //    * MatrixOperator.RotateX(20f * ((float)Math.PI / 180.0f)) * MatrixOperator.Move(new Vector3(0, 0, 0)));
            MainScene.Camera.Eye = new Vector3(
                        MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraPhi) * (float)Math.Sin(MainScene.Camera.CameraZeta),
                        MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraZeta),
                        MainScene.Camera.Radius * (float)Math.Sin(MainScene.Camera.CameraPhi) * (float)Math.Sin(MainScene.Camera.CameraZeta));
            MainScene.ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(MainScene.Camera));
            MainScene.ProjectionMatrix = Matrix4x4.Transpose(MatrixOperator.GetProjectionMatrix(MainScene.Camera));
            MainScene.ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(WindowWidth, WindowHeight));

            Frame();
        }

        private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainScene.Camera.Radius += -e.Delta / 100;

            e.Handled = true;
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

                LastMousePosition = currentPosition;
            }
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            LastMousePosition = e.GetPosition(this);

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
            MouseHandler.LastAction = MouseHandler.Actions.Idle;

        private void Resize(object sender, SizeChangedEventArgs e)
        {
            Image.Width = (int)e.NewSize.Width;
            Image.Height = (int)e.NewSize.Height;

            WindowWidth = (int)Width;
            WindowHeight = (int)Height;

            MainScene.SceneResize(WindowWidth, WindowHeight);
        }


        private async void Frame()
        {
            var Vertexes = Reader.Vertices.ToList();
            var Normales = Reader.VertexNormals.ToList();

            while (true)
            {
                var writableBitmap = new WriteableBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Bgr24, null);
                var rect = new Int32Rect(0, 0, WindowWidth, WindowHeight);

                IntPtr buffer = writableBitmap.BackBuffer;

                int stride = writableBitmap.BackBufferStride;
                writableBitmap.Lock();

                MainScene.Camera.Eye = new Vector3(
                       MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraPhi) * (float)Math.Sin(MainScene.Camera.CameraZeta),
                       MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraZeta),
                       MainScene.Camera.Radius * (float)Math.Sin(MainScene.Camera.CameraPhi) * (float)Math.Sin(MainScene.Camera.CameraZeta));

                MainScene.UpdateViewMatix();

                // rasterizator 3000
                var drawer = new Drawer(WindowWidth, WindowHeight, buffer, stride);

                unsafe
                {
                    byte* pixels = (byte*)buffer.ToPointer();

                    Parallel.ForEach(Reader.Faces, face =>
                    {
                        var FaceVertexes = face.VertexIds.ToList();
                        var FaceNormales = face.NormalIds.ToList();
                        var ZeroVertext = Vertexes[FaceVertexes[0] - 1];

                        Vector3 PoliNormal = Vector3.Zero;

                        for (int i = 0; i < FaceNormales.Count; i++)
                        {
                            PoliNormal += Normales[FaceNormales[i] - 1];
                        }

                        if (Vector3.Dot(PoliNormal / FaceNormales.Count, new Vector3(Vertexes[FaceVertexes[0] - 1].X,
                            Vertexes[FaceVertexes[0] - 1].Y, Vertexes[FaceVertexes[0] - 1].Z) + MainScene.Camera.Eye) > 0)
                        {
                            // RASTRIZATION
                            var triangle = Enumerable.Range(0, 3)
                                .Select(i => MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i] - 1]))
                                .ToList();
                            drawer.Rasterize(triangle);

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
                    });
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
                *pixelPtr++ = 0;
                *pixelPtr = 0;

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
