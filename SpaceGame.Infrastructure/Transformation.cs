using System.Numerics;

namespace SpaceGame.Infrastructure;

public class Transformation
{
    public Vector2 Translate { get; set; } = Vector2.Zero;
    public float Rotate { get; set; } = 0f; // Radians
}
