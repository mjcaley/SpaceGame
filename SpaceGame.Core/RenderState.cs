using SpaceGame.Infrastructure;

namespace SpaceGame.Core;

public class RenderState
{
    public IGraphicsPipeline Pipeline { get; }
    public VertexBuffer VertexBuffer { get; }
}