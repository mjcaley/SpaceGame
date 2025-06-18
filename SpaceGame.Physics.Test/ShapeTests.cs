using System.Numerics;

namespace SpaceGame.Physics.Tests;

public class ShapeTests
{
    [Fact]
    public void CircleCircleColliding()
    {
        var c1 = new Shape.Circle(Vector2.Zero, 1f);
        var c2 = new Shape.Circle(new(.25f, 0f), 1f);

        Assert.True(CollisionResolver.Colliding(c1, c2));
    }
}