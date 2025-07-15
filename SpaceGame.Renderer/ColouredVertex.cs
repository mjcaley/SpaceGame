using System.Numerics;
using System.Runtime.InteropServices;

namespace SpaceGame.Renderer;

[StructLayout(LayoutKind.Sequential)]
public struct ColouredVertex(Vector2 position, Vector4 colour)
{
    public Vector2 Position { get; set; } = position;
    public Vector4 Colour { get; set; } = colour;
}
