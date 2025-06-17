using System.Numerics;

namespace SpaceGame.Physics;

public class BoundingBox
{
    public Vector2 Origin { get; init; }
    public Vector2 Size { get; init; }

    public bool Overlaps(BoundingBox other)
    {
        return !(this.Origin.X > other.Origin.X + other.Size.X ||
                 this.Origin.X + this.Size.X < other.Origin.X ||
                 this.Origin.Y > other.Origin.Y + other.Size.Y ||
                 this.Origin.Y + this.Size.Y < other.Origin.Y);
    }

    public bool Contains(BoundingBox other)
    {
        return this.Origin.X <= other.Origin.X &&
               this.Origin.X + this.Size.X >= other.Origin.X + other.Size.X &&
               this.Origin.Y <= other.Origin.Y &&
               this.Origin.Y + this.Size.Y >= other.Origin.Y + other.Size.Y;
    }
}
