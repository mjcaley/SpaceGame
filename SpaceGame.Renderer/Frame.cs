using SpaceGame.Infrastructure;
using SpaceGame.SDLWrapper;
using System.Numerics;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class Frame(CommandBufferWithSwapchain commandBuffer, Renderer renderer) : IFrame
{
    private readonly List<ColouredVertex> _vertices = [];
    private readonly List<(Vector3, Rectangle)> _rectVertices = [];

    private unsafe void Upload<T>(CopyPass pass, List<T> data, VertexBuffer vertexBuffer) where T : unmanaged
    {
        var size = sizeof(T) * data.Count;
        var uploadBuffer = renderer.BorrowUploadBuffer(size);
        var mappedBufferPtr = MapGPUTransferBuffer(renderer.GpuDevice.Handle, uploadBuffer.Buffer.Handle, true);
        if (mappedBufferPtr == nint.Zero)
        {
            return;
        }
        var mappedBuffer = (T*)mappedBufferPtr;

        for (var i = 0; i < data.Count; i++)
        {
            mappedBuffer[i] = data[i];
        }

        UnmapGPUTransferBuffer(renderer.GpuDevice.Handle, uploadBuffer.Buffer.Handle);
        uploadBuffer.Return();

        GPUTransferBufferLocation source = new()
        {
            TransferBuffer = uploadBuffer.Buffer.Handle,
            Offset = 0,
        };
        GPUBufferRegion destination = new()
        {
            Buffer = vertexBuffer.Handle,
            Offset = 0,
            Size = (uint)size
        };
        UploadToGPUBuffer(pass.Handle, source, destination, false);
    }
       
    public unsafe void Draw(Rectangle rectangle)
    {
        // Need buffers
        // 1 Vertex buffer for 6 vertices
        // 1 Index buffer the size of all vertices
        // 1 Vertex buffer to store matrix 4x4 for model transformation
        // 1 uniform matrix4x4 for view * projection matrix
        // OR 2 uniform matrix4x4 for view and projection matrices
        //_vertices.AddRange(rectangle.ToVertices());
    }

    public unsafe void Draw(Vector3 position, Rectangle rectangle)
    {
        _rectVertices.Add((position, rectangle));
    }

    private unsafe void DrawRectangle()
    {
        //if (_rectVertices.Count == 0) return;

        //var vertexSize = sizeof(float) * 6;
        //var indexSize = sizeof(short) * _rectVertices.Count;
        //var modelSize = sizeof(float) * 16 * _rectVertices.Count;

        //var uploadIndexBuffer = renderer.BorrowUploadBuffer(indexSize);
        //var uploadModelBuffer = renderer.BorrowUploadBuffer(modelSize);


        //commandBuffer.WithCopyPass((cmd, pass) =>
        //{
        //    Upload(pass, _vertices, )
        //})
        //.WithRenderPass((cmd, pass) =>
        //{
        //    var bufferBinding = new[] {
        //        new GPUBufferBinding
        //        {
        //            Buffer = vertexBuffer.Buffer.Handle,
        //            Offset = 0
        //        }
        //    };

        //    BindGPUGraphicsPipeline(pass.Handle, renderer.RectanglePipeline.Pipeline.Handle);
        //    BindGPUVertexBuffers(pass.Handle, 0, bufferBinding, (uint)bufferBinding.Length);
        //    var translate = Matrix4x4.CreateOrthographic(
        //        cmd.SwapchainTexture.Width,
        //        cmd.SwapchainTexture.Height,
        //        -1,
        //        1);
        //    var translatePtr = (nint)(&translate);
        //    PushGPUVertexUniformData(cmd.CommandBufferHandle, 0, translatePtr, (uint)sizeof(Matrix4x4));
        //    DrawGPUPrimitives(pass.Handle, (uint)_vertices.Count, (uint)_vertices.Count / 6, 0, 0);
        //});

        //uploadBuffer.Return();
        //vertexBuffer.Return();
    }

    private unsafe void Draw()
    {
        var size = _vertices.Capacity * sizeof(ColouredVertex);
        if (size == 0) return;

        var uploadBuffer = renderer.BorrowUploadBuffer(size);
        var vertexBuffer = renderer.BorrowVertexBuffer(size);

        commandBuffer.WithCopyPass((cmd, pass) =>
        {
            Upload(pass, _vertices, vertexBuffer.Buffer);
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
