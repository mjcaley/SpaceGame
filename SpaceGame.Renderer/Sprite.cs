using System.Linq;
using System.Numerics;

namespace SpaceGame.Renderer;

public class Sprite
{
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.Zero;
    public Vector4 Colour { get; set; } = Vector4.Zero;

    public List<Vector2> Vertices()
    {
        var vertices = new List<Vector2>
        {
            new(Origin.X, Origin.Y),
            new(Origin.X + Size.X, Origin.Y),
            new(Origin.X + Size.X, Origin.Y + Size.Y),
            new(Origin.X, Origin.Y),
            new(Origin.X + Size.X, Origin.Y + Size.Y),
            new(Origin.X, Origin.Y + Size.Y)
        };

        return vertices;
    }

    // public double[] ToArray()
    // {
    //     return Vertices().Select<Vector2, double[]>(vertex => return [vertex.X, vertex.Y]);
    // }
}
