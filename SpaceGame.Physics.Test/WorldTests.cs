using System.Numerics;

namespace SpaceGame.Physics.Tests;

public class WorldTests
{
    [Fact]
    public void BodyAdded()
    {
        var w = new World();
        var b = new Body
        {
            Kind = BodyKind.Dynamic,
            Shape = new Shape.Circle(Vector2.Zero, 1f),
        };
        var handle = w.Add(b);

        Assert.Same(w.Get(handle), b);
    }

    [Fact]
    public void BodyDeleted()
    {
        var w = new World();
        var b = new Body
        {
            Kind = BodyKind.Dynamic,
            Shape = new Shape.Circle(Vector2.Zero, 1f),
        };
        
        var handle = w.Add(b);
        Assert.Same(w.Get(handle), b);

        w.Remove(handle);
        Assert.Null(w.Get(handle));
    }
}