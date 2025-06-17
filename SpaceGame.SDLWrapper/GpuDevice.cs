using static SDL3.SDL;
using SpaceGame.Infrastructure;

namespace SpaceGame.SDLWrapper;

public class GpuDevice : IGpuDevice
{
    public GpuDevice(IWindow window)
    {
        _window = window;
        Handle = CreateGPUDevice(GPUShaderFormat.SPIRV, true, null);
        if (Handle == nint.Zero)
        {
            throw new GpuDeviceException("Failed to create GPU device");
        }

        var claimed = ClaimWindowForGPUDevice(Handle, _window.Handle);
        if (claimed)
        {
            SetGPUSwapchainParameters(Handle, _window.Handle, GPUSwapchainComposition.SDR, GPUPresentMode.Mailbox);
            return;
        }
        
        DestroyGPUDevice(Handle);
        Handle = nint.Zero;
        throw new GpuDeviceException("Failed to claim window");
    }

    private readonly IWindow _window;
    public nint Handle { get; private set; }
    
    private void ReleaseUnmanagedResources()
    {
        ReleaseWindowFromGPUDevice(Handle, _window.Handle);
        DestroyGPUDevice(Handle);
        Handle = nint.Zero;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~GpuDevice()
    {
        ReleaseUnmanagedResources();
    }
}
