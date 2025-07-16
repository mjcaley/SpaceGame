using System.Numerics;
using SpaceGame.Core.Components;

namespace SpaceGame.Core;

public class MovementSystem(InputState inputState)
{
    public void Update(Player player, Transform transform, float deltaTime)
    {
        Vector2 direction = Vector2.Zero;

        if (inputState.UpPressed)
        {
            direction.Y += 1f;
        }
        if (inputState.DownPressed)
        {
            direction.Y -= 1f;
        }
        if (inputState.LeftPressed)
        {
            direction.X -= 1f;
        }
        if (inputState.RightPressed)
        {
            direction.X += 1f;
        }
        if (direction != Vector2.Zero)
        {
            direction = Vector2.Normalize(direction);
        }

        transform.Velocity = direction * player.Speed * deltaTime;
    }
}
