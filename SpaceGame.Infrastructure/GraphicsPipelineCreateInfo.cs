using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public struct GraphicsPipelineCreateInfo(IShader vertexShader, IShader fragmentShader)
{
    public IShader VertexShader { get; set; } = vertexShader;
    public IShader FragmentShader { get; set;  } = fragmentShader;
    public VertexInputState VertexInputState { get; set; }
    public GPUPrimitiveType PrimitiveType { get; set; }
    public GPURasterizerState RasterizerState { get; set; } = default;
    public GPUMultisampleState MultisampleState { get; set; } = default;
    public GraphicsPipelineTargetInfo TargetInfo { get; set; } = new();

    public GPUGraphicsPipelineCreateInfo ToSDL()
    {
        return new()
        {
            VertexShader = VertexShader.Handle,
            FragmentShader = FragmentShader.Handle,
            VertexInputState = VertexInputState.ToSDL(),
            PrimitiveType = PrimitiveType,
            RasterizerState = RasterizerState,
            MultisampleState = MultisampleState,
            TargetInfo = TargetInfo.ToSDL()
        };
    }
}
