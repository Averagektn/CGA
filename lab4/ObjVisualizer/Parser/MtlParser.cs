using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ObjVisualizer.Parser
{
    internal class MtlParser(string filePath) : IMtlParser
    {

        private static readonly Dictionary<string, ImageFormat> _formatDictionary = new()
        {
            { ".bmp", ImageFormat.Bmp },
            { ".gif", ImageFormat.Gif },
            { ".jpg", ImageFormat.Jpeg },
            { ".jpeg", ImageFormat.Jpeg },
            { ".png", ImageFormat.Png },
            { ".tiff", ImageFormat.Tiff }
        };

        private readonly string _mtlName = Path.GetFileName(filePath);
        private readonly string _mtlDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

        private const string MAP_KD = "map_kd";
        private const string MAP_MRAO = "map_mrao";
        private const string NORM = "norm";

        public byte[] GetMapKdBytes() => GetBitmapBytes(MAP_KD);

        public byte[] GetMapMraoBytes() => GetBitmapBytes(MAP_MRAO);

        public byte[] GetNormBytes() => GetBitmapBytes(NORM);

        private byte[] GetBitmapBytes(string paramName)
        {
            string line;

            using var reader = new StreamReader(_mtlDirectory + Path.DirectorySeparatorChar + _mtlName);
            {
                do
                {
                    line = reader.ReadLine() ?? string.Empty;
                }
                while (!line.Contains(paramName, StringComparison.InvariantCultureIgnoreCase) && line != string.Empty);
            }

            var fileName = line.Split(' ')[1];
            var fileExtension = Path.GetExtension(fileName);
            var bitmap = new Bitmap(_mtlDirectory + Path.DirectorySeparatorChar + fileName);

            using var stream = new MemoryStream();
            bitmap.Save(stream, _formatDictionary[fileExtension]);

            return stream.ToArray();
        }
    }
}
