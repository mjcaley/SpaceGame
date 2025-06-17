using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpaceGame.Core;
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

var game = host.Services.GetRequiredService<Game>();
game.Run();
