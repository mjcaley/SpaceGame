using SpaceGame.Core.Components;
using SpaceGame.Infrastructure;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using static SDL3.SDL;

namespace SpaceGame.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ColouredVertex
    {
        public Vector2 Vertex;
        public Vector4 Colour;
    }
    
    public class RenderSystem
    {
        public RenderSystem(IRenderer renderer)
        {
            _renderer = renderer;
        }

        private readonly IRenderer _renderer;
        private readonly List<Tuple<Transform, Components.Rectangle>> _rectangles = [];

        public void Add(Transform transform, Components.Rectangle rectangle)
        {
            _rectangles.Add(Tuple.Create(transform, rectangle));
        }

        public void Clear()
        {
            _rectangles.Clear();
        }

        public void Draw()
        {
            var frame = _renderer.BeginFrame();
            foreach (var (transform, rectangle) in _rectangles)
            {
                var rect = new Infrastructure.Rectangle()
                {
                    Origin = rectangle.Origin,
                    Size = rectangle.Size,
                    Colour = rectangle.Colour
                };
                frame.Draw(rect, new Transformation { Translate = transform.Position });
            }
            frame.End();
        }
    }
}
