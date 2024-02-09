using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Camera(Vector3 eye, Vector3 up, Vector3 target, float aspect, float fov, float zFar, float zNear)
    {
        public Vector3 Eye { get; set; } = eye;
        public Vector3 Up { get; set; } = up;
        public Vector3 Target { get; set; } = target;
        public float Aspect { get; set; } = aspect;
        public float FOV { get; set; } = fov;
        public float ZFar { get; set; } = zFar;
        public float ZNear { get; set; } = zNear;

        public void ChangeCameraAspect(int Width, int Height)
        {
            Aspect = (float)Width / Height;
        }
    }
}
