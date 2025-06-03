namespace SpaceGame.Infrastructure;

public interface ITransferBuffer : IDisposable
{
    public nint Handle { get; }
    public int Size { get; }
    public bool TryResize(int size);
}
