using System.Numerics;

namespace ObjVisualizer.Data
{
    internal  struct LineSegment(Vector3 left, Vector3 right)
    {
        public  Vector3 Left = left;
        public  Vector3 Right = right;
    }
}
