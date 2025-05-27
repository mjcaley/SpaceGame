using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public readonly struct VertexInputState(GPUVertexBufferDescription[] vertexBufferDescriptions, GPUVertexAttribute[] vertexAttributes)
{
    public GPUVertexBufferDescription[] VertexBufferDescriptions { get; } = vertexBufferDescriptions;
    public GPUVertexAttribute[] VertexAttributes { get; } = vertexAttributes;

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
