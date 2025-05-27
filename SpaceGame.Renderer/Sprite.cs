using System.Linq;
using Silk.NET.Maths;

namespace SpaceGame.Renderer;

public class Sprite
{
    public Vector2D<double> Origin { get; set; } = Vector2D<double>.Zero;
    public Vector2D<double> Size { get; set; } = Vector2D<double>.Zero;
    public Vector4D<double> Colour { get; set; } = Vector4D<double>.Zero;

    public List<Vector2D<double>> Vertices()
    {
        var vertices = new List<Vector2D<double>>
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
    //     return Vertices().Select<Vector2D<double>, double[]>(vertex => return [vertex.X, vertex.Y]);
    // }
}
