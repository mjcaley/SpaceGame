using SDL3;
using SpaceGame.Console;
using SpaceGame.Core;
using SpaceGame.Core.Components;
using SpaceGame.Core.Systems;


class Program
{
    private static void Exit(string message, int exitCode)
    {
        Console.WriteLine(message);
        Environment.Exit(exitCode);
    }

    public static void Main(string[] args)
    {
        using var sdl = SDLDependencies.Create();
        if (sdl == null)
        {
            Exit("SDL failed to initialize", -1);
        }

        var renderer = new Renderer(sdl!.Window, sdl!.GPUDevice);
        var game = new Game();
        game.World.System<Transform>("Renderer")
            .Each((ref Transform t) =>
            {
                renderer.Draw();
            });

        game.Run();
    }
}
