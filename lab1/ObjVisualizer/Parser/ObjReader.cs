using ObjVisualizer.Data;
using System.Globalization;
using System.IO;

namespace ObjVisualizer.Parser
{
    internal sealed class ObjReader : IObjReader
    {
        public IEnumerable<Vertex> Vertices => _vertices;
        public IEnumerable<VertexTexture> VertexTextures => _vertexTextures;
        public IEnumerable<VertexNormal> VertexNormals => _vertexNormals;
        public IEnumerable<Face> Faces => _faces;

        private static readonly Dictionary<string, ObjReader> _readers = [];

        private readonly Dictionary<string, Action<string[]>> _actions;

        private readonly List<Vertex> _vertices = [];
        private readonly List<VertexTexture> _vertexTextures = [];
        private readonly List<VertexNormal> _vertexNormals = [];
        private readonly List<Face> _faces = [];

        private ObjReader(string filename)
        {
            _actions = new()
            {
                ["v"] = AddVertex,
                ["vt"] = AddVertexTexture,
                ["vn"] = AddVertexNormal,
                ["f"] = AddFace
            };

            using var reader = new StreamReader(filename);

            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                var data = line.Split(' ');

                if (_actions.TryGetValue(data[0], out Action<string[]>? value))
                {
                    value.Invoke(data);
                }
            }
        }

        public static IObjReader GetObjReader(string filename)
        {
            if (_readers.TryGetValue(filename, out var reader))
            {
                return reader;
            }

            reader = new(filename);

            _readers.Add(filename, reader);

            return reader;
        }

        private void AddVertex(string[] data)
        {
            if (data.Length == 6)
            {
                _vertices.Add(new(
                    double.Parse(data[2], CultureInfo.InvariantCulture),
                    double.Parse(data[3], CultureInfo.InvariantCulture),
                    double.Parse(data[4], CultureInfo.InvariantCulture),
                    double.Parse(data[5], CultureInfo.InvariantCulture)));
            }
            else if (data.Length == 5)
            {
                _vertices.Add(new(
                    double.Parse(data[2], CultureInfo.InvariantCulture),
                    double.Parse(data[3], CultureInfo.InvariantCulture),
                    double.Parse(data[4], CultureInfo.InvariantCulture)));
            }
        }

        private void AddVertexTexture(string[] data)
        {
            if (data.Length == 2)
            {
                _vertexTextures.Add(new(double.Parse(data[1], CultureInfo.InvariantCulture)));
            }
            else if (data.Length == 3)
            {
                _vertexTextures.Add(new(
                    double.Parse(data[1], CultureInfo.InvariantCulture),
                    double.Parse(data[2], CultureInfo.InvariantCulture)));
            }
            else if (data.Length == 4)
            {
                _vertexTextures.Add(new(
                    double.Parse(data[1], CultureInfo.InvariantCulture),
                    double.Parse(data[2], CultureInfo.InvariantCulture),
                    double.Parse(data[3], CultureInfo.InvariantCulture)));
            }
        }

        private void AddVertexNormal(string[] data)
        {
            _vertexNormals.Add(new(
                double.Parse(data[1], CultureInfo.InvariantCulture),
                double.Parse(data[2], CultureInfo.InvariantCulture),
                double.Parse(data[3], CultureInfo.InvariantCulture)));
        }

        private void AddFace(string[] data)
        {
            List<Vertex> vs = [];
            List<VertexNormal> vns = [];
            List<VertexTexture> vts = [];

            for (int i = 1; i < data.Length && data[i] != string.Empty; i++)
            {
                var elem = data[i].Split('/');

                int vId = int.Parse(elem[0]);
                if (vId != -1)
                {
                    vs.Add(_vertices[vId - 1]);
                }
                else
                {
                    vs.Add(_vertices[^1]);
                }

                int vtId;
                int vnId;

                if (elem.Length > 1)
                {
                    if (elem[1] != string.Empty)
                    {
                        vtId = int.Parse(elem[1]);
                        if (vtId != -1)
                        {
                            vts.Add(_vertexTextures[vtId - 1]);
                        }
                        else
                        {
                            vts.Add(_vertexTextures[^1]);
                        }
                    }
                    else
                    {
                        vnId = int.Parse(elem[2]);
                        if (vnId != -1)
                        {
                            vns.Add(_vertexNormals[vnId - 1]);
                        }
                        else
                        {
                            vns.Add(_vertexNormals[^1]);
                        }
                    }
                }
                if (elem.Length > 2)
                {
                    vnId = int.Parse(elem[2]);
                    if (vnId != -1)
                    {
                        vns.Add(_vertexNormals[vnId - 1]);
                    }
                    else
                    {
                        vns.Add(_vertexNormals[^1]);
                    }
                }

                _faces.Add(new(vs, vts, vns));
            }
        }
    }
}
