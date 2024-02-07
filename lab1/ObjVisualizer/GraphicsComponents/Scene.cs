using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Scene
    {
        private static Scene? instance;
        public Camera camera;
        public Matrix4x4 ModelMatrix;
        public Matrix4x4 ViewMatrix;
        public Matrix4x4 ProjectionMatrix;
        public Matrix4x4 ViewPortMatrix;
        public bool ChangeStatus;
        private Matrix4x4 RotateMatrix;
        private Matrix4x4 ScaleMatrix;
        private Matrix4x4 MoveMatrix;

        private Scene()
        {
            ModelMatrix = Matrix4x4.Identity;
            ViewMatrix = Matrix4x4.Identity;
            ProjectionMatrix = Matrix4x4.Identity;
            ViewPortMatrix = Matrix4x4.Identity;
            RotateMatrix = Matrix4x4.Identity;
            ScaleMatrix = Matrix4x4.Identity;
            MoveMatrix = Matrix4x4.Identity;
            camera = new Camera(Vector3.Zero, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0);
            ChangeStatus = true;
        }

        public static Scene GetScene()
        {
            if (instance == null)
                instance = new Scene();
            return instance;
        }

        public void SceneResize(int NewWindowWidth, int NewWindowHeight)
        {
            camera.ChangeCameraAspect(NewWindowWidth, NewWindowHeight);
            ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(NewWindowWidth, NewWindowHeight));

        }

        public Vector4 GetTransformedVertex(Vector4 Vertex)
        {
            Vertex = Vector4.Transform(Vertex, ViewMatrix);
            Vertex = Vector4.Transform(Vertex, ProjectionMatrix);
            Vertex = Vector4.Divide(Vertex, Vertex.W);
            Vertex = Vector4.Transform(Vertex, ViewPortMatrix);
            return Vertex;
        }

        public void UpdateModelMatrix()
        {
            ModelMatrix = ScaleMatrix * RotateMatrix * MoveMatrix;
        }

        public void ResetTransformMatrixes()
        {
            RotateMatrix = Matrix4x4.Identity;
            MoveMatrix = Matrix4x4.Identity;
            ScaleMatrix = Matrix4x4.Identity;
        }

        public void UpdateRotateMatrix(Vector3 rotation)
        {
            //RotateMatrix = MatrixOperator.RotateX(angleX * Math.PI / 180.0) * MatrixOperator.RotateY(angleY * Math.PI / 180.0) * MatrixOperator.RotateZ(angleZ * Math.PI / 180.0);
            RotateMatrix = MatrixOperator.RotateX(rotation.X * Math.PI / 180.0)*MatrixOperator.RotateY(rotation.Y * Math.PI / 180.0);
            UpdateModelMatrix();
        }

        public void UpdateScaleMatrix(float deltaScale)
        {
            ScaleMatrix = MatrixOperator.Scale(new Vector3(1 + deltaScale, 1 + deltaScale, 1 + deltaScale));
            UpdateModelMatrix();
        }

    }
}
