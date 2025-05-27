using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public readonly struct ShaderCreateInfo(byte[] code, string entryPoint, GPUShaderFormat shaderFormat, GPUShaderStage stage)
{
    public byte[] Code { get;  } = code;
    public string Entrypoint { get; } = entryPoint;
    public GPUShaderFormat Format { get; } = shaderFormat;
    public GPUShaderStage Stage { get; } = stage;
    public int NumSamplers { get; } = 0;
    public uint NumStorageBuffers { get; } = 0;
    public uint NumStorageTextures { get; } = 0;
    public uint NumUniformBuffers { get; } = 0;

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
