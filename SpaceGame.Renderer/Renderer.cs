using SDL3;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class Renderer
{
    public Renderer(nint window, nint gpuDevice)
    {
        _window = window;
        _gpuDevice = gpuDevice;
    }

    private nint _window;
    private nint _gpuDevice;


}
