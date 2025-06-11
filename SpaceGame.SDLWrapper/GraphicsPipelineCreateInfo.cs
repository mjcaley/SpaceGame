using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public struct GraphicsPipelineCreateInfo
{
    public GraphicsPipelineCreateInfo(IShader vertexShader, IShader fragmentShader)
    {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
    }

    public IShader VertexShader { get; set; }
    public IShader FragmentShader { get; set; }
    public VertexInputState VertexInputState { get; set; } = new();
    public GPUPrimitiveType PrimitiveType { get; set; } = default;
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
