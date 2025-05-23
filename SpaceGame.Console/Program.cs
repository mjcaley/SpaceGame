using SDL3;
using Silk.NET.Maths;
using SpaceGame.Console;
using SpaceGame.Core;
using SpaceGame.Core.Components;
using SpaceGame.Infrastructure;
using SpaceGame.Renderer;


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

        using var renderer = new Renderer(sdl!.Window, sdl!.GPUDevice);
        renderer.Add(new Sprite()
        {
            Origin=new Vector2D<double>(0.1f, 0.1f),
            Size=new Vector2D<double>(0.5f, 0.5f),
            Colour= new Vector4D<double>(0, 0, 1.0f, 1.0f),
        });
        var game = new Game(renderer);
        game.Run();
    }
}
