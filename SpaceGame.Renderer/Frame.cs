using SpaceGame.Infrastructure;
using SpaceGame.SDLWrapper;
using System.Numerics;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class Frame(CommandBufferWithSwapchain commandBuffer, Renderer renderer) : IFrame
{
    private readonly List<ColouredVertex> _vertices = [];
    
    public unsafe void Draw(Rectangle rectangle)
    {
        _vertices.AddRange(rectangle.ToVertices());
    }

    private unsafe void Draw()
    {
        var size = (_vertices.Capacity * sizeof(ColouredVertex));
        if (size == 0) return;

        var uploadBuffer = renderer.BorrowUploadBuffer(size);
        var vertexBuffer = renderer.BorrowVertexBuffer(size);

        commandBuffer.WithCopyPass((cmd, pass) =>
        {
            var mappedBufferPtr = MapGPUTransferBuffer(renderer.GpuDevice.Handle, uploadBuffer.Buffer.Handle, true);
            if (mappedBufferPtr == nint.Zero)
            {
                return;
            }
            var mappedBuffer = (ColouredVertex*)mappedBufferPtr;

            for (var i = 0; i < _vertices.Count; i++)
            {
                mappedBuffer[i].Position = _vertices[i].Position;
                mappedBuffer[i].Colour = _vertices[i].Colour;
            }

            UnmapGPUTransferBuffer(renderer.GpuDevice.Handle, uploadBuffer.Buffer.Handle);

            GPUTransferBufferLocation source = new()
            {
                TransferBuffer = uploadBuffer.Buffer.Handle,
                Offset = 0,
            };
            GPUBufferRegion destination = new()
            {
                Buffer = vertexBuffer.Buffer.Handle,
                Offset = 0,
                Size = (uint)size
            };
            UploadToGPUBuffer(pass.Handle, source, destination, false);
        })
        .WithRenderPass((cmd, pass) =>
        {
            var bufferBinding = new[] {
                new GPUBufferBinding
                {
                    Buffer = vertexBuffer.Buffer.Handle,
                    Offset = 0
                }
            };

            BindGPUGraphicsPipeline(pass.Handle, renderer.RectanglePipeline.Pipeline.Handle);
            BindGPUVertexBuffers(pass.Handle, 0, bufferBinding, (uint)bufferBinding.Length);
            var translate = Matrix4x4.CreateOrthographic(
                cmd.SwapchainTexture.Width,
                cmd.SwapchainTexture.Height,
                -1,
                1);
            var translatePtr = (nint)(&translate);
            PushGPUVertexUniformData(cmd.CommandBufferHandle, 0, translatePtr, (uint)sizeof(Matrix4x4));
            DrawGPUPrimitives(pass.Handle, (uint)_vertices.Count, (uint)_vertices.Count / 6, 0, 0);
        });

        uploadBuffer.Return();
        vertexBuffer.Return();
    }

    public void End()
    {
        Draw();
        commandBuffer.Submit();
    }
}
