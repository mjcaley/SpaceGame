using SpaceGame.Infrastructure;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper
{
    public class VertexBuffer : IVertexBuffer
    {
        public VertexBuffer(IGpuDevice gpuDevice, nint handle, int size)
        {
            _gpuDevice = gpuDevice;
            Handle = handle;
            Size = size;
        }

        public VertexBuffer(GpuDevice gpuDevice, int size)
        {
            _gpuDevice = gpuDevice;

            Handle = CreateGPUBuffer(
                gpuDevice.Handle,
                new()
                {
                    Size = (uint)size,
                    Usage = GPUBufferUsageFlags.Vertex
                }
            );

            Size = size;
        }

        private IGpuDevice _gpuDevice;
        public nint Handle { get; private set; }
        public int Size { get; private set; }

        public bool TryResize(int size, GPUTransferBufferUsage usage)
        {
            if (Size == size)
            {
                return true;
            }

            var newHandle = CreateGPUBuffer(
                _gpuDevice.Handle,
                new()
                {
                    Size = (uint)size,
                    Usage = GPUBufferUsageFlags.Vertex
                }
            );

            if (newHandle == nint.Zero)
            {
                return false;
            }

            ReleaseGPUBuffer(_gpuDevice.Handle, Handle);

            Handle = newHandle;
            Size = size;

            return true;
        }

        private void ReleaseUnmanagedResources()
        {
            ReleaseGPUBuffer(_gpuDevice.Handle, Handle);
            Handle = nint.Zero;
            Size = 0;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~VertexBuffer()
        {
            ReleaseUnmanagedResources();
        }
    }
}
