using System.Numerics;

namespace SpaceGame.Physics;

public abstract record Shape
{
    public static BoundingBox GetBoundingBox(Shape shape) =>
        shape switch
        {
            Circle circle => circle.GetBoundingBox(),
            _ => throw new NotSupportedException("Shape not supported")
        };
    
    public record Circle(Vector2 center, float radius) : Shape, IBoundary
    {
        public Vector2 Center => center;
        public float Radius => radius;

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
}
