using SpaceGame.SDLWrapper;
using static SDL3.SDL;
using System.Numerics;
using SpaceGame.Infrastructure;
namespace SpaceGame.Renderer;

public class ColouredRectanglePipeline : IDisposable
{
    public ColouredRectanglePipeline(IWindow window, IGpuDevice gpuDevice, VertexShader vertexShader, FragmentShader fragmentShader)
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

    public unsafe void Draw(CommandBufferWithSwapchain cmd, RenderPass pass, VertexBuffer vertices, Matrix4x4 modelViewProjection, int numInstances)
    {
        var vertexBindings = new[]{
            new GPUBufferBinding
            {
                Buffer = vertices.Handle,
                Offset = 0
            }
        };

        BindGPUGraphicsPipeline(pass.Handle, Pipeline.Handle);
        BindGPUVertexBuffers(pass.Handle, 0, vertexBindings, 1);
        var viewProjectionPtr = (nint)(&modelViewProjection);
        PushGPUVertexUniformData(cmd.CommandBufferHandle, 0, viewProjectionPtr, (uint)sizeof(Matrix4x4));
        DrawGPUPrimitives(pass.Handle, (uint)numInstances * sizeof(float) * 6, (uint)numInstances, 0, 0);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Pipeline.Dispose();
    }
}
