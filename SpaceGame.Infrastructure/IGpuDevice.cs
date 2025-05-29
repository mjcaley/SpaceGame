namespace SpaceGame.Infrastructure;

public interface IGpuDevice : IDisposable
{
    public nint Handle { get; }
}
