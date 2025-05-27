using SpaceGame.Infrastructure;
using SDL3;
using static SDL3.SDL;
using Silk.NET.Maths;

namespace SpaceGame.Renderer;

public class Renderer : IRenderer, IDisposable
{
    public Renderer(IWindow window, IGpuDevice gpuDevice)
    {
        _window = window;
        _gpuDevice = gpuDevice;

        AllocateBuffers();
        CreatePipeline();
    }

    private IWindow _window;
    private IGpuDevice _gpuDevice;
    
    private nint _transferBuffer = nint.Zero;
    private uint _transferBufferSize = 0;
    private nint _vertexBuffer = nint.Zero;
    private uint _vertexBufferSize = 0;
    private nint _pipeline;

    private List<Sprite> _sprites = [];
    private bool disposedValue;

    private nint _commandBuffer = nint.Zero;

    public unsafe VertexShader CreateVertexShader(byte[] code, string entryPoint)
    {
        fixed (byte* codePtr = code) 
        fixed (char* entryPointPtr = entryPoint)
        {
            GPUShaderCreateInfo info = new()
            {
                CodeSize=(nuint)code.Length,
                Code=(nint)codePtr,
                Entrypoint = (nint)entryPointPtr,
                Format = GPUShaderFormat.SPIRV,
                Stage = GPUShaderStage.Vertex,
                NumSamplers = 0,
                NumStorageBuffers = 0,
                NumStorageTextures = 0,
                NumUniformBuffers = 0
            };
            var shader = CreateGPUShader(_gpuDevice.Handle, info);
            if (shader == nint.Zero)
            {
                throw new NullReferenceException("Vertex shader returned null pointer");
            }
            
            return new VertexShader(_gpuDevice.Handle, shader);
        }
    }
    public unsafe FragmentShader CreateFragmentShader(byte[] code, string entryPoint)
    {
        fixed (byte* codePtr = code) 
        fixed (char* entryPointPtr = entryPoint)
        {
            GPUShaderCreateInfo info = new()
            {
                CodeSize=(nuint)code.Length,
                Code=(nint)codePtr,
                Entrypoint = (nint)entryPointPtr,
                Format = GPUShaderFormat.SPIRV,
                Stage = GPUShaderStage.Fragment,
                NumSamplers = 0,
                NumStorageBuffers = 0,
                NumStorageTextures = 0,
                NumUniformBuffers = 0
            };
            var shader = CreateGPUShader(_gpuDevice.Handle, info);
            if (shader == nint.Zero)
            {
                throw new NullReferenceException("Vertex shader returned null pointer");
            }
            
            return new FragmentShader(_gpuDevice.Handle, shader);
        }
    }

    public SpritePipeline CreateSpritePipeline(VertexShader vertexShader, FragmentShader fragmentShader)
    {
        return new SpritePipeline(_gpuDevice.Handle, 0);
    }

    public ICommandBuffer AcquireCommandBuffer()
    {
        var commandBuffer = AcquireGPUCommandBuffer(_gpuDevice.Handle);
        if (commandBuffer == nint.Zero)
        {
            throw new NullReferenceException("Command buffer is null pointer");
        }

        return new CommandBuffer(this, commandBuffer, _window.Handle);
    }
    
    private static uint Size(List<Sprite> sprites)
    {
        var vertices = sizeof(float) * 2 * 6;
        var colour = sizeof(float) * 4;
        return (uint)(sprites.Count * (vertices + colour));
    }

    public void Add(Sprite sprite)
    {
        _sprites.Add(sprite);
    }

    public void Remove(Sprite sprite)
    {
        _sprites.Remove(sprite);
    }

    private void AllocateBuffers()
    {
        ReleaseGPUTransferBuffer(_gpuDevice.Handle, _transferBuffer);
        ReleaseGPUBuffer(_gpuDevice.Handle, _vertexBuffer);

        var size = Size(_sprites);

        GPUTransferBufferCreateInfo transferCreateInfo = new()
        {
            Usage = GPUTransferBufferUsage.Upload,
            Size = size
        };
        _transferBuffer = CreateGPUTransferBuffer(_gpuDevice.Handle, in transferCreateInfo);
        if (_transferBuffer == nint.Zero)
        {
            return;
        }
        _transferBufferSize = size;

        GPUBufferCreateInfo vertexCreateInfo = new()
        {
            Usage = GPUBufferUsageFlags.Vertex,
            Size = size
        };
        _vertexBuffer = CreateGPUBuffer(_gpuDevice.Handle, in vertexCreateInfo);
        if (_vertexBuffer == nint.Zero)
        {
            return;
        }
        _vertexBufferSize = size;
    }

    private void CreatePipeline()
    {
        GPUGraphicsPipelineCreateInfo info = new()
        {
            
            
        };
    }

    private unsafe void Upload(nint commandBuffer)
    {
        var size = Size(_sprites);
        if (size > _transferBufferSize || size > _vertexBufferSize)
        {
            AllocateBuffers();
        }

        var mappedBufferPtr = MapGPUTransferBuffer(_gpuDevice.Handle, _transferBuffer, false);
        if (mappedBufferPtr == nint.Zero)
        {
            return;
        }
        float* mappedBuffer = (float*)mappedBufferPtr;
        var bufferIndex = 0;
        foreach(var sprite in _sprites)
        {
            foreach (var vertex in sprite.Vertices())
            {
                mappedBuffer[bufferIndex++] = (float)vertex.X;
                mappedBuffer[bufferIndex++] = (float)vertex.Y;
            }
            mappedBuffer[bufferIndex++] = (float)sprite.Colour.X;
            mappedBuffer[bufferIndex++] = (float)sprite.Colour.Y;
            mappedBuffer[bufferIndex++] = (float)sprite.Colour.Z;
            mappedBuffer[bufferIndex++] = (float)sprite.Colour.W;
        }
        UnmapGPUTransferBuffer(_gpuDevice.Handle, _transferBuffer);

        var copyPass = BeginGPUCopyPass(commandBuffer);
        if (copyPass == nint.Zero)
        {
            return;
        }

        GPUTransferBufferLocation source = new()
        {
            TransferBuffer = _transferBuffer,
            Offset = 0,
        };
        GPUBufferRegion destination = new()
        {
            Buffer=_vertexBuffer,
            Offset=0,
            Size=size
        };
        UploadToGPUBuffer(copyPass, source, destination, false);

        EndGPUCopyPass(copyPass);
    }

    public void Draw()
    {
        var commandBuffer = AcquireGPUCommandBuffer(_gpuDevice.Handle);
        if (commandBuffer == nint.Zero)
        {
            return;
        }

        Upload(commandBuffer);

        var swapchainSuccess = WaitAndAcquireGPUSwapchainTexture(commandBuffer, _window.Handle, out var swapchainTexture, out var swapchainTextureWidth, out var swapchainTextureHeight);
        if (!swapchainSuccess)
        {
            CancelGPUCommandBuffer(commandBuffer);
            return;
        }

        var colorTargetInfo = new GPUColorTargetInfo
        {
            Texture=swapchainTexture,
            ClearColor=new FColor { R=1.0f, G=0, B=0, A=1.0f },
            LoadOp=GPULoadOp.Clear,
            StoreOp=GPUStoreOp.Store
        };

        var renderPass = BeginGPURenderPass(commandBuffer, StructureToPointer<GPUColorTargetInfo>(colorTargetInfo), 1, 0);

        GPUBufferBinding binding = new()
        {
            Buffer=_vertexBuffer,
            Offset=0,
        };
        BindGPUVertexBuffers(renderPass, 0, StructureToPointer<GPUBufferBinding>(binding) ,1);
        
        EndGPURenderPass(renderPass);

        SubmitGPUCommandBuffer(commandBuffer);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            ReleaseGPUTransferBuffer(_gpuDevice.Handle, _transferBuffer);
            ReleaseGPUBuffer(_gpuDevice.Handle, _vertexBuffer);
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