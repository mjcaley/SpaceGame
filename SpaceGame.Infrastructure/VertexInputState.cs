using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public struct VertexInputState(GPUVertexBufferDescription[] vertexBufferDescriptions, GPUVertexAttribute[] vertexAttributes)
{
    public GPUVertexBufferDescription[] VertexBufferDescriptions { get; set; } = vertexBufferDescriptions;
    public GPUVertexAttribute[] VertexAttributes { get; set;  } = vertexAttributes;

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
