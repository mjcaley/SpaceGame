namespace SpaceGame.Renderer;

public class CommandBuffer(Renderer renderer, nint commandBuffer)
{
    public Renderer Renderer { get; init; } = renderer;
    public nint CommandBufferHandle { get; init; } = commandBuffer;
}
