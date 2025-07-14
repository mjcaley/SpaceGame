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
    private unsafe void Upload<T>(CopyPass pass, List<T> data, IndexBuffer indexBuffer) where T : unmanaged
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
            Buffer = indexBuffer.Handle,
            Offset = 0,
            Size = (uint)size
        };
        UploadToGPUBuffer(pass.Handle, source, destination, false);
    }

    public unsafe void Draw(Rectangle rectangle)
    {
        _vertices.AddRange(rectangle.ToVertices());
    }

    public unsafe void Draw(Vector3 position, Rectangle rectangle)
    {
        // Need buffers
        // 1 Vertex buffer for 6 vertices
        // 1 Index buffer the size of all vertices
        // 1 Vertex buffer to store matrix 4x4 for model transformation
        // 1 uniform matrix4x4 for view * projection matrix
        // OR 2 uniform matrix4x4 for view and projection matrices
        _rectVertices.Add((position, rectangle));
    }

    private unsafe void DrawRectangle()
    {
        var vertexBuffer = renderer.BorrowVertexBuffer(sizeof(ColouredVertex) * 4);
        var matrixBuffer = renderer.BorrowVertexBuffer(sizeof(Matrix4x4));
        var indexBuffer = renderer.BorrowIndexBuffer(sizeof(short) * 6);

        commandBuffer.WithCopyPass((cmd, pass) =>
        {
            Upload(
                pass,
                [
                    new ColouredVertex(new Vector2(0f, 0f), new Vector4(0f, 0f, 1f, 1f)),
                    new ColouredVertex(new Vector2(1f, 0f), new Vector4(0f, 0f, 1f, 1f)),
                    new ColouredVertex(new Vector2(0f, 1f), new Vector4(0f, 0f, 1f, 1f)),
                    new ColouredVertex(new Vector2(1f, 1f), new Vector4(0f, 0f, 1f, 1f)),
                ],
                vertexBuffer.Buffer
            );

            Upload(
                pass,
                [
                    Matrix4x4.Identity,
                ],
                matrixBuffer.Buffer
            );

            Upload(
                pass,
                [
                    (short)0,
                    (short)2,
                    (short)3,
                    (short)0,
                    (short)3,
                    (short)1,
                ],
                indexBuffer.Buffer
            );
        })
        .WithRenderPass(GPULoadOp.DontCare, GPUStoreOp.Store, (cmd, pass) =>
        {
            var camera = new Camera(Matrix4x4.Identity, Matrix4x4.Identity);
            renderer.IndexedColouredRectanglePipeline.Draw(cmd, pass, vertexBuffer.Buffer, indexBuffer.Buffer, matrixBuffer.Buffer, ref camera, 1);
        });

        vertexBuffer.Return();
        indexBuffer.Return();
        matrixBuffer.Return();
    }

    private unsafe void Draw()
    {
        var size = _vertices.Capacity * sizeof(ColouredVertex);
        if (size == 0) return;

        var vertexBuffer = renderer.BorrowVertexBuffer(size);

        commandBuffer.WithCopyPass((cmd, pass) =>
        {
            Upload(pass, _vertices, vertexBuffer.Buffer);
        })
        .WithRenderPass((cmd, pass) =>
        {
            var translate = Matrix4x4.CreateOrthographic(
                cmd.SwapchainTexture.Width,
                cmd.SwapchainTexture.Height,
                -1,
                1);
            renderer.ColouredRectanglePipeline.Draw(cmd, pass, vertexBuffer.Buffer, translate, _vertices.Count / 6);
        });

        vertexBuffer.Return();
    }

    public void End()
    {
        Draw();
        DrawRectangle();
        commandBuffer.Submit();
    }
}
