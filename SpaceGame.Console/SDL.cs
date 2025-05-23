using SDL3;

namespace SpaceGame.Console;

internal record SDLDependencies(nint Window, nint GPUDevice) : IDisposable
{
    private bool disposedValue;

    public nint Window { get; private set; } = Window;
    public nint GPUDevice { get; private set;  } = GPUDevice;

    public static SDLDependencies? Create()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events))
        {
            return null;
        }

        var window = SDL.CreateWindow("Space Game", 1024, 768, 0);
        if (window == IntPtr.Zero)
        {
            return null;
        }

        var gpuDevice = SDL.CreateGPUDevice(SDL.GPUShaderFormat.SPIRV, true, null);
        if (gpuDevice == IntPtr.Zero)
        {
            SDL.DestroyWindow(window);
            return null;
        }
        if (!SDL.ClaimWindowForGPUDevice(gpuDevice, window))
        {
            return null;
        }

        return new(window, gpuDevice);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            SDL.DestroyGPUDevice(GPUDevice);
            GPUDevice = IntPtr.Zero;
            SDL.DestroyWindow(Window);
            Window = IntPtr.Zero;
            SDL.Quit();
            disposedValue = true;
        }
    }

     ~SDLDependencies()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
