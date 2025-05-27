using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public struct ShaderCreateInfo
{
    public byte[] Code;
    public string Entrypoint;
    public GPUShaderFormat Format;
    public GPUShaderStage Stage;
    public int NumSamplers;
    public int NumStorageBuffers;
    public int NumStorageTextures;
    public int NumUniformBuffers;
}
