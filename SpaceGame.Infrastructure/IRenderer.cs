namespace SpaceGame.Infrastructure;

public interface IRenderer
{
    void Draw();
    ICommandBuffer AcquireCommandBuffer();
}
