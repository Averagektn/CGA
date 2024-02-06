using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Camera(Vector3 eye, Vector3 up, Vector3 target, float aspect, int fov, int zFar, int zNear)
    {
        public Vector3 Eye { get; set; } = eye;
        public Vector3 Up { get; set; } = up;
        public Vector3 Target { get; set; } = target;
        public float Aspect { get; set; } = aspect;
        public int FOV { get; set; } = fov;
        public int ZFar { get; set; } = zFar;
        public int ZNear { get; set; } = zNear;
    }
}
