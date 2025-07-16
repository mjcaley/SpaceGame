using System.Numerics;
using System.Runtime.InteropServices;

namespace SpaceGame.Renderer;

[StructLayout(LayoutKind.Sequential)]
internal struct QuadInstanceDetails
{
    public required Vector4 Colour { get; set; }
    public required Matrix4x4 Model { get; set; }
}
