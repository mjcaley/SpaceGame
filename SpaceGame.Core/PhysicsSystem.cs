using System.Numerics;
//using nkast.Aether.Physics2D.Dynamics;
//using PhysicsVector2 = nkast.Aether.Physics2D.Common.Vector2;
using SpaceGame.Infrastructure;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Worlds;

namespace SpaceGame.Core;

public class PhysicsSystem
{
    public PhysicsSystem()
    {
        var worldDef = new B2WorldDef
        {
            gravity = new B2Vec2()
        };
        World = b2CreateWorld(ref worldDef);
    }

    ~PhysicsSystem()
    {
        b2DestroyWorld(World);
    }

    //public World World { get; } = new(PhysicsVector2.Zero);
    public B2WorldId World { get; }

    public void OnAdd(int id, ref B2BodyDef bodyDefinition)
    {
        bodyDefinition.userData = id;
        var bodyId = b2CreateBody(World, ref bodyDefinition);
    }

    public void OnRemove(B2Body body)
    {
        //World.Remove(body);
    }

    public void Update(float deltaTime)
    {
        //World.Step(deltaTime);
    }
}
