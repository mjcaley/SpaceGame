using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using SpaceGame.Core.Components;
using System.Linq;
using System.Numerics;

using Vector2 = System.Numerics.Vector2;
using aVector2 = nkast.Aether.Physics2D.Common.Vector2;

namespace SpaceGame.Core;

public class PhysicsSystem
{
    private readonly World _world = new(aVector2.Zero);

    public void Update(float deltaTime)
    {
        _world.Step(deltaTime);
    }

    public void OnAdd(int id, Vector2 position, PhysicsBody body)
    {
        var newBody = _world.CreateBody(
            new aVector2(position.X, position.Y),
            bodyType: BodyType.Dynamic
        );
        newBody.CreateCircle(body.Shape.Radius, 0f);
        newBody.Tag = id;
    }

    public void OnRemove(int id)
    {
        foreach (var body in _world.BodyList.Where(b => b.Tag is int tag && tag == id))
        {
            _world.Remove(body);
        }       
    }
}
