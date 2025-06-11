using SDL3;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public class VertexShader(nint gpuDevice, nint handle) : IDisposable
{
    public nint Handle { get; private set; } = handle;

    private void ReleaseUnmanagedResources()
    {
        ReleaseGPUShader(gpuDevice, Handle);
        Handle = IntPtr.Zero;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~VertexShader()
    {
        ReleaseUnmanagedResources();
    }
}