using System.Numerics;
using System.Runtime.InteropServices;

namespace SpaceGame.Renderer;

[StructLayout(LayoutKind.Sequential)]
internal struct QuadVertices
{
    public Vector2 V1 { get; set; }
    public Vector2 V2 { get; set; }
    public Vector2 V3 { get; set; }
    public Vector2 V4 { get; set; }
    public Vector2 V5 { get; set; }
    public Vector2 V6 { get; set; }

    public static explicit operator List<Vector2>(QuadVertices quad) =>
    [
        quad.V1,
        quad.V2,
        quad.V3,
        quad.V4,
        quad.V5,
        quad.V6
    ];
}
