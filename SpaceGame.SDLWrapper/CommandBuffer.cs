using SpaceGame.Infrastructure;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public class CommandBuffer(nint commandBufferHandle)
{
    public nint CommandBufferHandle { get; private set; } = commandBufferHandle;

    public nint? SwapchainTexture { get; private set; }
    
    public void Cancel()
    {
        CancelGPUCommandBuffer(CommandBufferHandle);
        CommandBufferHandle = nint.Zero;
    }
    
    public void Submit()
    {
        SubmitGPUCommandBuffer(CommandBufferHandle);
        CommandBufferHandle = nint.Zero;
    }

    public CommandBufferWithSwapchain AcquireSwapchainTexture(nint windowHandle)
    {
        var swapchainResult = WaitAndAcquireGPUSwapchainTexture(
            CommandBufferHandle,
            windowHandle,
            out var swapchainTexture,
            out var swapchainWidth,
            out var swapchainHeight);

        if (!swapchainResult)
        {
            throw new Exception(); // TODO: Implement a nicer exception 
        }
        
        // TODO: Save width and height

        return new CommandBufferWithSwapchain(CommandBufferHandle, swapchainTexture);
    }

    public CommandBuffer WithCopyPass(Action<CommandBuffer, CopyPass> func)
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
    
    public CommandBuffer WithRenderPass(GPUColorTargetInfo colorTargetInfo, Action<CommandBuffer, RenderPass> func)
    {
        var renderPassHandle = BeginGPURenderPass(
            CommandBufferHandle,
            StructureToPointer<GPUColorTargetInfo>(colorTargetInfo),
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

    public CommandBuffer WithRenderPass(GPUColorTargetInfo[] colorTargetInfo, Action<CommandBuffer, RenderPass> func)
    {
        var renderPassHandle = BeginGPURenderPass(
            CommandBufferHandle,
            StructureArrayToPointer(colorTargetInfo),
            (uint)colorTargetInfo.Length,
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
