using SpaceGame.SDLWrapper;
using static SDL3.SDL;
using System.Numerics;
using SpaceGame.Infrastructure;
namespace SpaceGame.Renderer;

public class ColouredRectanglePipeline : IDisposable
{
    private static unsafe uint SizeOf<T>() where T : unmanaged => (uint)sizeof(T);

    public ColouredRectanglePipeline(IWindow window, IGpuDevice gpuDevice, VertexShader vertexShader, FragmentShader fragmentShader)
    {
        var pipelineHandle = CreateGPUGraphicsPipeline(gpuDevice.Handle, new GraphicsPipelineCreateInfo()
        {
            VertexShader = vertexShader,
            FragmentShader = fragmentShader,
            VertexInputState = new()
            {
                VertexBufferDescriptions = [
                        // position
                        new() {
                            Slot = 0,
                            Pitch = SizeOf<Vector2>(),
                            InputRate = GPUVertexInputRate.Vertex,
                            InstanceStepRate = 0
                        },
                        // colour
                        new() {
                            Slot = 1,
                            Pitch = SizeOf<Vector4>() + SizeOf<Matrix4x4>(),
                            InputRate = GPUVertexInputRate.Instance,
                            InstanceStepRate = 0
                        }],
                VertexAttributes = [
                        // position
                        new()
                        {
                            Location = 0,
                            BufferSlot = 0,
                            Format = GPUVertexElementFormat.Float2,
                            Offset = 0
                        },
                        // colour
                        new()
                        {
                            Location = 1,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = 0
                        },
                        // model
                        new()
                        {
                            Location = 2,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = SizeOf<Vector4>()
                        },
                        new()
                        {
                            Location = 3,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = SizeOf<Vector4>() * 2
                        },
                        new()
                        {
                            Location = 4,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = SizeOf<Vector4>() * 3
                        },
                        new()
                        {
                            Location = 5,
                            BufferSlot = 1,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = SizeOf<Vector4>() * 4
                        }]
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

    public void Draw(CommandBufferWithSwapchain cmd, RenderPass pass, VertexBuffer vertices, VertexBuffer instanceDetails, ref Camera camera, int numInstances)
    {
        Draw(cmd, pass, vertices, 0, instanceDetails, 0, ref camera, numInstances);
    }

    public unsafe void Draw(CommandBufferWithSwapchain cmd, RenderPass pass, VertexBuffer vertices, uint vertexOffset, VertexBuffer instanceDetails, uint instanceOffset, ref Camera camera, int numInstances)
    {
        PushGPUDebugGroup(cmd.CommandBufferHandle, "ColouredRectanglePipeline.Draw");
        var vertexBindings = new[] {
            new GPUBufferBinding
            {
                Buffer = vertices.Handle,
                Offset = vertexOffset
            },
            new GPUBufferBinding
            {
                Buffer = instanceDetails.Handle,
                Offset = instanceOffset
            }
        };

        fixed (Camera* cameraPtr = &camera)
        {
            BindGPUGraphicsPipeline(pass.Handle, Pipeline.Handle);
            BindGPUVertexBuffers(pass.Handle, 0, vertexBindings, (uint)vertexBindings.Length);
            PushGPUVertexUniformData(cmd.CommandBufferHandle, 0, (nint)cameraPtr, (uint)sizeof(Camera));
            DrawGPUPrimitives(pass.Handle, 6, (uint)numInstances, 0, 0);
        }
        PopGPUDebugGroup(cmd.CommandBufferHandle);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Pipeline.Dispose();
    }
}
