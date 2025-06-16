using SpaceGame.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public class CommandBufferWithSwapchain(nint commandBufferHandle, nint swapchainTexture)
{
    public nint CommandBufferHandle { get; private set; } = commandBufferHandle;

    public nint SwapchainTexture { get; private set; } = swapchainTexture;

    public GPUColorTargetInfo[] ColorTargetInfo { get; set; } = [];

    public void Submit()
    {
        SubmitGPUCommandBuffer(CommandBufferHandle);
        CommandBufferHandle = nint.Zero;
    }

    public CommandBufferWithSwapchain WithCopyPass(Action<CommandBufferWithSwapchain, CopyPass> func)
    {
        var copyPassHandle = BeginGPUCopyPass(CommandBufferHandle);
        if (copyPassHandle == nint.Zero)
        {
            return this;
        }
        var copyPass = new CopyPass(copyPassHandle);

        func(this, copyPass);

        EndGPUCopyPass(copyPass.Handle);

        return this;
    }

    public CommandBufferWithSwapchain Update(Action<CommandBufferWithSwapchain> func)
    {
        func(this);

        return this;
    }
    public CommandBufferWithSwapchain WithRenderPass(Action<CommandBufferWithSwapchain, RenderPass> func)
    {
        var renderPassHandle = BeginGPURenderPass(
            CommandBufferHandle,
            StructureArrayToPointer(ColorTargetInfo),
            (uint)ColorTargetInfo.Length,
            nint.Zero);
        if (renderPassHandle == nint.Zero)
        {
            return this;
        }
        var renderPass = new RenderPass(renderPassHandle);

        func(this, renderPass);

        EndGPURenderPass(renderPass.Handle);

        return this;
    }
}
