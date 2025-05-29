using System.Drawing;
using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public interface IRenderer : IDisposable
{
    void Draw();
    IGpuDevice GpuDevice { get; }
    ICommandBuffer AcquireCommandBuffer();
    IShader CreateShader(ref ShaderCreateInfo shaderCreateInfo);
    ITransferBuffer CreateTransferBuffer(int size, GPUTransferBufferUsage usage);
    IVertexBuffer CreateVertexBuffer(int size);
    IGraphicsPipeline CreatePipeline(ref GraphicsPipelineCreateInfo pipelineCreateInfo);
}
