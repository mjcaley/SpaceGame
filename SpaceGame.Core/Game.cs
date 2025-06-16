using SpaceGame.Infrastructure;
using SpaceGame.Core.Components;
using Flecs.NET;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using System.Numerics;
using static SDL3.SDL;

namespace SpaceGame.Core;

public class Game(IRenderer renderer)
{
    public World World { get; private set; } = World.Create();
    private readonly RenderSystem _renderSystem = new(renderer);
    private readonly InputState _inputState = new();

    private void AddMockEntities()
    {
        World.Entity("Player")
            .Add<Transform>()
            .Add<Components.Rectangle>()
            .Set(new Transform { Position = new Vector2(100f, 100f) })
            .Set(new Components.Rectangle { Layer = Layer.Foreground, Colour = new Vector4(0f, 0f, 1.0f, 1.0f), Size = new Vector2(16, 16) })
            .Set(new Player { Speed = 100f });
        
        World.Entity("Enemy")
            .Add<Transform>()
            .Add<Components.Rectangle>()
            .Set(new Transform { Position = Vector2.Zero })
            .Set(new Components.Rectangle { Layer = Layer.Foreground, Colour = new Vector4(1.0f, 0f, 0f, 1.0f), Size = new Vector2(16f, 16f) });
    }

    private void Setup()
    {
        AddMockEntities();

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
                Vector2 direction = Vector2.Zero;

                if (_inputState.UpPressed)
                {
                    direction.Y += 1f;
                }
                if (_inputState.DownPressed)
                {
                    direction.Y -= 1f;
                }
                if (_inputState.LeftPressed)
                {
                    direction.X -= 1f;
                }
                if (_inputState.RightPressed)
                {
                    direction.X += 1f;
                }
                if (direction != Vector2.Zero)
                {
                    direction = Vector2.Normalize(direction);
                }

                t.Position += direction * p.Speed * it.DeltaTime();
            });

        World.System<Transform, Components.Rectangle>("Batch sprites")
            .Kind(Ecs.PostUpdate)
            .Each((Iter it, int i, ref Transform t, ref Components.Rectangle s) =>
            {
                _renderSystem.Add(t, s);
            });

        World.System("Frame timer")
            .Kind(Ecs.OnUpdate)
            .Iter((Iter it) =>
            {
                Console.WriteLine("timer");
                Console.WriteLine($"Delta time is {it.DeltaTime()}");
            });

        World.System("Draw")
            .Kind(Ecs.PostFrame)
            .Iter(() => _renderSystem.Draw());

        World.SetTargetFps(60);
    }

    public void Run()
    {
        Console.WriteLine("Hello, Space Game!");
        Setup();

        while (World.Progress()) {
            if (World.ShouldQuit())
            {
                break;
            }
        }
    }
}
