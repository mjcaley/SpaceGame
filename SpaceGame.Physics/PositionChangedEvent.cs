using System.Numerics;

namespace SpaceGame.Physics;

public class PositionChangedEvent(BodyHandle handle, Vector2 position) : EventArgs
{
    public BodyHandle Handle { get; } = handle;
    public Vector2 Position { get; } = position;
}
