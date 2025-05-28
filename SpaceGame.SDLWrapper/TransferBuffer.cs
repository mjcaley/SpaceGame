using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public class TransferBuffer(GpuDevice gpuDevice, nint handle, int size) : IDisposable
{
    public nint Handle { get; private set; } = handle;
    public int Size { get; private set; } = size;

    public bool TryResize(int size, GPUTransferBufferUsage usage)
    {
        if (Size == size)
        {
            return true;
        }
        
        var newHandle = CreateGPUTransferBuffer(
            gpuDevice.Handle,
            new()
            {
                Size = (uint)size,
                Usage = usage
            }
        );

        if (newHandle == nint.Zero)
        {
            return false;
        }
        
        ReleaseGPUTransferBuffer(gpuDevice.Handle, Handle);

        Handle = newHandle;
        Size = size;

        return true;
    }

    public nint Map()
    {
        var pointer = MapGPUTransferBuffer(gpuDevice.Handle, Handle, false);
        
        return pointer;
    }
    
    private void ReleaseUnmanagedResources()
    {
        ReleaseGPUTransferBuffer(gpuDevice.Handle, Handle);
        Handle = nint.Zero;
        Size = 0;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~TransferBuffer()
    {
        ReleaseUnmanagedResources();
    }
}