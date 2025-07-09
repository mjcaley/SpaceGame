using SpaceGame.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public class CommandBufferWithSwapchain(nint commandBufferHandle, SwapchainTexture swapchainTexture)
{
    public nint CommandBufferHandle { get; private set; } = commandBufferHandle;

    public SwapchainTexture SwapchainTexture => swapchainTexture;

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

    public CommandBufferWithSwapchain WithRenderPass(GPULoadOp loadOp, GPUStoreOp storeOp, Action<CommandBufferWithSwapchain, RenderPass> func)
    {
        var colourTargetInfo = new GPUColorTargetInfo
        {
            Texture = SwapchainTexture.Handle,
            ClearColor = new FColor { R = 0.5f, G = 0, B = 0.5f, A = 1.0f },
            LoadOp = loadOp,
            StoreOp = storeOp
        };

        var renderPassHandle = BeginGPURenderPass(
            CommandBufferHandle,
            StructureToPointer<GPUColorTargetInfo>(colourTargetInfo),
            1,
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
