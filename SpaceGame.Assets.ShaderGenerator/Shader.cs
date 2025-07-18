namespace SpaceGame.Assets.ShaderGenerator;

public record Shader
{
    public required byte[] Code { get; init; }
    public string EntryPoint { get; init; } = "main";
    public required Format Format { get; init; }
    public required Stage Stage { get; init; }
    public int NumSamplers { get; init; } = 0;
    public int NumStorageTextures { get; init; } = 0;
    public int NumStorageBuffers { get; init; } = 0;
    public int NumUniformBuffers { get; init; } = 0;
}
