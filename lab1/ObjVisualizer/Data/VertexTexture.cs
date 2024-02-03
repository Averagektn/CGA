namespace ObjVisualizer.Data
{
    internal class VertexTexture(double u, double v = 0.0, double w = 0.0)
    {
        public double U { get; set; } = u;
        public double V { get; set; } = v;
        public double W { get; set; } = w;
    }
}
