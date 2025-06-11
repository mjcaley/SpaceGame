using SpaceGame.Infrastructure;

namespace SpaceGame.Renderer
{
    internal static class VertexExtensions
    {
        public static List<ColouredVertex> ToVertices(this Rectangle rectangle)
        {
            return
            [
                new(new(rectangle.Origin.X, rectangle.Origin.Y), rectangle.Colour),
                new(rectangle.Origin with { X = rectangle.Origin.X + rectangle.Size.X }, rectangle.Colour),
                new(new(rectangle.Origin.X + rectangle.Size.X, rectangle.Origin.Y + rectangle.Size.Y), rectangle.Colour),
                new(new(rectangle.Origin.X, rectangle.Origin.Y), rectangle.Colour),
                new(new(rectangle.Origin.X + rectangle.Size.X, rectangle.Origin.Y + rectangle.Size.Y), rectangle.Colour),
                new(rectangle.Origin with { Y = rectangle.Origin.Y + rectangle.Size.Y }, rectangle.Colour)
            ];
        }
    }
}
