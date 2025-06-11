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

    public CommandBuffer WithCopyPass(Action<nint, nint> func)
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
    
    public CommandBuffer WithRenderPass(GPUColorTargetInfo colorTargetInfo, Action<nint, nint> func)
    {
        var renderPass = BeginGPURenderPass(
            CommandBufferHandle,
            StructureToPointer<GPUColorTargetInfo>(colorTargetInfo),
            1,
            nint.Zero);
        if (renderPass == nint.Zero)
        {
            return this;
        }
        
        func(CommandBufferHandle, renderPass);
        
        EndGPURenderPass(renderPass);
        
        return this;
    }

    public CommandBuffer WithRenderPass(GPUColorTargetInfo[] colorTargetInfo, Action<nint, nint> func)
    {
        var renderPass = BeginGPURenderPass(
            CommandBufferHandle,
            StructureArrayToPointer(colorTargetInfo),
            (uint)colorTargetInfo.Length,
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
