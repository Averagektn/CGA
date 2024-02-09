namespace ObjVisualizer.MouseHandlers
{
    internal class MouseHandler
    {
        public static Actions LastAction = Actions.Idle;

        public enum Actions
        {
            XRotation,
            YRotation,
            ZRotation,
            Idle
        }
    }
}
