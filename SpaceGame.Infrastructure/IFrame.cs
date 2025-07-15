namespace SpaceGame.Infrastructure;

public interface IFrame
{
    void Draw(Rectangle rectangle, Transformation transformation);
    void End();
}