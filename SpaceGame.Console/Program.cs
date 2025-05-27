using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDL3;
using Silk.NET.Maths;
using SpaceGame.Core;
using SpaceGame.Core.Components;
using SpaceGame.Infrastructure;
using SpaceGame.Renderer;
using SpaceGame.SDLWrapper;


var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    [$"SpaceGame:Renderer:Width"] = "1025",
    [$"SpaceGame:Renderer:Height"] = "768"
});

builder.Services.Configure<WindowSettings>(
    builder.Configuration.GetSection(WindowSettings.SectionName));

builder.Services
    .AddScoped<IWindow, Window>()
    .AddScoped<IGpuDevice, GpuDevice>()
    .AddScoped<IRenderer, Renderer>()
    .AddScoped<Game>()
;
    
var host = builder.Build();

var renderer = host.Services.GetRequiredService<IRenderer>() as Renderer;
renderer.Add(new Sprite()
{
    Origin=new Vector2D<double>(0.1f, 0.1f),
    Size=new Vector2D<double>(0.5f, 0.5f),
    Colour= new Vector4D<double>(0, 0, 1.0f, 1.0f),
});

var game = host.Services.GetRequiredService<Game>();
game.Run();
