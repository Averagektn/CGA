using ObjVisualizer.GraphicsComponents;
using ObjVisualizer.Parser;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
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
        public MainWindow()
        {
            Reader = ObjReader.GetObjReader("C:\\Users\\dimon\\OneDrive\\Рабочий стол\\Study\\Универ\\Курс 3\\Семестр 6\\АКГ\\Shrek.obj");

            InitializeComponent();
            InitializeWindowComponents();
            Frame();

        }

        private void InitializeWindowComponents()
        {
            Application.Current.MainWindow.SizeChanged += Resize;
            WindowHeight = (int)this.Height;
            WindowWidth = (int)this.Width;
            Image = new Image();
            Image.Width = this.Width;
            Image.Height = this.Height;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            timer.Start();
            var grid = new Grid();
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
        }

        private void Resize(object sender, SizeChangedEventArgs e)
        {

            Image.Width = (int)e.NewSize.Width;
            Image.Height = (int)e.NewSize.Height;

            WriteableBitmap writableBitmap = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 96, 96, PixelFormats.Bgra32, null);
            WindowHeight = (int)e.NewSize.Height;
            WindowWidth = (int)e.NewSize.Width;

        }


        async private void Frame()
        {

            Camera camera = new Camera(new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0), (float)WindowWidth / (float)WindowHeight, 60.0f * ((float)Math.PI / 180.0f), 10.0f, 0.1f);
            Vector4 Vertext1;
            Vector4 Vertext2;
            Vector4 Vertext3;
            var Vertex = Reader.Vertices.ToList();
            while (true)
            {
                PointsCount = 0;
                await Task.Delay(1);
                WriteableBitmap writableBitmap = new WriteableBitmap(WindowWidth+1, WindowHeight+1, 96, 96, PixelFormats.Bgr24, null);
                Int32Rect rect = new Int32Rect(0, 0, WindowWidth+1, WindowHeight+1);
                writableBitmap.Lock();
                IntPtr buffer = writableBitmap.BackBuffer;
                int stride = writableBitmap.BackBufferStride;
                //Matrix4x4 ModelMatrix = Matrix4x4.Transpose(MatrixOperator.GetModelMatrix());
                Matrix4x4 ModelMatrix = Matrix4x4.Transpose(MatrixOperator.Scale(new Vector3(0.05f, 0.05f, 0.05f)));
                Matrix4x4 ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(camera));
                Matrix4x4 ProjectionMatrix = Matrix4x4.Transpose(MatrixOperator.GetProjectionMatrix(camera));
                Matrix4x4 ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(WindowWidth, WindowHeight));
                unsafe
                {
                    byte* pixels = (byte*)buffer.ToPointer();

                    foreach (var face in Reader.Faces)
                    {
                        var Vertexes = face.VertexIds.ToList();
                        Vertext1 = Vector4.Transform(Vertex[Vertexes[0] - 1], ModelMatrix);
                        Vertext1 = Vector4.Transform(Vertext1, ViewMatrix);
                        Vertext1 = Vector4.Transform(Vertext1, ProjectionMatrix);
                        Vertext1 = Vector4.Divide(Vertext1, Vertext1.W);
                        Vertext1 = Vector4.Transform(Vertext1, ViewPortMatrix);


                        Vertext2 = Vector4.Transform(Vertex[Vertexes[1] - 1], ModelMatrix);
                        Vertext2 = Vector4.Transform(Vertext2, ViewMatrix);
                        Vertext2 = Vector4.Transform(Vertext2, ProjectionMatrix);
                        Vertext2 = Vector4.Divide(Vertext2, Vertext2.W);
                        Vertext2 = Vector4.Transform(Vertext2, ViewPortMatrix);

                        Vertext3 = Vector4.Transform(Vertex[Vertexes[2] - 1], ModelMatrix);
                        Vertext3 = Vector4.Transform(Vertext3, ViewMatrix);
                        Vertext3 = Vector4.Transform(Vertext3, ProjectionMatrix);
                        Vertext3 = Vector4.Divide(Vertext3, Vertext3.W);
                        Vertext3 = Vector4.Transform(Vertext3, ViewPortMatrix);
                        if ((int)Vertext1.X > 0 && (int)Vertext2.X > 0 &&
                            (int)Vertext1.Y > 0 && (int)Vertext2.Y > 0 &&
                            (int)Vertext1.X < WindowWidth && (int)Vertext2.X < WindowWidth &&
                            (int)Vertext1.Y < WindowWidth && (int)Vertext2.Y < WindowWidth)
                            DrawLine((int)Vertext1.X, (int)Vertext1.Y, (int)Vertext2.X, (int)Vertext2.Y, pixels, stride);
                        if ((int)Vertext2.X > 0 && (int)Vertext3.X > 0 &&
                            (int)Vertext2.Y > 0 && (int)Vertext3.Y > 0 &&
                            (int)Vertext2.X < WindowWidth && (int)Vertext3.X < WindowWidth &&
                            (int)Vertext2.Y < WindowWidth && (int)Vertext3.Y < WindowWidth)
                            DrawLine((int)Vertext2.X, (int)Vertext2.Y, (int)Vertext3.X, (int)Vertext3.Y, pixels, stride);
                        if ((int)Vertext1.X > 0 && (int)Vertext3.X > 0 &&
                            (int)Vertext1.Y > 0 && (int)Vertext3.Y > 0 &&
                            (int)Vertext1.X < WindowWidth && (int)Vertext3.X < WindowWidth &&
                            (int)Vertext1.Y < WindowWidth && (int)Vertext3.Y < WindowWidth)
                            DrawLine((int)Vertext3.X, (int)Vertext3.Y, (int)Vertext1.X, (int)Vertext1.Y, pixels, stride);
                    }


                }
                writableBitmap.AddDirtyRect(rect);
                writableBitmap.Unlock();
                Image.Source = writableBitmap;
                FrameCount++;
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
            int val1;
            int val2;
            for (int x = x0; x <= x1; x++)
            {

                if (steep)
                {
                    val1 = x;
                    val2 = y;
                }
                else
                {
                    val1 = y;
                    val2 = x;
                }
                byte* pixelPtr = data + val2 * stride + val1 * 3;
                *(pixelPtr++) = 255;
                *(pixelPtr++) = 255;
                *(pixelPtr) = 255;





                //PointsCount++;


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