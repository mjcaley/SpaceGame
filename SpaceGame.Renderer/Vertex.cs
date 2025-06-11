using System.Numerics;

namespace SpaceGame.Renderer
{
    public struct Vertex(Vector2 position, Vector4 colour)
    {
        public Vector2 Position { get; set; } = position;
        public Vector4 Colour { get; set; } = colour;
    }
}
