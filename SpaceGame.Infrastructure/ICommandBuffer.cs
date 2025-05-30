using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public interface ICommandBuffer
{
    ICommandBufferWithSwapchain AcquireSwapchainTexture();
    ICommandBuffer WithCopyPass(Action<nint, nint> func);
    ICommandBuffer WithRenderPass(GPUColorTargetInfo[] colorTargetInfo, Action<nint, nint> func);
    ICommandBuffer WithRenderPass(GPUColorTargetInfo colorTargetInfo, Action<nint, nint> func);
    void Submit();
    void Cancel();
}
