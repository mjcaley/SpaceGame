using Flecs.NET;
using Flecs.NET.Core;

namespace SpaceGame.Core;

public class Game
{
    public World World { get; private set; } = World.Create();
    private bool running;

    public void Run()
    {
        Console.WriteLine("Hello, Space Game!");
        running = true;

        while (running)
        {
            World.Progress();
        }
    }
}
