namespace SpaceGame.Infrastructure;

public interface IRenderer
{
    void Draw();
    ICommandBuffer AcquireCommandBuffer();
    IShader CreateShader(ref ShaderCreateInfo shaderCreateInfo);
}
