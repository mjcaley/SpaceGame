namespace SpaceGame.Infrastructure;

public interface IVertexBuffer : IDisposable
{
    public nint Handle { get; }
    public int Size { get; }
}
