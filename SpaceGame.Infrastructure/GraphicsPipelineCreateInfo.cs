using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public readonly struct GraphicsPipelineCreateInfo(IShader vertexShader, IShader fragmentShader)
{
    public IShader VertexShader { get; } = vertexShader;
    public IShader FragmentShader { get; } = fragmentShader;
    public GPUVertexInputState VertexInputState { get; }
}
