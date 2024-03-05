namespace ObjVisualizer.Parser
{
    internal interface IMtlParser
    {
        byte[] GetMapKdBytes();
        byte[] GetMapMraoBytes();
        byte[] GetNormBytes();
    }
}
