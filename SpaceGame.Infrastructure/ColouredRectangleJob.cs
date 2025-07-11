using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Infrastructure;

public class ColouredRectangleJob
{
    private bool _dirty = false;
    private SortedSet<Rectangle> _rectangles = [];
    private List<short> _indices = [];
    private List<Matrix4x4> _models = [];

    public void Set(IEnumerable<(Rectangle, Transformation)> rectangles)
    {
        var newRectangles = new SortedSet<Rectangle>(rectangles.Select(r => r.Item1));
        if (newRectangles != _rectangles)
        {
            _dirty = true;
            _rectangles = newRectangles;
        }
        _models = [.. rectangles.Select(i => Matrix4x4.CreateTranslation(i.Item2.Translate.X, i.Item2.Translate.Y, 0))];
    }

    public void Checkpoint()
    {
        _dirty = false;
    }
}
