using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
