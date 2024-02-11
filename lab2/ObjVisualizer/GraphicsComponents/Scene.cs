﻿using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal class Scene
    {
        public Camera Camera { get; set; }

        public Matrix4x4 ModelMatrix;
        public Matrix4x4 ViewMatrix;
        public Matrix4x4 ProjectionMatrix;
        public Matrix4x4 ViewPortMatrix;

        public bool ChangeStatus { get; set; }

        private static Scene? Instance;

        private Matrix4x4 RotateMatrix;
        private Matrix4x4 ScaleMatrix;
        private Matrix4x4 MoveMatrix;

        private Scene()
        {
            ModelMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ViewMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ProjectionMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ViewPortMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            RotateMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ScaleMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            MoveMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            Camera = new Camera(Vector3.Zero, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0);
            ChangeStatus = true;
        }

        public static Scene GetScene()
        {
            Instance ??= new Scene();

            return Instance;
        }

        public void UpdateViewMatix()
        {
            ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(Camera));
        }

        public void SceneResize(int NewWindowWidth, int NewWindowHeight)
        {
            Camera.ChangeCameraAspect(NewWindowWidth, NewWindowHeight);
            ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(NewWindowWidth, NewWindowHeight));
        }

        public Vector4 GetTransformedVertex(Vector4 Vertex, out Vector4 PreProjection)
        {
            Vertex = Vector4.Transform(Vertex, ViewMatrix);
            PreProjection = Vertex;
            Vertex = Vector4.Transform(Vertex, ProjectionMatrix);
            Vertex = Vector4.Divide(Vertex, Vertex.W);
            Vertex = Vector4.Transform(Vertex, ViewPortMatrix);

            return Vertex;
        }

        public void UpdateViewMatrix()
        {
            ViewMatrix = MatrixOperator.GetViewMatrix(Camera);
        }

        //public void UpdateModelMatrix()
        //{
        //    ModelMatrix = Matrix4x4.Transpose(MoveMatrix);
        //}

        public void ResetTransformMatrixes()
        {
            RotateMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            MoveMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ScaleMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
        }

        //public void UpdateMoveMatrix(Vector3 move)
        //{
        //    MoveMatrix = MatrixOperator.Move(move);
        //    UpdateModelMatrix();
        //}

        //public void UpdateRotateMatrix(Vector3 rotation)
        //{
        //    RotateMatrix = MatrixOperator.RotateX(rotation.X * Math.PI / 180.0)
        //        * MatrixOperator.RotateY(rotation.Y * Math.PI / 180.0);

        //    UpdateModelMatrix();
        //}

        //public void UpdateScaleMatrix(float deltaScale)
        //{
        //    ScaleMatrix = MatrixOperator.Scale(new Vector3(1 + deltaScale, 1 + deltaScale, 1 + deltaScale));
        //    UpdateModelMatrix();
        //}
    }
}
