using SpaceGame.Infrastructure;
using SpaceGame.SDLWrapper;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class Frame(CommandBufferWithSwapchain commandBuffer, Renderer renderer) : IFrame
{
    public unsafe void Draw(Rectangle rectangle)
    {
        var vertices = rectangle.ToVertices();
        var size = vertices.Capacity * sizeof(float) * 2 * sizeof(float) * 4;
        var uploadBuffer = renderer.RectanglePipeline.UploadBuffer;
        uploadBuffer.TryResize(size);
        var vertexBuffer = renderer.RectanglePipeline.VertexBuffer;
        vertexBuffer.TryResize(size);

        commandBuffer.WithCopyPass((cmd, pass) =>
        {
            var mappedBufferPtr = MapGPUTransferBuffer(renderer.GpuDevice.Handle, uploadBuffer.Handle, true);
            if (mappedBufferPtr == nint.Zero)
            {
                return;
            }
            var mappedBuffer = (ColouredVertex*)mappedBufferPtr;

            for (var i = 0; i < vertices.Count; i++)
            {
                mappedBuffer[i].Position = vertices[i].Position;
                mappedBuffer[i].Colour = vertices[i].Colour;
            }

            UnmapGPUTransferBuffer(renderer.GpuDevice.Handle, uploadBuffer.Handle);

            GPUTransferBufferLocation source = new()
            {
                TransferBuffer = uploadBuffer.Handle,
                Offset = 0,
            };
            GPUBufferRegion destination = new()
            {
                Buffer = vertexBuffer.Handle,
                Offset = 0,
                Size = (uint)size
            };
            UploadToGPUBuffer(pass, source, destination, false);
        })
        .WithRenderPass((cmd, pass) =>
        {
            var bufferBinding = new[] {
                            new GPUBufferBinding
                            {
                                Buffer = vertexBuffer.Handle,
                                Offset = 0
                            }
                        };

            BindGPUGraphicsPipeline(pass, renderer.RectanglePipeline.Pipeline.Handle);
            BindGPUVertexBuffers(pass, 0, bufferBinding, (uint)bufferBinding.Length);
            DrawGPUPrimitives(pass, (uint)size, 1, 0, 0);
        });
    }

    public void End()
    {
        commandBuffer.Submit();
    }
}
