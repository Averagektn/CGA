namespace ObjVisualizer.Data
{
    internal class Face(IEnumerable<Vertex> vertices, IEnumerable<VertexTexture> textures, IEnumerable<VertexNormal> normals)
    {
        public readonly IEnumerable<Vertex> Vertices = vertices;
        public readonly IEnumerable<VertexNormal> Normals = normals;
        public readonly IEnumerable<VertexTexture> Textures = textures;
    }
}
