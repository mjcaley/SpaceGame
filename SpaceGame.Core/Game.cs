using SpaceGame.Infrastructure;
using SpaceGame.Core.Components;
using Flecs.NET;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using System.Numerics;
using SDL3;
using static SDL3.SDL;

namespace SpaceGame.Core;

public class Game(IRenderer renderer)
{
    public World World { get; private set; } = World.Create();
    private RenderSystem renderSystem = new(renderer);

    private void AddMockEntities()
    {
        World.Entity("Player")
            .Add<Transform>()
            .Add<Sprite>()
            .Set(new Transform { Position = new Vector2(0.2f, 0.5f) })
            .Set(new Sprite { Texture = "player_texture", Layer = Layer.Foreground, Size = new Vector2(0.1f, 0.1f) });
        World.Entity("Enemy")
            .Add<Transform>()
            .Add<Sprite>()
            .Set(new Transform { Position = new Vector2(0.6f, 0.5f) })
            .Set(new Sprite { Texture = "player_texture", Layer = Layer.Foreground, Size = new Vector2(0.1f, 0.1f) });
    }

    private void Setup()
    {
        AddMockEntities();

        World.System("Clear sprite batch")
            .Kind(Ecs.PreUpdate)
            .Iter((Iter it) => renderSystem.Clear());

        World.System<Transform, Sprite>("Batch sprites")
            .Each((Iter it, int i, ref Transform t, ref Sprite s) =>
            {
                renderSystem.Add(t, s);
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
            .Iter(() =>
            {
                renderSystem.Draw();
            });

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
