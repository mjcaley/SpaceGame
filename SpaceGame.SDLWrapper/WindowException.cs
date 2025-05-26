namespace SpaceGame.SDLWrapper;

public class WindowException : Exception
{
    public WindowException() : base() { }

    public WindowException(string message) : base(message) { }
}