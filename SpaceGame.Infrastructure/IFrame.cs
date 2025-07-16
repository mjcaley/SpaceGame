namespace SpaceGame.Infrastructure;

public interface IFrame
{
    void Enqueue(Rectangle rectangle, Transformation transformation);
    void Draw();
    void End();
}