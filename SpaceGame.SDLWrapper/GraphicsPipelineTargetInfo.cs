using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public struct GraphicsPipelineTargetInfo
{
    public GraphicsPipelineTargetInfo() { }

    public GPUColorTargetDescription[] ColorTargetDescriptions { get; set; } = [];
    public GPUTextureFormat DepthStencilFormat { get; set; } = default;
    public bool HasDepthStencilTarget { get; set; } = default;

    public static implicit operator GPUGraphicsPipelineTargetInfo(GraphicsPipelineTargetInfo info)
        => new()
        {
            ColorTargetDescriptions = StructureArrayToPointer(info.ColorTargetDescriptions),
            NumColorTargets = (uint)info.ColorTargetDescriptions.Length,
            DepthStencilFormat = info.DepthStencilFormat,
            HasDepthStencilTarget = info.HasDepthStencilTarget == true ? (byte)1 : (byte)0
        };
}
