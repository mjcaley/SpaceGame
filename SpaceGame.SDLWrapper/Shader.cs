using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static SDL3.SDL;
using SpaceGame.Infrastructure;

namespace SpaceGame.SDLWrapper;

public class Shader(IGpuDevice gpuDevice, nint handle) : IShader
{
    public nint Handle { get; private set; } = handle;

    private void ReleaseUnmanagedResources()
    {
        ReleaseGPUShader(gpuDevice.Handle, Handle);
        Handle = nint.Zero;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        ReleaseUnmanagedResources();
    }
}
