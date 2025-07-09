using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public struct VertexInputState(GPUVertexBufferDescription[] vertexBufferDescriptions, GPUVertexAttribute[] vertexAttributes)
{
    public VertexInputState() : this([], [])
    {
    }

    public GPUVertexBufferDescription[] VertexBufferDescriptions { get; set; } = vertexBufferDescriptions;
    public GPUVertexAttribute[] VertexAttributes { get; set; } = vertexAttributes;

    public static implicit operator GPUVertexInputState(VertexInputState state)
        => new()
        {
            VertexBufferDescriptions = StructureArrayToPointer(state.VertexBufferDescriptions),
            NumVertexBuffers = (uint)state.VertexBufferDescriptions.Length,
            VertexAttributes = StructureArrayToPointer(state.VertexAttributes),
            NumVertexAttributes = (uint)state.VertexAttributes.Length
        };
}
