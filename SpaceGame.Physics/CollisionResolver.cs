using System.Numerics;

namespace SpaceGame.Physics;

public static class CollisionResolver
{
    public static bool Colliding(Shape s1, Shape s2) =>
        (s1, s2) switch
        {
            (Shape.Circle c1, Shape.Circle c2) => CircleColliding(c1, c2),
            _ => throw new NotSupportedException($"Shape type {s1.GetType()} and {s2.GetType()} are not supported for collision detection.")
        };

    private static bool CircleColliding(Shape.Circle c1, Shape.Circle c2) =>
        Vector2.Distance(c1.Center, c2.Center) < c1.Radius + c2.Radius;

    public static CollisionResult? Resolve(Shape s1, Shape s2)
    {
        if (!Colliding(s1, s2))
        {
            return null;
        }
        return (s1, s2) switch
        {
            (Shape.Circle c1, Shape.Circle c2) => ResolveCircleCollision(c1, c2),
            _ => throw new NotSupportedException($"Shape type {s1.GetType()} and {s2.GetType()} are not supported for collision resolution.")
        };
    }

    public static bool TryResolve(Shape s1, Shape s2, out CollisionResult? result)
    {
        result = Resolve(s1, s2);
        return result is not null;
    }

    private static CollisionResult ResolveCircleCollision(Shape.Circle c1, Shape.Circle c2)
    {
        var depth = c1.Radius + c1.radius - Vector2.Distance(c1.Center, c2.Center);
        var direction = Vector2.Normalize(c1.Center - c2.Center);

        return new()
        {
            ShapeA = c1,
            ShapeB = c2,
            Normal = direction,
            Depth = depth
        };
    }
}
