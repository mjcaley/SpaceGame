using SpaceGame.Infrastructure;
using System.Runtime.CompilerServices;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper
{
    public class VertexBuffer : IDisposable
    {
        public VertexBuffer(IGpuDevice gpuDevice, nint handle, int size)
        {
            _gpuDevice = gpuDevice;
            Handle = handle;
            Size = size;
        }

        public VertexBuffer(IGpuDevice gpuDevice, int size)
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

        private readonly IGpuDevice _gpuDevice;
        public nint Handle { get; private set; }
        public int Size { get; private set; }

        public bool TryResize(int size)
        {
            if (Size >= size)
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

        public bool TryResizeAndKeep(int size)
        {
            if (Size >= size)
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

            var commandBufferHandle = AcquireGPUCommandBuffer(_gpuDevice.Handle);
            if (commandBufferHandle == nint.Zero)
            {
                ReleaseGPUBuffer(_gpuDevice.Handle, newHandle);
                return false;
            }
            var commandBuffer = new CommandBuffer(commandBufferHandle);
            commandBuffer.WithCopyPass((cmd, pass) =>
            {
                var src = new GPUBufferLocation
                {
                    Buffer = Handle
                };
                var dest = new GPUBufferLocation
                {
                    Buffer = newHandle
                };

                CopyGPUBufferToBuffer(pass.Handle, src, dest, (uint)Size, true);
            })
            .Submit();

            ReleaseGPUBuffer(_gpuDevice.Handle, Handle);

            Handle = newHandle;
            Size = size;

            return true;
        }

        public void Upload<T>(IEnumerable<T> data)
        {
            var size = data.Count() * Unsafe.SizeOf<T>();

            if (!TryResize(size))
            {
                throw new InvalidOperationException("Unable to resize vertex buffer for uploading data.");
            }

            var commandBufferHandle = AcquireGPUCommandBuffer(_gpuDevice.Handle);
            if (commandBufferHandle == nint.Zero)
            {
                throw new InvalidOperationException("Failed to acquire command buffer for uploading vertex data.");
            }
            var transferBuffer = CreateGPUTransferBuffer(_gpuDevice.Handle, new()
            {
                Usage = GPUTransferBufferUsage.Upload,
                Size = (uint)size
            });

            var commandBuffer = new CommandBuffer(commandBufferHandle);
            commandBuffer.WithCopyPass((cmd, pass) =>
            {
                var src = new GPUBufferLocation
                {
                    Buffer = Handle
                };
                var dest = new GPUBufferLocation
                {
                    Buffer = transferBuffer
                };
                CopyGPUBufferToBuffer(pass.Handle, src, dest, (uint)size, false);
            })
            .Submit();

            ReleaseGPUTransferBuffer(_gpuDevice.Handle, transferBuffer);
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
