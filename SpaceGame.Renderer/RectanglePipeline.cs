using SpaceGame.SDLWrapper;

namespace SpaceGame.Renderer;

public class RectanglePipeline(GraphicsPipeline pipeline, TransferBuffer uploadBuffer, VertexBuffer vertexBuffer) : IDisposable
{
    public GraphicsPipeline Pipeline => pipeline;
    public TransferBuffer UploadBuffer => uploadBuffer;
    public VertexBuffer VertexBuffer => vertexBuffer;

    public void Dispose()
    {
        pipeline.Dispose();
        uploadBuffer.Dispose();
        vertexBuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
