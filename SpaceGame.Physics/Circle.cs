using System.Numerics;

namespace SpaceGame.Physics;

public class Circle : IShape
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; }

    public BoundingBox GetBoundingBox()
    {
        return new BoundingBox
        {
            Origin = Center - new Vector2(Radius),
            Size = new Vector2(Radius * 2)
        };
    }

    public Vector2 Furthest(Vector2 direction)
    {
        if (direction == Vector2.Zero)
        {
            throw new ArgumentException("Direction cannot be zero", nameof(direction));
        }

        return Center + new Vector2(Radius) * Vector2.Normalize(direction);
    }
}
