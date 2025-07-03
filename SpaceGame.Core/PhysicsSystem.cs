using System.Numerics;
//using nkast.Aether.Physics2D.Dynamics;
//using PhysicsVector2 = nkast.Aether.Physics2D.Common.Vector2;
using SpaceGame.Infrastructure;
using Box2D.NET;
using static Box2D.NET.B2Body;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Types;
using Flecs.NET.Core;
using SpaceGame.Core.Components;

namespace SpaceGame.Core;

public class PhysicsSystem
{
    public PhysicsSystem(World world)
    {
        _world = world;
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

    private World _world;
    public B2WorldId World { get; }

    public void OnAdd(ulong id, Vector2 position, Circle circle)
    {
        var definition = b2DefaultBodyDef();
        definition.userData = id;
        definition.type = B2BodyType.b2_dynamicBody;
        definition.position = new B2Vec2(position.X, position.Y);
        var body = b2CreateBody(World, ref definition);
                
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = 1f;
        shapeDef.material.friction = .5f;

        var shapeCircle = new B2Circle(new B2Vec2(circle.Center.X, circle.Center.Y), circle.Radius);

        b2CreateCircleShape(body, ref shapeDef, ref shapeCircle);

        _world.Entity(id).Set(new Box2DBodyId { BodyId = body });
    }

    public void OnRemove(ulong id)
    {
        var bodyId = _world.Entity(id).Get<Box2DBodyId>();
        if (bodyId == null)
        {
            return;
        }
        var world = b2GetWorldFromId(World);
        var body = b2GetBodyFullId(world, bodyId.BodyId);
        b2RemoveBodyFromIsland(world, body);

        _world.Entity(id).Remove<Box2DBodyId>();
    }

    public void Update(float deltaTime)
    {
        //World.Step(deltaTime);
    }
}
