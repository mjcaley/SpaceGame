using MoonWorks;
using MoonWorks.Graphics;

public class Program : Game
{
    public Program(AppInfo appInfo, WindowCreateInfo windowCreateInfo, FramePacingSettings framePacingSettings, ShaderFormat shaderFormat, bool debug = false)
        : base(appInfo, windowCreateInfo, framePacingSettings, shaderFormat, debug)
    { }

    public static void Main(string[] args)
    {
        var program = new Program(
            new("Michael Caley", "Space Game"),
            new("Space Game", 1024, 768, ScreenMode.Windowed),
            FramePacingSettings.CreateCapped(60, 60),
            ShaderFormat.SPIRV,
            true
        );
        program.Run();
    }

    protected override void Update(TimeSpan delta)
    {
        
    }

    protected override void Draw(double alpha)
    {
        
    }
}
