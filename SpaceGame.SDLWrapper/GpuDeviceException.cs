namespace SpaceGame.SDLWrapper;

public class GpuDeviceException : Exception
{
    public GpuDeviceException() : base() { }

    public GpuDeviceException(string message) : base(message) { }
}
