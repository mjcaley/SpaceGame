using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public struct VertexInputState
{
    public VertexInputState(GPUVertexBufferDescription[] vertexBufferDescriptions, GPUVertexAttribute[] vertexAttributes)
    {
        VertexBufferDescriptions = vertexBufferDescriptions;
        VertexAttributes = vertexAttributes;
    }

    public VertexInputState() : this(Array.Empty<GPUVertexBufferDescription>(), Array.Empty<GPUVertexAttribute>())
    {
    }

    public GPUVertexBufferDescription[] VertexBufferDescriptions { get; set; }
    public GPUVertexAttribute[] VertexAttributes { get; set;  }

    public GPUVertexInputState ToSDL()
    {
        return new()
        {
            VertexBufferDescriptions = StructureArrayToPointer(VertexBufferDescriptions),
            NumVertexBuffers = (uint)VertexBufferDescriptions.Length,
            VertexAttributes = StructureArrayToPointer(VertexAttributes),
            NumVertexAttributes = (uint)VertexAttributes.Length
        };
    }
}
