using System.Numerics;

namespace SpaceGame.Core.Components;

public class Transform
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
}
