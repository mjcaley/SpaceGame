using SpaceGame.Renderer;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public struct GraphicsPipelineCreateInfo(VertexShader vertexShader, FragmentShader fragmentShader)
{
    public VertexShader VertexShader { get; set; } = vertexShader;
    public FragmentShader FragmentShader { get; set; } = fragmentShader;
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
