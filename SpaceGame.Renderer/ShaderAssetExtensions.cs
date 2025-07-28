using SpaceGame.Assets;
using SpaceGame.SDLWrapper;
using static SDL3.SDL;

public static class ShaderAssetExtensions
{
    public static ShaderCreateInfo ToShaderCreateInfo(this Shader shader) =>
        new()
        {
            Code = [.. shader.Code],
            Entrypoint = shader.EntryPoint,
            Format = shader.Format switch
            {
                ShaderFormat.Spirv => GPUShaderFormat.SPIRV,
                _ => throw new ArgumentException("Unsupported shader format")
            },
            Stage = shader.Stage switch
            {
                ShaderStage.Vertex => GPUShaderStage.Vertex,
                ShaderStage.Fragment => GPUShaderStage.Fragment,
                ShaderStage.Compute => throw new ArgumentException("Compute shaders not supported in this object")
            },
            NumSamplers = 0,

        };
}
