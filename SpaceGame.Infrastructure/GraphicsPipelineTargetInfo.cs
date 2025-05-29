using static SDL3.SDL;

namespace SpaceGame.Infrastructure
{
    public struct GraphicsPipelineTargetInfo
    {
        public GraphicsPipelineTargetInfo() { }

        public GPUColorTargetDescription[] ColorTargetDescriptions { get; set; } = [];
        public GPUTextureFormat DepthStencilFormat { get; set; } = default;
        public bool HasDepthStencilTarget { get; set; } = default;

        public GPUGraphicsPipelineTargetInfo ToSDL()
        {
            var sdl = new GPUGraphicsPipelineTargetInfo()
                {
                    ColorTargetDescriptions = StructureArrayToPointer(ColorTargetDescriptions),
                    NumColorTargets = (uint)ColorTargetDescriptions.Length,
                    DepthStencilFormat = DepthStencilFormat,
                    HasDepthStencilTarget = HasDepthStencilTarget == true ? (byte)1 : (byte)0
                };
            return sdl;
        }
    }
}
