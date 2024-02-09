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
using Vector = System.Windows.Vector;

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
            Reader = ObjReader.GetObjReader("Objects\\SM_Ship01A_02_OBJ.obj");

            InitializeComponent();

            SizeChanged += Resize;
            PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            WindowHeight = (int)Height;
            WindowWidth = (int)Width;

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

            MainScene.Camera = new Camera(new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(0, -0.2f, 0),
                WindowWidth / (float)WindowHeight, 70.0f * ((float)Math.PI / 180.0f), 10.0f, 0.1f);
            MainScene.ModelMatrix = Matrix4x4.Transpose(MatrixOperator.Scale(
                new Vector3(0.01f, 0.01f, 0.01f)) * MatrixOperator.RotateY(-20f * ((float)Math.PI / 180.0f))
                * MatrixOperator.RotateX(20f * ((float)Math.PI / 180.0f)) * MatrixOperator.Move(new Vector3(0, -50, 0)));
            MainScene.ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(MainScene.Camera));
            MainScene.ProjectionMatrix = Matrix4x4.Transpose(MatrixOperator.GetProjectionMatrix(MainScene.Camera));
            MainScene.ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(WindowWidth, WindowHeight));

            Frame();
        }

        private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int scrollDelta = e.Delta;

            if (scrollDelta > 0)
            {
                MainScene.UpdateScaleMatrix(0.2f);
            }
            else if (scrollDelta < 0)
            {
                MainScene.UpdateScaleMatrix(-0.2f);
            }

            MainScene.ChangeStatus = true;
            MainScene.ResetTransformMatrixes();

            e.Handled = true;
        }
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                float rotationAngleY = 360.0f * 10 / WindowWidth;
                float rotationAngleX = 360.0f * 10 / WindowHeight;

                var rotationVector = new Vector3(0, 0, 0);
                Vector positionDelta = currentPosition - LastMousePosition;

                if (Math.Abs(positionDelta.Y) < WindowHeight * 0.01)
                {
                    if (positionDelta.X < 0)
                    {
                        rotationVector.Y = -rotationAngleY;
                    }
                    else if (positionDelta.X > 0)
                    {
                        rotationVector.Y = rotationAngleY;
                    }
                }

                if (Math.Abs(positionDelta.X) < WindowWidth * 0.01)
                {
                    if (positionDelta.Y < 0)
                    {
                        rotationVector.X = -rotationAngleX;
                    }
                    else if (positionDelta.Y > 0)
                    {
                        rotationVector.X = rotationAngleX;
                    }
                }

                MainScene.UpdateRotateMatrix(rotationVector);
                MainScene.ResetTransformMatrixes();

                LastMousePosition = currentPosition;
                MainScene.ChangeStatus = true;
            }
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            LastMousePosition = e.GetPosition(this);

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
            MouseHandler.LastAction = MouseHandler.Actions.Idle;

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    MainScene.UpdateMoveMatrix(new Vector3(.1f, 0, 0));
                    MainScene.ResetTransformMatrixes();
                    MainScene.ChangeStatus = true;
                    break;
                case Key.D:
                    MainScene.UpdateMoveMatrix(new Vector3(-.1f, 0, 0));
                    MainScene.ResetTransformMatrixes();
                    MainScene.ChangeStatus = true;
                    break;
                case Key.S:
                    MainScene.UpdateMoveMatrix(new Vector3(0, .1f, 0));
                    MainScene.ResetTransformMatrixes();
                    MainScene.ChangeStatus = true;
                    break;
                case Key.W:
                    MainScene.UpdateMoveMatrix(new Vector3(0, -.1f, 0));
                    MainScene.ResetTransformMatrixes();
                    MainScene.ChangeStatus = true;
                    break;
                default:
                    return;
            }
        }

        private void Resize(object sender, SizeChangedEventArgs e)
        {
            Image.Width = (int)e.NewSize.Width;
            Image.Height = (int)e.NewSize.Height;
            WindowHeight = (int)e.NewSize.Height;
            WindowWidth = (int)e.NewSize.Width;

            MainScene.SceneResize(WindowWidth, WindowHeight);
        }


        private async void Frame()
        {
           
            var Vertex = Reader.Vertices.ToList();
            while (true)
            {
                var writableBitmap = new WriteableBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Bgr24, null);
                var rect = new Int32Rect(0, 0, WindowWidth, WindowHeight);
                var buffer = writableBitmap.BackBuffer;

                int stride = writableBitmap.BackBufferStride;
                writableBitmap.Lock();

                unsafe
                {
                    byte* pixels = (byte*)buffer.ToPointer();

                    if (MainScene.ChangeStatus)
                    {
                        for (int i = 0; i < Vertex.Count; i++)
                        {
                            Vertex[i] = Vector4.Transform(Vertex[i], MainScene.ModelMatrix);
                        }
                    }
                    Parallel.ForEach(Reader.Faces, face =>
                    {
                        var FaceVertexes = face.VertexIds.ToList();
                        Vector4 TempVertexI = MainScene.GetTransformedVertex(Vertex[FaceVertexes[0] - 1]);
                        Vector4 TempVertexJ = MainScene.GetTransformedVertex(Vertex[FaceVertexes.Last() - 1]);
                        if ((int)TempVertexI.X > 0 && (int)TempVertexJ.X > 0 &&
                                    (int)TempVertexI.Y > 0 && (int)TempVertexJ.Y > 0 &&
                                    (int)TempVertexI.X < WindowWidth && (int)TempVertexJ.X < WindowWidth &&
                                    (int)TempVertexI.Y < WindowHeight && (int)TempVertexJ.Y < WindowHeight)
                            DrawLine((int)TempVertexI.X, (int)TempVertexI.Y, (int)TempVertexJ.X, (int)TempVertexJ.Y, pixels, stride);
                        for (int i = 0; i < FaceVertexes.Count - 1; i++)
                        {
                            TempVertexI = MainScene.GetTransformedVertex(Vertex[FaceVertexes[i] - 1]);
                            TempVertexJ = MainScene.GetTransformedVertex(Vertex[FaceVertexes[i + 1] - 1]);
                            if ((int)TempVertexI.X > 0 && (int)TempVertexJ.X > 0 &&
                                (int)TempVertexI.Y > 0 && (int)TempVertexJ.Y > 0 &&
                                (int)TempVertexI.X < WindowWidth && (int)TempVertexJ.X < WindowWidth &&
                                (int)TempVertexI.Y < WindowHeight && (int)TempVertexJ.Y < WindowHeight)
                                DrawLine((int)TempVertexI.X, (int)TempVertexI.Y, (int)TempVertexJ.X, (int)TempVertexJ.Y, pixels, stride);

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

                *(pixelPtr++) = 255;
                *(pixelPtr++) = 255;
                *(pixelPtr) = 255;

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
