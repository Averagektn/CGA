using System.Numerics;

namespace ObjVisualizer.Data
{
    internal class Face(IEnumerable<int> vertices, IEnumerable<int> textures, IEnumerable<int> normals)
    {
        public readonly IEnumerable<int> VertexIds = vertices;
        public readonly IEnumerable<int> TextureIds = textures;
        public readonly IEnumerable<int> NormalIds = normals;
    }
}
