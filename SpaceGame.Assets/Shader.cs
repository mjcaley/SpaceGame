using System.Collections.Immutable;

namespace SpaceGame.Assets;

public record Shader
{
    public ImmutableArray<byte> Code { get; init; }
    public required ShaderStage Stage { get; init; }
    public required ShaderFormat Format { get; init; }
    public required string EntryPoint { get; init; }
    public required int NumUniform { get; init; }
}
