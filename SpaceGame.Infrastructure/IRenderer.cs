namespace SpaceGame.Infrastructure;

public interface IRenderer : IDisposable
{
    void Draw();
    ICommandBuffer AcquireCommandBuffer();
    IShader CreateShader(ref ShaderCreateInfo shaderCreateInfo);
    IGraphicsPipeline CreatePipeline(ref GraphicsPipelineCreateInfo pipelineCreateInfo);
}
