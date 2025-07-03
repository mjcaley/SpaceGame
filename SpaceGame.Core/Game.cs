using SpaceGame.Infrastructure;
using SpaceGame.Core.Components;
using Flecs.NET;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using System.Numerics;
using static SDL3.SDL;

namespace SpaceGame.Core;

public class Game
{
    public World World { get; private set; } = World.Create();
    public Entity PrePhysics { get; }
    public Entity OnPhysics { get; }
    public Entity PostPhysics { get; }
    public Entity PreDraw { get; }
    public Entity OnDraw { get; }

    private TimerEntity _fixedTickSource;

    private readonly RenderSystem _renderSystem;
    private readonly PhysicsSystem _physicsSystem;
    private readonly InputState _inputState = new();
    private readonly MovementSystem _movementSystem;

    public Game(IRenderer renderer)
    {
        PrePhysics = World.Entity().Add(Ecs.Phase).Add(Ecs.DependsOn, Ecs.PostUpdate);
        OnPhysics = World.Entity().Add(Ecs.Phase).Add(Ecs.DependsOn, PrePhysics);
        PostPhysics = World.Entity().Add(Ecs.Phase).Add(Ecs.DependsOn, OnPhysics);
        PreDraw = World.Entity().Add(Ecs.Phase).Add(Ecs.DependsOn, PostPhysics);
        OnDraw = World.Entity().Add(Ecs.Phase).Add(Ecs.DependsOn, PreDraw);

        _fixedTickSource = World.Timer().Interval(.2f);

        _renderSystem = new(renderer);
        _movementSystem = new(_inputState);
        _physicsSystem = new(World);
    }

    private void AddMockEntities()
    {
        World.Entity("Player")
            .Set(new Transform { Position = new Vector2(100f, 100f) })
            .Set(new PhysicsBody { Shape = new Circle() { Center = Vector2.Zero, Radius = 10f } })
            .Set(new Components.Rectangle { Layer = Layer.Foreground, Colour = new Vector4(0f, 0f, 1.0f, 1.0f), Size = new Vector2(16, 16) })
            .Set(new Player { Speed = 100f });

        World.Entity("Enemy")
            .Set(new Transform { Position = Vector2.Zero })
            .Set(new Components.Rectangle { Layer = Layer.Foreground, Colour = new Vector4(1.0f, 0f, 0f, 1.0f), Size = new Vector2(16f, 16f) });
    }

    private void Setup()
    {
        World.Observer<PhysicsBody>()
            .Event(Ecs.OnSet)
            .Each((Iter it, int i, ref PhysicsBody b) =>
            {
                if (it.Entity(i).Has<Transform>())
                {
                    var t = it.Entity(i).Get<Transform>();
                    _physicsSystem.OnAdd(it.Entity(i), t.Position, b.Shape);
                }
                else
                {
                    _physicsSystem.OnAdd(it.Entity(i), Vector2.Zero, b.Shape);
                }
            });

        World.Observer<PhysicsBody>()
            .Event(Ecs.OnRemove)
            .Each((Iter it, int i, ref PhysicsBody _) =>
            {
                _physicsSystem.OnRemove(it.Entity(i));
            });

        World.Observer<Transform>()
            .Event<PhysicsPositionChanged>()
            .Each((Iter it, int i, ref Transform t) =>
            {
                
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

        World.System<Transform, Box2DBodyId>("Physics update")
            .Kind(PrePhysics)
            .TickSource(_fixedTickSource)
            .Each((Iter it, int i, ref Transform t, ref Box2DBodyId b) =>
            {
                if (t.Velocity != Vector2.Zero)
                {
                    _physicsSystem.ApplyForce(b.BodyId, t.Velocity);
                }
            });

        World.System()
            .Kind(OnPhysics)
            .TickSource(_fixedTickSource)
            .Iter((Iter it) => {
                _physicsSystem.Update(it.DeltaTime());
            });

        World.System<Transform, Box2DBodyId>("Update position from physics")
            .Kind(PostPhysics)
            .Each((Iter it, int i, ref Transform t, ref Box2DBodyId b) =>
            {
                var physicsPosition = _physicsSystem.GetPosition(b.BodyId);
                if (physicsPosition != t.Position)
                {
                    t.Position = new Vector2(physicsPosition.X, physicsPosition.Y);
                }
                Console.WriteLine($"Entity {it.Entity(i).Name} position updated to {t.Position}");
            });

        World.System<Transform, Components.Rectangle>()
            .Kind(PreDraw)
            .Each((Iter it, int i, ref Transform t, ref Components.Rectangle r) =>
            {
                _renderSystem.Add(t, r);
            });

        World.System("Draw")
            .Kind(OnDraw)
            .Iter(_renderSystem.Draw);

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
