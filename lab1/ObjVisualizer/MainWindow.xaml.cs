using ObjVisualizer.Parser;
using System.Windows;

namespace ObjVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var reader = ObjReader.GetObjReader("C:\\Users\\rylon\\Downloads\\Telegram Desktop\\Shrek.obj");
        }
    }
}