namespace SpaceGame.Core.Systems;

public class Renderer
{
    private nint _window;
    private nint _gpuDevice;

    public Renderer(nint window, nint gpuDevice)
    {
        _window = window;
        _gpuDevice = gpuDevice;
    }

    public void Draw()
    {

    }
}
