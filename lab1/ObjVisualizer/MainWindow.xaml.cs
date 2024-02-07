using ObjVisualizer.GraphicsComponents;
using ObjVisualizer.Parser;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
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
        private int WindowWidth;
        private int WindowHeight;
        private Image Image;
        private DispatcherTimer timer;
        private TextBlock textBlock;
        private int FrameCount;
        private long PointsCount;
        private IObjReader Reader;

        private Scene MainScene;
        public MainWindow()
        {
            Reader = ObjReader.GetObjReader("Objects\\Shrek.obj");

            InitializeComponent();
            InitializeWindowComponents();
            Frame();

        }

        private void InitializeWindowComponents()
        {

            Application.Current.MainWindow.SizeChanged += Resize;
            PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonDownEvent +=
            WindowHeight = (int)this.Height;
            WindowWidth = (int)this.Width;


            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            timer.Start();
            var grid = new Grid();
            Image = new Image();
            Image.Width = this.Width;
            Image.Height = this.Height;
            Image.Stretch = Stretch.Fill;

            textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.VerticalAlignment = VerticalAlignment.Bottom;
            textBlock.FontSize = 12;
            textBlock.Foreground = Brushes.White;

            grid.Children.Add(Image);
            grid.Children.Add(textBlock);

            Grid.SetRow(Image, 0);
            Grid.SetColumn(Image, 0);
            Grid.SetZIndex(Image, 0);

            Grid.SetRow(textBlock, 0);
            Grid.SetColumn(textBlock, 0);
            Grid.SetZIndex(textBlock, 1);

            this.Content = grid;

            MainScene = Scene.GetScene();

            MainScene.camera = new Camera(new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(0, -0.2f, 0), (float)WindowWidth / (float)WindowHeight, 70.0f * ((float)Math.PI / 180.0f), 10.0f, 0.1f);
            MainScene.ModelMatrix = Matrix4x4.Transpose(MatrixOperator.Scale(new Vector3(0.01f, 0.01f, 0.01f)) * MatrixOperator.RotateY(-20f * ((float)Math.PI / 180.0f))* MatrixOperator.RotateX(20f * ((float)Math.PI / 180.0f)) * MatrixOperator.Move(new Vector3(0, -50, 0)));
            MainScene.ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(MainScene.camera));
            MainScene.ProjectionMatrix = Matrix4x4.Transpose(MatrixOperator.GetProjectionMatrix(MainScene.camera));
            MainScene.ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(WindowWidth, WindowHeight));
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
            e.Handled = true;
        }
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            { 
                Point currentPosition = e.GetPosition(this);
            }
        }
        private void Resize(object sender, SizeChangedEventArgs e)
        {

            Image.Width = (int)e.NewSize.Width;
            Image.Height = (int)e.NewSize.Height;
            WindowHeight = (int)e.NewSize.Height;
            WindowWidth = (int)e.NewSize.Width;
            WriteableBitmap writableBitmap = new WriteableBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Bgr24, null);
            MainScene.SceneResize(WindowWidth, WindowHeight);
        }


        async private void Frame()
        {

            Vector4 Vertext1;
            Vector4 Vertext2;
            Vector4 Vertext3;
            Vector4 TempVertexI;
            Vector4 TempVertexJ;
            var Vertex = Reader.Vertices.ToList();

            while (true)
            {
                PointsCount = 0;
                WriteableBitmap writableBitmap = new WriteableBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Bgr24, null);
                Int32Rect rect = new Int32Rect(0, 0, WindowWidth, WindowHeight);
                IntPtr buffer = writableBitmap.BackBuffer;
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

                    foreach (var face in Reader.Faces)
                    {

                        var Vertexes = face.VertexIds.ToList();
                        TempVertexI = MainScene.GetTransformedVertex(Vertex[Vertexes[0] - 1]);
                        TempVertexJ = MainScene.GetTransformedVertex(Vertex[Vertexes.Last() - 1]) ;
                        if ((int)TempVertexI.X > 0 && (int)TempVertexJ.X > 0 &&
                                    (int)TempVertexI.Y > 0 && (int)TempVertexJ.Y > 0 &&
                                    (int)TempVertexI.X < WindowWidth && (int)TempVertexJ.X < WindowWidth &&
                                    (int)TempVertexI.Y < WindowHeight && (int)TempVertexJ.Y < WindowHeight)
                                    DrawLine((int)TempVertexI.X, (int)TempVertexI.Y, (int)TempVertexJ.X, (int)TempVertexJ.Y, pixels, stride);
                        for (int i = 0; i < Vertexes.Count - 1; i++)
                        {
                            TempVertexI = MainScene.GetTransformedVertex(Vertex[Vertexes[i] - 1]);

                            for (int j = i + 1; j < Vertexes.Count; j++)
                            {

                                TempVertexJ = MainScene.GetTransformedVertex(Vertex[Vertexes[j] - 1]);
                                
                                if ((int)TempVertexI.X > 0 && (int)TempVertexJ.X > 0 &&
                                    (int)TempVertexI.Y > 0 && (int)TempVertexJ.Y > 0 &&
                                    (int)TempVertexI.X < WindowWidth && (int)TempVertexJ.X < WindowWidth &&
                                    (int)TempVertexI.Y < WindowHeight && (int)TempVertexJ.Y < WindowHeight)
                                    DrawLine((int)TempVertexI.X, (int)TempVertexI.Y, (int)TempVertexJ.X, (int)TempVertexJ.Y, pixels, stride);
                            }
                        }

                        

                    }


                }
                //MainScene.ModelMatrix = Matrix4x4.Transpose(MatrixOperator.RotateX(1f * ((float)Math.PI / 180.0f)));

                writableBitmap.AddDirtyRect(rect);
                writableBitmap.Unlock();
                MainScene.ModelMatrix = Matrix4x4.Transpose(MatrixOperator.GetModelMatrix());
                MainScene.ChangeStatus = false ;
                Image.Source = writableBitmap;
                FrameCount++;
                await Task.Delay(1);

            }

        }

        public unsafe void DrawLine(int x0, int y0, int x1, int y1, byte* data, int stride)
        {

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep)
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
                if (steep)
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
            textBlock.Text = FrameCount.ToString() + " fps";
            FrameCount = 0;
        }
    }
}