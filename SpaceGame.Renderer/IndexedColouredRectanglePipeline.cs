using SpaceGame.SDLWrapper;
using static SDL3.SDL;
using System.Numerics;
using SpaceGame.Infrastructure;
namespace SpaceGame.Renderer;

public class IndexedColouredRectanglePipeline : IDisposable
{
    private static unsafe uint SizeOf<T>() where T : unmanaged => (uint)sizeof(T);

    public IndexedColouredRectanglePipeline(IWindow window, IGpuDevice gpuDevice, VertexShader vertexShader, FragmentShader fragmentShader)
    {
        var pipelineHandle = CreateGPUGraphicsPipeline(gpuDevice.Handle, new GraphicsPipelineCreateInfo()
        {
            VertexShader = vertexShader,
            FragmentShader = fragmentShader,
            VertexInputState = new()
            {
                VertexBufferDescriptions = [
                        new() {
                            Slot = 0,
                            Pitch = sizeof(float) * 6,
                            InputRate = GPUVertexInputRate.Vertex,
                            InstanceStepRate = 0
                        },
                        new() {
                            Slot = 1,
                            Pitch = SizeOf<Matrix4x4>(),
                            InputRate = GPUVertexInputRate.Instance
                        }],
                VertexAttributes = [
                        new()
                        {
                            Location = 0,
                            BufferSlot = 0,
                            Format = GPUVertexElementFormat.Float2,
                            Offset = 0
                        },
                        new()
                        {
                            Location = 1,
                            BufferSlot = 0,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = sizeof(float) * 2
                        },
                        new()
                        {
                            Location = 2,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = 0
                        }
                    ]
            },
            PrimitiveType = GPUPrimitiveType.TriangleList,
            TargetInfo = new()
            {
                ColorTargetDescriptions = [
                        new()
                        {
                            Format=GetGPUSwapchainTextureFormat(gpuDevice.Handle, window.Handle),
                        }
                    ]
            }
        });
        Pipeline = new GraphicsPipeline(gpuDevice, pipelineHandle);
    }

    public GraphicsPipeline Pipeline { get; }

    public unsafe void Draw(CommandBufferWithSwapchain cmd, RenderPass pass, VertexBuffer vertices, IndexBuffer indices, VertexBuffer models, Matrix4x4 viewProjection, int numInstances)
    {
        var vertexBindings = new[] {
                new GPUBufferBinding
                {
                    Buffer = vertices.Handle,
                    Offset = 0
                },
                new GPUBufferBinding
                {
                    Buffer = models.Handle,
                    Offset = 0
                }
        };

        var indexBinding = new GPUBufferBinding
        {
            Buffer = indices.Handle,
            Offset = 0
        };

        BindGPUGraphicsPipeline(pass.Handle, Pipeline.Handle);
        BindGPUVertexBuffers(pass.Handle, 0, vertexBindings, (uint)vertexBindings.Length);
        BindGPUIndexBuffer(pass.Handle, indexBinding, GPUIndexElementSize.IndexElementSize16Bit);
        var viewProjectionPtr = (nint)(&viewProjection);
        PushGPUVertexUniformData(cmd.CommandBufferHandle, 0, viewProjectionPtr, (uint)sizeof(Matrix4x4));
        DrawGPUIndexedPrimitives(pass.Handle, (uint)numInstances * sizeof(short), (uint)numInstances, 0, 0, 0);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Pipeline.Dispose();
    }
}
