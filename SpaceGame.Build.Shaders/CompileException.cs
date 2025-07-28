namespace SpaceGame.Build.Shaders;

internal class CompileException : Exception
{
    public CompileException() : base() { }
    public CompileException(string message) : base(message) { }
}
