using ObjVisualizer.Data;

namespace ObjVisualizer.Parser
{
    internal interface IMtlParser
    {
        ImageData GetMapKdBytes();
        ImageData GetMapMraoBytes();
        ImageData GetNormBytes();
    }
}
