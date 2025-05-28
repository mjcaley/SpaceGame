using System.Collections;
using System.Runtime.CompilerServices;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public class MappedBuffer<T>(GpuDevice gpuDevice, nint handle) : IDisposable
{
    public nint Handle { get; private set; } = handle;
    
    private void ReleaseUnmanagedResources()
    {
        UnmapGPUTransferBuffer(gpuDevice.Handle, Handle);
        Handle = nint.Zero;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~MappedBuffer()
    {
        ReleaseUnmanagedResources();
    }
}