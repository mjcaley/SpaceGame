using SpaceGame.Infrastructure;
using SpaceGame.SDLWrapper;
using System.CodeDom.Compiler;
using System.Numerics;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class Frame(CommandBufferWithSwapchain commandBuffer, Renderer renderer) : IFrame
{
    private readonly Dictionary<QuadVertices, List<QuadInstanceDetails>> _quads = [];

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

    public void Enqueue(Rectangle rectangle, Transformation transformation)
    {
        var vertices = rectangle.ToVertices();
        var key = new QuadVertices
        {
            V1 = vertices[0].Position,
            V2 = vertices[1].Position,
            V3 = vertices[2].Position,
            V4 = vertices[3].Position,
            V5 = vertices[4].Position,
            V6 = vertices[5].Position
        };
        var details = new QuadInstanceDetails
        {
            Colour = rectangle.Colour,
            Model = Matrix4x4.CreateTranslation(new Vector3(transformation.Translate.X, transformation.Translate.Y, 0f))
        };

        if (_quads.TryGetValue(key, out List<QuadInstanceDetails>? value))
        {
            value.Add(details);
        }
        else
        {
            _quads[key] = [details];
        }
    }

    private unsafe void DrawQuads()
    {
        if (_quads.Count == 0) return;

        // Pack data and get offsets
        var offsets = new List<InstanceOffsets>();
        var quadVertices = new List<QuadVertices>();
        var quadInstanceDetails = new List<QuadInstanceDetails>();

        foreach (var (index, (vertices, instance)) in _quads.Index())
        {
            quadVertices.Add(vertices);
            quadInstanceDetails.AddRange(instance);
            offsets.Add(new InstanceOffsets
            {
                VertexOffset = (uint)(index * sizeof(QuadVertices)),
                InstanceDetailsOffset = (uint)(index * sizeof(QuadInstanceDetails)),
                NumberOfInstances = instance.Count
            }
            );
        }
        var vertexBuffer = renderer.BorrowVertexBuffer(quadVertices.Count * sizeof(QuadVertices));
        _borrowedVertexBuffers.Add(vertexBuffer);
        var instanceBuffer = renderer.BorrowVertexBuffer(quadInstanceDetails.Count * sizeof(QuadInstanceDetails));
        _borrowedVertexBuffers.Add(instanceBuffer);

        var camera = new Camera(Matrix4x4.CreateOrthographic(1024f, 768f, .01f, 100f), Matrix4x4.CreateScale(16f));

        commandBuffer
            .WithCopyPass((cmd, pass) =>
            {
                Upload(pass, quadVertices, vertexBuffer.Buffer);
                Upload(pass, quadInstanceDetails, instanceBuffer.Buffer);
            })
            .WithRenderPass((cmd, pass) =>
            {
                foreach (var offset in offsets)
                {
                    renderer.ColouredRectanglePipeline.Draw(cmd, pass, vertexBuffer.Buffer, offset.VertexOffset, instanceBuffer.Buffer, offset.InstanceDetailsOffset, ref camera, offset.NumberOfInstances);
                }
            });
    }

    public void Draw()
    {
        DrawQuads();
    }

    public void End()
    {
        commandBuffer.Submit();
        _borrowedIndexBuffers.ForEach(b => b.Return());
        _borrowedVertexBuffers.ForEach(b => b.Return());
    }
}
