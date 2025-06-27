using System.Numerics;
using nkast.Aether.Physics2D.Dynamics;
using PhysicsVector2D = nkast.Aether.Physics2D.Common.Vector2;
using SpaceGame.Infrastructure;

namespace SpaceGame.Core;

public class PhysicsSystem
{
    public World World { get; } = new();

    public void OnAdd(int id, Body body)
    {
        body.Tag = id;
        World.Add(body);
    }

    public void OnRemove(Body body)
    {
        World.Remove(body);
    }

    public void Update(float deltaTime)
    {
        World.Step(deltaTime);
    }
}
