using System.Numerics;

namespace SpaceGame.Physics;

public record CollisionResult
{
    public Shape ShapeA { get; init; }
    public Shape ShapeB { get; init; }
    public Vector2 Normal { get; init; }
    public float Depth { get; init; }
}
