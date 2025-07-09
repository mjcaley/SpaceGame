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

    public static implicit operator GPUGraphicsPipelineCreateInfo(GraphicsPipelineCreateInfo info)
        => new()
        {
            VertexShader = info.VertexShader.Handle,
            FragmentShader = info.FragmentShader.Handle,
            VertexInputState = info.VertexInputState,
            PrimitiveType = info.PrimitiveType,
            RasterizerState = info.RasterizerState,
            MultisampleState = info.MultisampleState,
            TargetInfo = info.TargetInfo
        };
}
