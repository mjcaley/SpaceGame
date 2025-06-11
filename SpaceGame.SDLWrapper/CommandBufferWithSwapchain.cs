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

    public CommandBufferWithSwapchain WithCopyPass(Action<nint, nint> func)
    {
        var copyPass = BeginGPUCopyPass(CommandBufferHandle);
        if (copyPass == nint.Zero)
        {
            return this;
        }

        func(CommandBufferHandle, copyPass);

        EndGPUCopyPass(copyPass);

        return this;
    }

    public CommandBufferWithSwapchain Update(Action<CommandBufferWithSwapchain> func)
    {
        func(this);

        return this;
    }
    public CommandBufferWithSwapchain WithRenderPass(Action<nint, nint> func)
    {
        var renderPass = BeginGPURenderPass(
            CommandBufferHandle,
            StructureArrayToPointer(ColorTargetInfo),
            (uint)ColorTargetInfo.Length,
            nint.Zero);
        if (renderPass == nint.Zero)
        {
            return this;
        }

        func(CommandBufferHandle, renderPass);

        EndGPURenderPass(renderPass);

        return this;
    }
}
