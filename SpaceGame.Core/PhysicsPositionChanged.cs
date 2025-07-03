using System.Numerics;

namespace SpaceGame.Core;

public record PhysicsPositionChanged
{
    public Vector2 Position { get; init; }
}
