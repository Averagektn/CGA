using ObjVisualizer.Data;

namespace ObjVisualizer.Parser
{
    internal interface IObjReader
    {
        IEnumerable<Vertex> Vertices { get; }
        IEnumerable<VertexTexture> VertexTextures { get; }
        IEnumerable<VertexNormal> VertexNormals { get; }
        IEnumerable<Face> Faces { get; }
    }
}
