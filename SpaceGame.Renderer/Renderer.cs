using SpaceGame.Infrastructure;
using SpaceGame.SDLWrapper;
using static SDL3.SDL;

namespace SpaceGame.Renderer;

public class Renderer : IRenderer
{
    public Renderer(IWindow window, IGpuDevice gpuDevice)
    {
        _window = window;
        _gpuDevice = gpuDevice;
        InitPipelines();
    }

    private IWindow _window;
    private IGpuDevice _gpuDevice;
    private List<TransferBuffer> _uploadBuffers = [];
    private List<VertexBuffer> _vertexBuffers = [];

    public IWindow Window => _window;
    public IGpuDevice GpuDevice => _gpuDevice;

    private RectanglePipeline _rectanglePipeline;
    public RectanglePipeline RectanglePipeline => _rectanglePipeline;

    public IFrame BeginFrame() => new Frame(
        AcquireCommandBuffer()
        .AcquireSwapchainTexture(_window.Handle)
        .Update(cmd =>
            cmd.ColorTargetInfo = [
                new()
                {
                    Texture = cmd.SwapchainTexture.Handle,
                    ClearColor = new FColor { R = 0.5f, G = 0, B = 0.5f, A = 1.0f },
                    LoadOp = GPULoadOp.Clear,
                    StoreOp = GPUStoreOp.Store
                }
            ]
        ), this);

    private bool disposedValue;

    private void InitPipelines()
    {
        using var rectVertexShader = new VertexShader(
            GpuDevice.Handle,
            CreateGPUShader(GpuDevice.Handle, new ShaderCreateInfo()
            {
                Code = Assets.Shaders.QuadVertex.Spirv,
                Entrypoint = "main",
                Format = GPUShaderFormat.SPIRV,
                Stage = GPUShaderStage.Vertex,
                NumUniformBuffers = 1
            }.ToSDL()
        ));
        using var rectFragmentShader = new FragmentShader(
            GpuDevice.Handle,
            CreateGPUShader(GpuDevice.Handle, new ShaderCreateInfo(
                Assets.Shaders.QuadFragment.Spirv,
                "main",
                GPUShaderFormat.SPIRV,
                GPUShaderStage.Fragment
            ).ToSDL()
        ));
        var pipelineHandle = CreateGPUGraphicsPipeline(GpuDevice.Handle, new GraphicsPipelineCreateInfo()
        {
            VertexShader = rectVertexShader,
            FragmentShader = rectFragmentShader,
            VertexInputState = new()
            {
                VertexBufferDescriptions = [
                        new GPUVertexBufferDescription() {
                            Slot = 0,
                            Pitch = sizeof(float) * 6,
                            InputRate = GPUVertexInputRate.Vertex,
                            InstanceStepRate = 0
                        }],
                VertexAttributes = [
                        new GPUVertexAttribute()
                        {
                            Location = 0,
                            BufferSlot = 0,
                            Format = GPUVertexElementFormat.Float2,
                            Offset = 0
                        },
                        new GPUVertexAttribute()
                        {
                            Location = 1,
                            BufferSlot = 0,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = sizeof(float) * 2
                        },
                    ]
            },
            PrimitiveType = GPUPrimitiveType.TriangleList,
            TargetInfo = new()
            {
                ColorTargetDescriptions = [
                        new()
                        {
                            Format=GetGPUSwapchainTextureFormat(GpuDevice.Handle, Window.Handle),
                        }
                    ]
            }
        }.ToSDL());

        _rectanglePipeline = new RectanglePipeline(
            new GraphicsPipeline(GpuDevice, pipelineHandle),
            CreateTransferBuffer(sizeof(float) * 6, GPUTransferBufferUsage.Upload),
            CreateVertexBuffer(sizeof(float) * 6 * 4)
        );
    }

    private CommandBuffer AcquireCommandBuffer()
    {
        var commandBuffer = AcquireGPUCommandBuffer(_gpuDevice.Handle);
        if (commandBuffer == nint.Zero)
        {
            throw new NullReferenceException("Command buffer is null pointer");
        }

        return new CommandBuffer(commandBuffer);
    }

    private TransferBuffer CreateTransferBuffer(int size, GPUTransferBufferUsage usage)
    {
        var transferBuffer = CreateGPUTransferBuffer(_gpuDevice.Handle, new()
        {
            Usage = usage,
            Size = (uint)size
        });

        if (transferBuffer == nint.Zero)
        {
            throw new NullReferenceException("Transfer buffer is null pointer");
        }

        return new TransferBuffer(_gpuDevice, transferBuffer, size, usage);
    }

    public BorrowedBuffer<TransferBuffer> BorrowUploadBuffer(int size)
    {
        TransferBuffer buffer;
        
        if (_uploadBuffers.Count == 0)
        {
            buffer = CreateTransferBuffer(size, GPUTransferBufferUsage.Upload);
        }
        else
        {
            buffer = _uploadBuffers[0];
            buffer.TryResize(size);
            _uploadBuffers.RemoveAt(0);
        }

        return new BorrowedBuffer<TransferBuffer>(buffer, _uploadBuffers);
    }

    private VertexBuffer CreateVertexBuffer(int size)
    {
        var vertexBuffer = CreateGPUBuffer(_gpuDevice.Handle, new()
        {
            Usage = GPUBufferUsageFlags.Vertex,
            Size = (uint)size
        });

        if (vertexBuffer == nint.Zero)
        {
            throw new NullReferenceException("Transfer buffer is null pointer");
        }

        return new VertexBuffer(_gpuDevice, vertexBuffer, size);
    }

    public BorrowedBuffer<VertexBuffer> BorrowVertexBuffer(int size)
    {
        VertexBuffer buffer;
        
        if (_uploadBuffers.Count == 0)
        {
            buffer = CreateVertexBuffer(size);
        }
        else
        {
            buffer = _vertexBuffers[0];
            buffer.TryResize(size);
            _uploadBuffers.RemoveAt(0);
        }

        return new BorrowedBuffer<VertexBuffer>(buffer, _vertexBuffers);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            RectanglePipeline?.Dispose();
            _uploadBuffers.ForEach(b => b.Dispose());
            _uploadBuffers.Clear();
            _vertexBuffers.ForEach(b => b.Dispose());
            _vertexBuffers.Clear();
            disposedValue = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~Renderer()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
