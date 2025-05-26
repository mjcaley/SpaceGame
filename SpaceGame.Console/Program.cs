using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDL3;
using Silk.NET.Maths;
using SpaceGame.Core;
using SpaceGame.Core.Components;
using SpaceGame.Infrastructure;
using SpaceGame.Renderer;
using SpaceGame.SDLWrapper;


class Program
{
    private static void Exit(string message, int exitCode)
    {
        Console.WriteLine(message);
        Environment.Exit(exitCode);
    }

    public static void Main(string[] args)
    {
        var host =
            Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services
                .AddSingleton<IWindow>(new Window("Space Game", 1024, 768))
                .AddSingleton<IGpuDevice, GpuDevice>()
                .AddSingleton<IRenderer, Renderer>()
                .AddScoped<Game>()
        ).Build();

        var renderer = host.Services.GetRequiredService<IRenderer>() as Renderer;
        renderer.Add(new Sprite()
        {
            Origin=new Vector2D<double>(0.1f, 0.1f),
            Size=new Vector2D<double>(0.5f, 0.5f),
            Colour= new Vector4D<double>(0, 0, 1.0f, 1.0f),
        });
        
        var game = host.Services.GetRequiredService<Game>();
        game.Run();
    }
}
