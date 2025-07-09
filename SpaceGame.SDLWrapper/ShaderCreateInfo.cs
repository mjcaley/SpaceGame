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

    public static implicit operator GPUShaderCreateInfo(ShaderCreateInfo info) => new()
    {
        CodeSize = (nuint)info.Code.Length,
        Code = StructureArrayToPointer(info.Code),
        Entrypoint = StringToPointer(info.Entrypoint),
        Format = info.Format,
        Stage = info.Stage,
        NumSamplers = (uint)info.NumSamplers,
        NumStorageBuffers = info.NumStorageBuffers,
        NumStorageTextures = info.NumStorageTextures,
        NumUniformBuffers = info.NumUniformBuffers
    };
}
