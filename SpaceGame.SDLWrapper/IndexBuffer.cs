﻿using SpaceGame.Infrastructure;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public class IndexBuffer : IDisposable
{
    public IndexBuffer(IGpuDevice gpuDevice, nint handle, int size)
    {
        _gpuDevice = gpuDevice;
        Handle = handle;
        Size = size;
    }

    public IndexBuffer(IGpuDevice gpuDevice, int size)
    {
        _gpuDevice = gpuDevice;

        Handle = CreateGPUBuffer(
            gpuDevice.Handle,
            new()
            {
                Size = (uint)size,
                Usage = GPUBufferUsageFlags.Index
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
                Usage = GPUBufferUsageFlags.Index
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

    ~IndexBuffer()
    {
        ReleaseUnmanagedResources();
    }
}
