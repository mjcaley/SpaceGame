using System.Numerics;

namespace SpaceGame.Renderer;

public struct Camera(Matrix4x4 view, Matrix4x4 projection)
{
    public Matrix4x4 View = view;
    public Matrix4x4 Projection = projection;
}
