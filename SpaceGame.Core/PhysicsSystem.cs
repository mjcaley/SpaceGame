using System.Numerics;
using SpaceGame.Infrastructure;
using Box2D.NET;
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
        var worldDef = b2DefaultWorldDef();
        worldDef.gravity = new B2Vec2(0, 0);
        WorldId = b2CreateWorld(ref worldDef);
    }

    ~PhysicsSystem()
    {
        b2DestroyWorld(WorldId);
    }

    private World _world;
    public B2WorldId WorldId { get; }

    public void OnAdd(Entity id, Vector2 position, Circle circle)
    {
        var definition = b2DefaultBodyDef();
        definition.userData = id;
        definition.type = B2BodyType.b2_dynamicBody;
        definition.position = new B2Vec2(position.X, position.Y);
        var body = b2CreateBody(WorldId, ref definition);

        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = 1f;
        shapeDef.material.friction = .5f;

        var shapeCircle = new B2Circle(new B2Vec2(circle.Center.X, circle.Center.Y), circle.Radius);

        b2CreateCircleShape(body, ref shapeDef, ref shapeCircle);

        var massData = new B2MassData
        {
            mass = 10f,
            center = new B2Vec2(circle.Center.X, circle.Center.Y),
            rotationalInertia = 100f
        };
        b2Body_SetMassData(body, massData);

        id.Set(body);
    }

    public void OnRemove(Entity id)
    {
        var bodyId = id.Get<B2BodyId>();
        if (bodyId == null)
        {
            return;
        }
        b2DestroyBody(bodyId);

        id.Remove<B2BodyId>();
    }

    public void Update(float deltaTime)
    {
        b2World_Step(WorldId, deltaTime, 4);
        _world.Defer(() =>
        {
            var moveEvents = b2World_GetBodyEvents(WorldId);
            for (var i = 0; i < moveEvents.moveCount; i++)
            {
                var moveEvent = moveEvents.moveEvents[i];
                var entity = (Entity)moveEvent.userData;
                var position = new Vector2(moveEvent.transform.p.X, moveEvent.transform.p.Y);

                entity.Set(new PhysicsPositionChanged { Position = position });
            }
        });
    }

    public Vector2 GetPosition(B2BodyId id)
    {
        var position = b2Body_GetPosition(id);
        return new Vector2(position.X, position.Y);
    }

    public void ApplyForce(B2BodyId id, Vector2 force)
    {
        Console.WriteLine("applying forice");
        b2Body_ApplyForceToCenter(id, new B2Vec2(force.X, force.Y), true);
    }
}
