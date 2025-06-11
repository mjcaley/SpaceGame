namespace SpaceGame.Assets;

public record Shader(byte[] content)
{
    public byte[] Content { get; } = content;
}
