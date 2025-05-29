using SpaceGame.Infrastructure;
using static SDL3.SDL;

namespace SpaceGame.Renderer
{
    public class GraphicsPipeline(IGpuDevice gpuDevice, nint handle) : IGraphicsPipeline
    {
        public nint Handle { get; private set; } = handle;

        private void ReleaseUnmanagedResources()
        {
            ReleaseGPUGraphicsPipeline(gpuDevice.Handle, Handle);
            Handle = nint.Zero;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~GraphicsPipeline()
        {
            ReleaseUnmanagedResources();
        }
    }
}