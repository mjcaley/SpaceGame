namespace SpaceGame.Infrastructure;

public class WindowSettings
{
    public const string SectionName = "SpaceGame:Window";

    public string Title { get; set; } = "Space Game";
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 768;
}
