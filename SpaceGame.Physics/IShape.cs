using System.Numerics;

namespace SpaceGame.Physics;

public interface IShape
{
    Vector2 Center { get; set; }
    BoundingBox GetBoundingBox();
    Vector2 Furthest(Vector2 direction);
}
