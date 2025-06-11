using System.Drawing;
using static SDL3.SDL;

namespace SpaceGame.Infrastructure;

public interface IRenderer : IDisposable
{
    IFrame BeginFrame();
}
