using SpaceGame.Infrastructure;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class CommandBuffer(IRenderer renderer, nint CommandBufferHandle)
{
    public IRenderer Renderer { get; init; } = renderer;
    public nint CommandBufferHandle { get; private set; }

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

    public CommandBuffer WithRenderPass(GPUColorTargetInfo[] colorTargetInfo, Action<nint, nint> func)
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
