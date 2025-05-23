using SpaceGame.Infrastructure;
using SDL3;
using static SDL3.SDL;
using Silk.NET.Maths;

namespace SpaceGame.Renderer;

public class Renderer : IRenderer, IDisposable
{
    public Renderer(nint window, nint gpuDevice)
    {
        _window = window;
        _gpuDevice = gpuDevice;

        AllocateBuffers();
    }

    private nint _window;
    private nint _gpuDevice;
    
    private nint _transferBuffer = IntPtr.Zero;
    private uint _transferBufferSize = 0;
    private nint _vertexBuffer = IntPtr.Zero;
    private uint _vertexBufferSize = 0;

    private List<Sprite> _sprites = [];
    private bool disposedValue;

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
        ReleaseGPUTransferBuffer(_gpuDevice, _transferBuffer);
        ReleaseGPUBuffer(_gpuDevice, _vertexBuffer);

        var size = Size(_sprites);

        GPUTransferBufferCreateInfo transferCreateInfo = new()
        {
            Usage = GPUTransferBufferUsage.Upload,
            Size = size
        };
        _transferBuffer = CreateGPUTransferBuffer(_gpuDevice, in transferCreateInfo);
        if (_transferBuffer == IntPtr.Zero)
        {
            return;
        }
        _transferBufferSize = size;

        GPUBufferCreateInfo vertexCreateInfo = new()
        {
            Usage = GPUBufferUsageFlags.Vertex,
            Size = size
        };
        _vertexBuffer = CreateGPUBuffer(_gpuDevice, in vertexCreateInfo);
        if (_vertexBuffer == IntPtr.Zero)
        {
            return;
        }
        _vertexBufferSize = size;
    }

    private unsafe void Upload(nint commandBuffer)
    {
        var size = Size(_sprites);
        if (size > _transferBufferSize || size > _vertexBufferSize)
        {
            AllocateBuffers();
        }

        var mappedBufferPtr = MapGPUTransferBuffer(_gpuDevice, _transferBuffer, false);
        if (mappedBufferPtr == IntPtr.Zero)
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
        UnmapGPUTransferBuffer(_gpuDevice, _transferBuffer);

        var copyPass = BeginGPUCopyPass(commandBuffer);
        if (copyPass == IntPtr.Zero)
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
        var commandBuffer = AcquireGPUCommandBuffer(_gpuDevice);
        if (commandBuffer == IntPtr.Zero)
        {
            return;
        }

        Upload(commandBuffer);

        var swapchainSuccess = WaitAndAcquireGPUSwapchainTexture(commandBuffer, _window, out var swapchainTexture, out var swapchainTextureWidth, out var swapchainTextureHeight);
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

            ReleaseGPUTransferBuffer(_gpuDevice, _transferBuffer);
            ReleaseGPUBuffer(_gpuDevice, _vertexBuffer);
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