using SpaceGame.Infrastructure;
using SpaceGame.SDLWrapper;
using System.Numerics;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class Frame(CommandBufferWithSwapchain commandBuffer, Renderer renderer) : IFrame
{
    private readonly List<ColouredVertex> _vertices = [];
    private readonly List<(Vector3, Rectangle)> _rectVertices = [];
    private readonly List<ColouredVertexModel> _colouredVertices = [];

    private readonly List<BorrowedBuffer<VertexBuffer>> _borrowedVertexBuffers = [];
    private readonly List<BorrowedBuffer<IndexBuffer>> _borrowedIndexBuffers = [];

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

    public void Draw(Rectangle rectangle, Transformation transformation)
    {
        Console.WriteLine($"Rectangle draw colour is {rectangle.Colour}");
        var vertices = rectangle.ToVertices();
        var model = Matrix4x4.CreateTranslation(new Vector3(transformation.Translate.X, transformation.Translate.Y, 0f));
        _colouredVertices.Add(new() {
            V1 = vertices[0],
            V2 = vertices[1],
            V3 = vertices[2],
            V4 = vertices[3],
            V5 = vertices[4],
            V6 = vertices[5],
            Model = model
        });
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
        _borrowedVertexBuffers.Add(vertexBuffer);
        var matrixBuffer = renderer.BorrowVertexBuffer(sizeof(Matrix4x4));
        _borrowedVertexBuffers.Add(matrixBuffer);
        var indexBuffer = renderer.BorrowIndexBuffer(sizeof(short) * 6);
        _borrowedIndexBuffers.Add(indexBuffer);

        commandBuffer.WithCopyPass((cmd, pass) =>
        {
            Upload(
                pass,
                [
                    new ColouredVertex(new Vector2(0f, 0f), new Vector4(0f, 1f, 0f, 1f)),
                    new ColouredVertex(new Vector2(1f, 0f), new Vector4(0f, 1f, 0f, 1f)),
                    new ColouredVertex(new Vector2(0f, 1f), new Vector4(0f, 1f, 0f, 1f)),
                    new ColouredVertex(new Vector2(1f, 1f), new Vector4(0f, 1f, 0f, 1f)),
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
            var camera = new Camera(Matrix4x4.CreateOrthographic(1024f, 768f, .01f, 100f), Matrix4x4.CreateScale(16f));
            renderer.IndexedColouredRectanglePipeline.Draw(cmd, pass, vertexBuffer.Buffer, indexBuffer.Buffer, matrixBuffer.Buffer, ref camera, 1);
        });
    }

    private unsafe void Draw()
    {
        var instances = _colouredVertices.Count;
        if (instances == 0) return;

        var vertexBuffer = renderer.BorrowVertexBuffer(instances * sizeof(ColouredVertex) * 6);
        _borrowedVertexBuffers.Add(vertexBuffer);
        var modelBuffer = renderer.BorrowVertexBuffer(instances * sizeof(Matrix4x4));
        _borrowedVertexBuffers.Add(modelBuffer);

        commandBuffer.WithCopyPass((cmd, pass) =>
        {
            Upload(pass, _colouredVertices.Select(v => new List<ColouredVertex> { v.V1, v.V2, v.V3, v.V4, v.V5, v.V6 }).SelectMany(v => v).ToList(), vertexBuffer.Buffer);
            Upload(pass, _colouredVertices.Select(v => v.Model).ToList(), modelBuffer.Buffer);
        })
        .WithRenderPass((cmd, pass) =>
        {
            var camera = new Camera(Matrix4x4.CreateOrthographic(1024f, 768f, .01f, 100f), Matrix4x4.CreateScale(16f));
            renderer.ColouredRectanglePipeline.Draw(cmd, pass, vertexBuffer.Buffer, modelBuffer.Buffer, ref camera, instances);
        });
    }

    public void End()
    {
        Console.WriteLine("End frame");
        Draw();
        //DrawRectangle();
        commandBuffer.Submit();
        _borrowedIndexBuffers.ForEach(b => b.Return());
        _borrowedVertexBuffers.ForEach(b => b.Return());
    }
}
