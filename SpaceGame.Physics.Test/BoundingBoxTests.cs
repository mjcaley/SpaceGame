namespace SpaceGame.Physics.Tests;

public class BoundingBoxTests
{
    [Fact]
    public void Overlaps()
    {
        var b1 = new BoundingBox
        {
            Origin = new(0f, 0f),
            Size = new(1f, 1f)
        };
        var b2 = new BoundingBox
        {
            Origin = new(0.5f, 0f),
            Size = new(1f, 1f)
        };

        Assert.True(b1.Overlaps(b2));
    }

    [Theory]
    [InlineData(1.1f, 0f)]
    [InlineData(-1.1f, 0f)]
    [InlineData(0f, 1.1f)]
    [InlineData(0f, -1.1f)]
    public void DoesntOverlap(float b2OriginX, float b2OriginY)
    {
        var b1 = new BoundingBox
        {
            Origin = new(0f, 0f),
            Size = new(1f, 1f)
        };
        var b2 = new BoundingBox
        {
            Origin = new(b2OriginX, b2OriginY),
            Size = new(1f, 1f)
        };

        Assert.False(b1.Overlaps(b2));
    }

    [Fact]
    public void Contains()
    {
        var b1 = new BoundingBox
        {
            Origin = new(0f, 0f),
            Size = new(1f, 1f)
        };
        var b2 = new BoundingBox
        {
            Origin = new(.5f, .5f),
            Size = new(.5f, .5f)
        };

        Assert.True(b1.Contains(b2));
    }

    [Fact]
    public void DoesntContain()
    {
        var b1 = new BoundingBox
        {
            Origin = new(0f, 0f),
            Size = new(1f, 1f)
        };
        var b2 = new BoundingBox
        {
            Origin = new(2.5f, 2.5f),
            Size = new(1f, 1f)
        };

        Assert.False(b1.Contains(b2));
    }
}
