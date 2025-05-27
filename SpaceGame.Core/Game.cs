using SpaceGame.Infrastructure;
using SpaceGame.Core.Components;
using Flecs.NET;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using SDL3;
using static SDL3.SDL;

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

                renderer
                    .AcquireCommandBuffer()
                    .AcquireSwapchainTexture()
                    .Update(cmd =>
                    {
                        cmd.ColorTargetInfo = [
                            new ()
                            {
                                Texture = cmd.SwapchainTexture,
                                ClearColor = new FColor { R = 1.0f, G = 0, B = 0, A = 1.0f },
                                LoadOp = GPULoadOp.Clear,
                                StoreOp = GPUStoreOp.Store
                            }
                        ];
                    })
                    .WithRenderPass(
                        (cmd, pass) =>
                        {
                            
                        })
                    .Submit();
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

        while (World.Progress()) {
            if (World.ShouldQuit())
            {
                break;
            }
        }
    }
}
