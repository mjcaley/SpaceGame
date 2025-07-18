namespace SpaceGame.Assets.ShaderGenerator;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class ShaderFormatAttribute(Format format) : Attribute
{
    public Format Format => format;
}
