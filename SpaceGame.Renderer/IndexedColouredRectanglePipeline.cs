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
                            Pitch = SizeOf<ColouredVertex>(),
                            InputRate = GPUVertexInputRate.Vertex,
                            InstanceStepRate = 0
                        },
                        new() {
                            Slot = 1,
                            Pitch = SizeOf<Matrix4x4>(),
                            InputRate = GPUVertexInputRate.Instance,
                            InstanceStepRate = 0
                        }
                        ],
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
                        // matrix 4x4
                        new()
                        {
                            Location = 2,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = 0
                        },
                        new()
                        {
                            Location = 3,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = sizeof(float) * 4
                        },
                        new()
                        {
                            Location = 4,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = sizeof(float) * 8
                        },
                        new()
                        {
                            Location = 5,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = sizeof(float) * 12
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
        if (pipelineHandle == nint.Zero)
        {
            var error = GetError();
            throw new InvalidOperationException($"Failed to create graphics pipeline for indexed coloured rectangle. {error}");
        }
        Pipeline = new GraphicsPipeline(gpuDevice, pipelineHandle);
    }

    public GraphicsPipeline Pipeline { get; }

    public unsafe void Draw(CommandBufferWithSwapchain cmd, RenderPass pass, VertexBuffer vertices, IndexBuffer indices, VertexBuffer models, ref Camera camera, int numInstances)
    {
        PushGPUDebugGroup(cmd.CommandBufferHandle, "IndexedColouredRectanglePipeline.Draw()");
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
        fixed (Camera* cameraPtr = &camera)
        {
            PushGPUVertexUniformData(cmd.CommandBufferHandle, 0, (nint)cameraPtr, (uint)sizeof(Camera));
            BindGPUVertexBuffers(pass.Handle, 0, vertexBindings, (uint)vertexBindings.Length);
            BindGPUIndexBuffer(pass.Handle, indexBinding, GPUIndexElementSize.IndexElementSize16Bit);
            DrawGPUIndexedPrimitives(pass.Handle, (uint)numInstances * 6, (uint)numInstances, 0, 0, 0);
        }
        PopGPUDebugGroup(cmd.CommandBufferHandle);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Pipeline.Dispose();
    }
}
