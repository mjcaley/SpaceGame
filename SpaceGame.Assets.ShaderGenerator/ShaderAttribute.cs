namespace SpaceGame.Assets.ShaderGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ShaderAttribute(string path, Stage stage, string entryPoint = "main") : Attribute
{
    public string Path => path;
    public Stage Stage => stage;
    public string EntryPoint { get; } = entryPoint;
}
