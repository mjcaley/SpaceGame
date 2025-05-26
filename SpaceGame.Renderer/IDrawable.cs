namespace SpaceGame.Renderer;

public interface IDrawable
{
    VertexShader VertexShader { get; set; }
    FragmentShader FragmentShader { get; set; }
    void Draw(CommandBuffer commandBuffer);
}
