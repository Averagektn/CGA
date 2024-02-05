using System.Numerics;

namespace ObjVisualizer.Data
{
    internal class Face(IEnumerable<Vector4> vertices, IEnumerable<Vector3> textures, IEnumerable<Vector3> normals)
    {
        public readonly IEnumerable<Vector4> Vertices = vertices;
        public readonly IEnumerable<Vector3> Normals = normals;
        public readonly IEnumerable<Vector3> Textures = textures;
    }
}
