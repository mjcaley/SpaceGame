namespace SpaceGame.Infrastructure;

public interface IWindow : IDisposable
{
    public nint Handle { get; }
}
