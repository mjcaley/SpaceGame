using SpaceGame.Infrastructure;
using SpaceGame.Core.Components;
using Flecs.NET;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using System.Numerics;
using static SDL3.SDL;
using Body = nkast.Aether.Physics2D.Dynamics.Body;

namespace SpaceGame.Core;

public class Game
{
    public World World { get; private set; } = World.Create();
    public Entity PrePhysics { get; }
    public Entity OnPhysics { get; }
    public Entity PostPhysics { get; }

    private TimerEntity _fixedTickSource;

    private readonly RenderSystem _renderSystem;
    private readonly PhysicsSystem _physicsSystem = new();
    private readonly InputState _inputState = new();
    private readonly MovementSystem _movementSystem;

    public Game(IRenderer renderer)
    {
        PrePhysics = World.Entity().Add(Ecs.Phase).Add(Ecs.DependsOn, Ecs.PostUpdate);
        OnPhysics = World.Entity().Add(Ecs.Phase).Add(Ecs.DependsOn, PrePhysics);
        PostPhysics = World.Entity().Add(Ecs.Phase).Add(Ecs.DependsOn, OnPhysics);

        _fixedTickSource = World.Timer().Interval(.2f);

        _renderSystem = new(renderer);
        _movementSystem = new(_inputState);
    }

    private void AddMockEntities()
    {
        var playerBody = new Body()
        {
            BodyType = nkast.Aether.Physics2D.Dynamics.BodyType.Dynamic
        };
        playerBody.CreateCircle(.5f, 1f);
        playerBody.Position = new nkast.Aether.Physics2D.Common.Vector2(100f, 100f);

        World.Entity("Player")
            .Set(new Transform { Position = new Vector2(100f, 100f) })
            .Set(playerBody)
            .Set(new Components.Rectangle { Layer = Layer.Foreground, Colour = new Vector4(0f, 0f, 1.0f, 1.0f), Size = new Vector2(16, 16) })
            .Set(new Player { Speed = 100f });

        var enemyBody = new Body()
        {
            BodyType = nkast.Aether.Physics2D.Dynamics.BodyType.Dynamic
        };
        playerBody.CreateCircle(.5f, 1f);
        World.Entity("Enemy")
            .Set(new Transform { Position = Vector2.Zero })
            .Set(enemyBody)
            .Set(new Components.Rectangle { Layer = Layer.Foreground, Colour = new Vector4(1.0f, 0f, 0f, 1.0f), Size = new Vector2(16f, 16f) });
    }

    private void Setup()
    {
        World.Observer<Body>()
            .Event(Ecs.OnSet)
            .Each((Iter it, int i, ref Body b) =>
            {
                _physicsSystem.OnAdd(i, b);
            });

        World.Observer<Body>()
            .Event(Ecs.OnRemove)
            .Each((Iter it, int i, ref Body b) =>
            {
                _physicsSystem.OnRemove(b);
            });

        World.System("Input")
            .Kind(Ecs.PreUpdate)
            .Iter((Iter it) =>
            {
                while (PollEvent(out var @event))
                {
                    switch (@event.Type)
                    {
                        case (uint)EventType.KeyDown:
                            switch (@event.Key.Scancode)
                            {
                                case Scancode.Escape:
                                    World.Quit(); break;
                                case Scancode.W:
                                    _inputState.UpPressed = true; break;
                                case Scancode.A:
                                    _inputState.LeftPressed = true; break;
                                case Scancode.S:
                                    _inputState.DownPressed = true; break;
                                case Scancode.D:
                                    _inputState.RightPressed = true; break;
                            }
                            break;
                        case (uint)EventType.KeyUp:
                            switch (@event.Key.Scancode)
                            {
                                case Scancode.W:
                                    _inputState.UpPressed = false; break;
                                case Scancode.A:
                                    _inputState.LeftPressed = false; break;
                                case Scancode.S:
                                    _inputState.DownPressed = false; break;
                                case Scancode.D:
                                    _inputState.RightPressed = false; break;
                            }
                            break;
                    }
                }
            });
        
        World.System("Clear sprite batch")
            .Kind(Ecs.PreUpdate)
            .Iter((Iter it) => _renderSystem.Clear());

        World.System<Transform, Player>("Player movement")
            .Each((Iter it, int i, ref Transform t, ref Player p) =>
            {
                _movementSystem.Update(p, t, it.DeltaTime());
            });

        World.System<Transform, Body>("Physics update")
            .Kind(PrePhysics)
            .TickSource(_fixedTickSource)
            .Each((Iter it, int i, ref Transform t, ref Body b) =>
            {
                b.ApplyForce(new nkast.Aether.Physics2D.Common.Vector2(t.Velocity.X, t.Velocity.Y));
                Console.WriteLine($"Physics force: {b.AngularVelocity} {b.LinearVelocity}");
            });

        World.System()
            .Kind(OnPhysics)
            .TickSource(_fixedTickSource)
            .Iter((Iter it) => {
                _physicsSystem.Update(it.DeltaTime());
            });

        World.System<Transform, Components.Rectangle, Body>("Batch sprites")
            .Kind(PostPhysics)
            .Each((Iter it, int i, ref Transform t, ref Components.Rectangle s, ref Body b) =>
            {
                Console.WriteLine($"{i} {t.Position} to {b.Position}");
                t.Position = new Vector2(b.Position.X, b.Position.Y);
                _renderSystem.Add(t, s);
            });

        World.System("Frame timer")
            .Kind(Ecs.OnUpdate)
            .Iter((Iter it) =>
            {
                // Console.WriteLine($"Delta time is {it.DeltaTime()}");
            });

        World.System("Draw")
            .Kind(Ecs.PostFrame)
            .Iter(() => _renderSystem.Draw());

        World.SetTargetFps(60);
    }

    public void Run()
    {
        Setup();
        AddMockEntities();

        while (World.Progress()) {
            if (World.ShouldQuit())
            {
                break;
            }
        }
    }
}
