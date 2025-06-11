using SpaceGame.Infrastructure;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper
{
    public class GraphicsPipeline(IGpuDevice gpuDevice, nint handle) : IDisposable
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