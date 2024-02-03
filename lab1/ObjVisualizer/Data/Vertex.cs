namespace ObjVisualizer.Data
{
    internal class Vertex(double x, double y, double z, double w = 1.0)
    {
        public double X { get; set; } = x;
        public double Y { get; set; } = y;
        public double Z { get; set; } = z;
        public double W { get; set; } = w;
    }
}
