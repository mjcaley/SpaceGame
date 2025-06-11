using SpaceGame.Infrastructure;
using SpaceGame.SDLWrapper;

namespace SpaceGame.Renderer;

public class Frame(CommandBufferWithSwapchain commandBuffer, Renderer renderer) : IFrame
{
    public void Draw(Rectangle sprite)
    {
        var vertices = new List<ColouredVertex>
        {
            new(new(sprite.Origin.X, sprite.Origin.Y), sprite.Colour),
            new(sprite.Origin with { X = sprite.Origin.X + sprite.Size.X }, sprite.Colour),
            new(new(sprite.Origin.X + sprite.Size.X, sprite.Origin.Y + sprite.Size.Y), sprite.Colour),
            new(new(sprite.Origin.X, sprite.Origin.Y), sprite.Colour),
            new(new(sprite.Origin.X + sprite.Size.X, sprite.Origin.Y + sprite.Size.Y), sprite.Colour),
            new(sprite.Origin with { Y = sprite.Origin.Y + sprite.Size.Y }, sprite.Colour)
        };
        renderer.GetTransferBuffer();

        commandBuffer.WithCopyPass((cmd, pass) =>
        {
            
        });
    }
}