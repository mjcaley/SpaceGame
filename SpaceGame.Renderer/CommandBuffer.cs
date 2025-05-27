using SpaceGame.Infrastructure;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class CommandBuffer(IRenderer renderer, nint CommandBufferHandle, nint windowHandle) : ICommandBuffer
{
    public IRenderer Renderer { get; init; } = renderer;
    public nint CommandBufferHandle { get; private set; }

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

    public ICommandBuffer AcquireSwapchainTexture()
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
        
        SwapchainTexture = swapchainTexture;
        // TODO: Save width and height

        return this;
    }

    public ICommandBuffer WithCopyPass(Action<nint, nint> func)
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
    
    public ICommandBuffer WithRenderPass(GPUColorTargetInfo colorTargetInfo, Action<nint, nint> func)
    {
        var copyPass = BeginGPURenderPass(
            CommandBufferHandle,
            StructureToPointer<GPUColorTargetInfo>(colorTargetInfo),
            1,
            nint.Zero);
        if (copyPass == nint.Zero)
        {
            return this;
        }
        
        func(CommandBufferHandle, copyPass);
        
        EndGPUCopyPass(copyPass);
        
        return this;
    }

    public ICommandBuffer WithRenderPass(GPUColorTargetInfo[] colorTargetInfo, Action<nint, nint> func)
    {
        var copyPass = BeginGPURenderPass(
            CommandBufferHandle,
            StructureArrayToPointer<GPUColorTargetInfo>(colorTargetInfo),
            (uint)colorTargetInfo.Length,
            nint.Zero);
        if (copyPass == nint.Zero)
        {
            return this;
        }
        
        func(CommandBufferHandle, copyPass);
        
        EndGPUCopyPass(copyPass);
        
        return this;
    }
}
