using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public struct ShaderCreateInfo(byte[] code, string entryPoint, GPUShaderFormat shaderFormat, GPUShaderStage stage)
{
    public byte[] Code { get; set; } = code;
    public string Entrypoint { get; set; } = entryPoint;
    public GPUShaderFormat Format { get; set; } = shaderFormat;
    public GPUShaderStage Stage { get; set; } = stage;
    public int NumSamplers { get; set; } = 0;
    public uint NumStorageBuffers { get; set; } = 0;
    public uint NumStorageTextures { get; set; } = 0;
    public uint NumUniformBuffers { get; set; } = 0;

    public GPUShaderCreateInfo ToSDL()
    {
        return new()
        {
            CodeSize = (nuint)Code.Length,
            Code = StructureArrayToPointer(Code),
            Entrypoint = StringToPointer(Entrypoint),
            Format = Format,
            Stage = Stage,
            NumSamplers = (uint)NumSamplers,
            NumStorageBuffers = NumStorageBuffers,
            NumStorageTextures = NumStorageTextures,
            NumUniformBuffers = NumUniformBuffers
        };
    }
}
