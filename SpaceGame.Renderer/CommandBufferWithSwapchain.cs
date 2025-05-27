using SpaceGame.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class CommandBufferWithSwapchain(IRenderer renderer, nint CommandBufferHandle, nint SwapchainTexture) : ICommandBufferWithSwapchain
{
    public IRenderer Renderer { get; init; } = renderer;
    public nint CommandBufferHandle { get; private set; } = CommandBufferHandle;

    public nint SwapchainTexture { get; private set; } = SwapchainTexture;

    public GPUColorTargetInfo[] ColorTargetInfo { get; set; } = [];

    public void Submit()
    {
        SubmitGPUCommandBuffer(CommandBufferHandle);
        CommandBufferHandle = nint.Zero;
    }

    public ICommandBufferWithSwapchain WithCopyPass(Action<nint, nint> func)
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

    public ICommandBufferWithSwapchain Update(Action<ICommandBufferWithSwapchain> func)
    {
        func(this);

        return this;
    }
    public ICommandBufferWithSwapchain WithRenderPass(Action<nint, nint> func)
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
