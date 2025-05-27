using static SDL3.SDL;

namespace SpaceGame.Infrastructure
{
    public interface ICommandBufferWithSwapchain
    {
        nint SwapchainTexture { get; }
        GPUColorTargetInfo[] ColorTargetInfo { get; set; }

        ICommandBufferWithSwapchain Update(Action<ICommandBufferWithSwapchain> func);
        ICommandBufferWithSwapchain WithCopyPass(Action<nint, nint> func);
        ICommandBufferWithSwapchain WithRenderPass(Action<nint, nint> func);
        void Submit();
    }
}
