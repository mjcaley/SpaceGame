using SpaceGame.Infrastructure;
using SpaceGame.Core.Components;
using Flecs.NET;
using Flecs.NET.Core;

namespace SpaceGame.Core;

public class Game(IRenderer renderer)
{
    public World World { get; private set; } = World.Create();
    private bool running = false;

    private void Setup()
    {
        World.System("Renderer")
            .Kind(Ecs.PostFrame)
            .Iter(() =>
            {
                Console.WriteLine("renderer");
                renderer.Draw();
            });

        World.System("Frame timer")
            .Kind(Ecs.OnUpdate)
            .Iter((Iter it) =>
            {
                Console.WriteLine("timer");
                Console.WriteLine($"Delta time is {it.DeltaTime()}");
            });

        World.SetTargetFps(60);
    }

    public void Run()
    {
        Console.WriteLine("Hello, Space Game!");
        Setup();
        running = true;

        while (World.Progress()) {}
    }
}
