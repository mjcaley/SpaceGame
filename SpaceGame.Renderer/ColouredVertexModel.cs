using System.Numerics;
using System.Runtime.InteropServices;

namespace SpaceGame.Renderer;

[StructLayout(LayoutKind.Sequential)]
internal struct ColouredVertexModel()
{
    public required ColouredVertex V1 { get; set; }
    public required ColouredVertex V2 { get; set; }
    public required ColouredVertex V3 { get; set; }
    public required ColouredVertex V4 { get; set; }
    public required ColouredVertex V5 { get; set; }
    public required ColouredVertex V6 { get; set; }
    public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;
}
