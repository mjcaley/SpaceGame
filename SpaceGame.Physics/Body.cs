using System.Numerics;

namespace SpaceGame.Physics;

public class Body
{
    public BodyKind Kind { get; set; }
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public required Shape Shape { get; set; }
}
